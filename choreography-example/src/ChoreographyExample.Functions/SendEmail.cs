using System.Text.Json;
using Azure.Messaging.ServiceBus;
using ChoreographyExample.Functions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ChoreographyExample.Functions;

public class SendEmail(ILogger<SendEmail> logger)
{
    [Function(nameof(SendEmail))]
    public async Task Run(
        [ServiceBusTrigger("order-events", "order-created-email-sub", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message)
    {
        var orderEvent = JsonSerializer.Deserialize<OrderCreated>(message.Body.ToStream())!;

        logger.LogInformation(
            "[{CorrelationId}] SendEmail: Email sent to {Email} for order {OrderId}",
            orderEvent.CorrelationId,
            orderEvent.CustomerEmail,
            orderEvent.OrderId);

        await Task.CompletedTask;
    }
}
