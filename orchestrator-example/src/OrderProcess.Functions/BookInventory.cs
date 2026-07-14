using System.Text.Json;
using Azure.Messaging.ServiceBus;
using OrderOrchestrator;
using OrderProcess.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace OrderProcess.Functions;

public class BookInventory(
    ServiceBusClient serviceBusClient,
    FakeInventoryStore inventoryStore,
    ILogger<BookInventory> logger)
{
    private readonly ServiceBusSender _resultSender = serviceBusClient.CreateSender("orchestrator-events");

    [Function(nameof(BookInventory))]
    public async Task Run(
        [ServiceBusTrigger("orchestrator-commands", "book-inventory-sub", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message)
    {
        var command = JsonSerializer.Deserialize<BookInventoryCommand>(message.Body.ToStream())!;

        var success = inventoryStore.TryBook(command.Product, command.Amount);

        var result = new BookInventoryResult
        {
            CorrelationId = command.CorrelationId,
            Success = success,
            FailureReason = success ? null : $"Insufficient stock for {command.Product}"
        };

        var resultMessage = new ServiceBusMessage(JsonSerializer.SerializeToUtf8Bytes(result))
        {
            Subject = "BookInventoryResult",
            CorrelationId = command.CorrelationId.ToString()
        };
        await _resultSender.SendMessageAsync(resultMessage);

        if (success)
        {
            logger.LogInformation(
                "[{CorrelationId}] BookInventory: Inventory booked for order {OrderId} (product: {Product}, qty: {Amount})",
                command.CorrelationId, command.OrderId, command.Product, command.Amount);
        }
        else
        {
            logger.LogWarning(
                "[{CorrelationId}] BookInventory: Failed to book inventory for order {OrderId} (product: {Product}, qty: {Amount}) - insufficient stock",
                command.CorrelationId, command.OrderId, command.Product, command.Amount);
        }
    }
}
