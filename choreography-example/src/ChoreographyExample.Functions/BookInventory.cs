using System.Text.Json;
using Azure.Messaging.ServiceBus;
using ChoreographyExample.Functions.Models;
using ChoreographyExample.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ChoreographyExample.Functions;

public class BookInventory(
    ServiceBusClient serviceBusClient,
    FakeInventoryStore inventoryStore,
    ILogger<BookInventory> logger)
{
    [Function(nameof(BookInventory))]
    public async Task Run(
        [ServiceBusTrigger("order-events", "order-created-inventory-sub", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message)
    {
        var orderEvent = JsonSerializer.Deserialize<OrderCreated>(message.Body.ToStream())!;

        if (inventoryStore.TryBook(orderEvent.Product, orderEvent.Amount))
        {
            logger.LogInformation(
                "[{CorrelationId}] BookInventory: Inventory booked for order {OrderId} (product: {Product}, qty: {Amount})",
                orderEvent.CorrelationId,
                orderEvent.OrderId,
                orderEvent.Product,
                orderEvent.Amount);

            var bookedEvent = new OrderInventoryBooked
            {
                CorrelationId = orderEvent.CorrelationId,
                OrderId = orderEvent.OrderId,
                CustomerEmail = orderEvent.CustomerEmail,
                Product = orderEvent.Product,
                Amount = orderEvent.Amount
            };

            var sender = serviceBusClient.CreateSender("order-events");
            var bookedMessage = new ServiceBusMessage(JsonSerializer.SerializeToUtf8Bytes(bookedEvent))
            {
                Subject = "OrderInventoryBooked"
            };
            bookedMessage.ApplicationProperties.Add("CorrelationId", orderEvent.CorrelationId.ToString());
            await sender.SendMessageAsync(bookedMessage);

            logger.LogInformation(
                "[{CorrelationId}] OrderInventoryBooked published: OrderId={OrderId}",
                orderEvent.CorrelationId,
                orderEvent.OrderId);
        }
        else
        {
            logger.LogWarning(
                "[{CorrelationId}] BookInventory: Failed to book inventory for order {OrderId} (product: {Product}, qty: {Amount}) - insufficient stock",
                orderEvent.CorrelationId,
                orderEvent.OrderId,
                orderEvent.Product,
                orderEvent.Amount);
        }
    }
}
