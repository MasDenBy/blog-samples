using System.Text.Json;
using Azure.Messaging.ServiceBus;
using OrderProcess.Functions.Models;
using OrderOrchestrator;
using OrderProcess.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace OrderProcess.Functions;

public class CreateDelivery(
    ServiceBusClient serviceBusClient,
    FakeDeliveryStore deliveryStore,
    ILogger<CreateDelivery> logger)
{
    private readonly ServiceBusSender _resultSender = serviceBusClient.CreateSender("orchestrator-events");

    [Function(nameof(CreateDelivery))]
    public async Task Run(
        [ServiceBusTrigger("orchestrator-commands", "create-delivery-sub", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message)
    {
        var command = JsonSerializer.Deserialize<CreateDeliveryCommand>(message.Body.ToStream())!;

        var deliveryId = Guid.NewGuid();
        var deliveryEvent = new DeliveryCreated
        {
            CorrelationId = command.CorrelationId,
            OrderId = command.OrderId,
            DeliveryId = deliveryId
        };

        deliveryStore.Add(deliveryEvent);

        var result = new CreateDeliveryResult
        {
            CorrelationId = command.CorrelationId,
            OrderId = command.OrderId,
            DeliveryId = deliveryId,
            Success = true
        };

        var resultMessage = new ServiceBusMessage(JsonSerializer.SerializeToUtf8Bytes(result))
        {
            Subject = "CreateDeliveryResult",
            CorrelationId = command.CorrelationId.ToString()
        };
        await _resultSender.SendMessageAsync(resultMessage);

        logger.LogInformation(
            "[{CorrelationId}] CreateDelivery: Delivery created for order {OrderId}, DeliveryId={DeliveryId}",
            command.CorrelationId, command.OrderId, deliveryId);
    }
}
