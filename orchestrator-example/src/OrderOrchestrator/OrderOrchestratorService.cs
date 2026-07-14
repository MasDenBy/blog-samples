using System.Collections.Concurrent;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OrderOrchestrator;

public class OrderOrchestratorService : BackgroundService
{
    private readonly ServiceBusProcessor _eventProcessor;
    private readonly ServiceBusSender _commandSender;
    private readonly ILogger<OrderOrchestratorService> _logger;
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<object>> _pendingSteps = new();

    public OrderOrchestratorService(
        ServiceBusClient serviceBusClient,
        ILogger<OrderOrchestratorService> logger)
    {
        _eventProcessor = serviceBusClient.CreateProcessor("orchestrator-events", "orchestrator-sub");
        _commandSender = serviceBusClient.CreateSender("orchestrator-commands");
        _logger = logger;

        _eventProcessor.ProcessMessageAsync += OnEventReceived;
        _eventProcessor.ProcessErrorAsync += OnError;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _eventProcessor.StartProcessingAsync(stoppingToken);
        _logger.LogInformation("OrderOrchestrator started");

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
        }

        await _eventProcessor.StopProcessingAsync();
    }

    private async Task OnEventReceived(ProcessMessageEventArgs args)
    {
        var message = args.Message;
        var correlationId = Guid.Parse(message.CorrelationId);

        if (message.Subject == "OrderPlaced")
        {
            var orderPlaced = JsonSerializer.Deserialize<OrderPlaced>(message.Body.ToStream())!;
            _logger.LogInformation(
                "[{CorrelationId}] Orchestrator: OrderPlaced received for OrderId={OrderId}",
                correlationId, orderPlaced.OrderId);

            _ = ProcessOrderAsync(orderPlaced, args.CancellationToken);
        }
        else
        {
            if (_pendingSteps.TryRemove(correlationId, out var tcs))
                tcs.TrySetResult(DeserializeResult(message));
        }

        await args.CompleteMessageAsync(message);
    }

    private async Task ProcessOrderAsync(OrderPlaced order, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation(
                "[{CorrelationId}] Orchestrator: Starting orchestration for OrderId={OrderId}",
                order.CorrelationId, order.OrderId);

            var bookResult = await SendAndWaitAsync<BookInventoryResult>(
                new BookInventoryCommand
                {
                    CorrelationId = order.CorrelationId,
                    OrderId = order.OrderId,
                    Product = order.Product,
                    Amount = order.Amount
                },
                "BookInventory",
                order.CorrelationId);

            if (!bookResult.Success)
            {
                _logger.LogWarning(
                    "[{CorrelationId}] Orchestrator: Step 1 (BookInventory) failed - {Reason}",
                    order.CorrelationId, bookResult.FailureReason);
                return;
            }

            _logger.LogInformation(
                "[{CorrelationId}] Orchestrator: Step 1 (BookInventory) completed",
                order.CorrelationId);

            await SendAndWaitAsync<SendEmailResult>(
                new SendEmailCommand
                {
                    CorrelationId = order.CorrelationId,
                    OrderId = order.OrderId,
                    CustomerEmail = order.CustomerEmail
                },
                "SendEmail",
                order.CorrelationId);

            _logger.LogInformation(
                "[{CorrelationId}] Orchestrator: Step 2 (SendEmail) completed",
                order.CorrelationId);

            await SendAndWaitAsync<CreateDeliveryResult>(
                new CreateDeliveryCommand
                {
                    CorrelationId = order.CorrelationId,
                    OrderId = order.OrderId
                },
                "CreateDelivery",
                order.CorrelationId);

            _logger.LogInformation(
                "[{CorrelationId}] Orchestrator: Step 3 (CreateDelivery) completed",
                order.CorrelationId);

            _logger.LogInformation(
                "[{CorrelationId}] Orchestrator: Order orchestration completed successfully",
                order.CorrelationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[{CorrelationId}] Orchestrator: Order orchestration failed",
                order.CorrelationId);
        }
    }

    private async Task<T> SendAndWaitAsync<T>(object command, string commandType, Guid correlationId)
    {
        var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingSteps[correlationId] = tcs;

        var message = new ServiceBusMessage(JsonSerializer.SerializeToUtf8Bytes(command))
        {
            Subject = commandType,
            CorrelationId = correlationId.ToString()
        };
        await _commandSender.SendMessageAsync(message);

        var result = await tcs.Task;
        return (T)result;
    }

    private object DeserializeResult(ServiceBusReceivedMessage message)
    {
        return message.Subject switch
        {
            "BookInventoryResult" => JsonSerializer.Deserialize<BookInventoryResult>(message.Body.ToStream())!,
            "SendEmailResult" => JsonSerializer.Deserialize<SendEmailResult>(message.Body.ToStream())!,
            "CreateDeliveryResult" => JsonSerializer.Deserialize<CreateDeliveryResult>(message.Body.ToStream())!,
            _ => throw new InvalidOperationException($"Unknown event type: {message.Subject}")
        };
    }

    private Task OnError(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Orchestrator processor error");
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _commandSender.DisposeAsync();
        await _eventProcessor.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}
