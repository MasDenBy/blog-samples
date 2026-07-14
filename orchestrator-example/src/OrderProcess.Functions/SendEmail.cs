using System.Text.Json;
using Azure.Messaging.ServiceBus;
using OrderOrchestrator;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace OrderProcess.Functions;

public class SendEmail(
    ServiceBusClient serviceBusClient,
    ILogger<SendEmail> logger)
{
    private readonly ServiceBusSender _resultSender = serviceBusClient.CreateSender("orchestrator-events");

    [Function(nameof(SendEmail))]
    public async Task Run(
        [ServiceBusTrigger("orchestrator-commands", "send-email-sub", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message)
    {
        var command = JsonSerializer.Deserialize<SendEmailCommand>(message.Body.ToStream())!;

        logger.LogInformation(
            "[{CorrelationId}] SendEmail: Email sent to {Email} for order {OrderId}",
            command.CorrelationId, command.CustomerEmail, command.OrderId);

        var result = new SendEmailResult
        {
            CorrelationId = command.CorrelationId,
            Success = true
        };

        var resultMessage = new ServiceBusMessage(JsonSerializer.SerializeToUtf8Bytes(result))
        {
            Subject = "SendEmailResult",
            CorrelationId = command.CorrelationId.ToString()
        };
        await _resultSender.SendMessageAsync(resultMessage);
    }
}
