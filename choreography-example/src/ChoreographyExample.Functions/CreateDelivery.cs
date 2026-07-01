using System.Text.Json;
using Azure.Messaging.ServiceBus;
using ChoreographyExample.Functions.Models;
using ChoreographyExample.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ChoreographyExample.Functions;

public class CreateDelivery(
    ServiceBusClient serviceBusClient,
    FakeDeliveryStore deliveryStore,
    ILogger<CreateDelivery> logger)
{
    [Function(nameof(CreateDelivery))]
    public async Task Run(
        [ServiceBusTrigger("order-events", "inventory-booked-delivery-sub", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message)
    {
        var bookedEvent = JsonSerializer.Deserialize<OrderInventoryBooked>(message.Body.ToStream())!;

        var deliveryId = Guid.NewGuid();
        var deliveryEvent = new DeliveryCreated
        {
            CorrelationId = bookedEvent.CorrelationId,
            OrderId = bookedEvent.OrderId,
            DeliveryId = deliveryId
        };

        deliveryStore.Add(deliveryEvent);

        var sender = serviceBusClient.CreateSender("delivery-events");
        var deliveryMessage = new ServiceBusMessage(JsonSerializer.SerializeToUtf8Bytes(deliveryEvent))
        {
            Subject = "DeliveryCreated"
        };
        deliveryMessage.ApplicationProperties.Add("CorrelationId", bookedEvent.CorrelationId.ToString());
        await sender.SendMessageAsync(deliveryMessage);

        logger.LogInformation(
            "[{CorrelationId}] CreateDelivery: Delivery created for order {OrderId}, DeliveryId={DeliveryId}",
            bookedEvent.CorrelationId,
            bookedEvent.OrderId,
            deliveryId);

        logger.LogInformation(
            "[{CorrelationId}] DeliveryCreated published to delivery-events: OrderId={OrderId}",
            bookedEvent.CorrelationId,
            bookedEvent.OrderId);
    }
}
