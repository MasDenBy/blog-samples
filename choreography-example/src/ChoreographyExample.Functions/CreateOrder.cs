using System.Text.Json;
using Azure.Messaging.ServiceBus;
using ChoreographyExample.Functions.Models;
using ChoreographyExample.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace ChoreographyExample.Functions;

public class CreateOrder(
    ServiceBusClient serviceBusClient,
    FakeOrderStore orderStore,
    ILogger<CreateOrder> logger)
{
    private static readonly string[] Products = ["Laptop", "Mouse", "Keyboard", "Monitor", "Headphones"];
    private static readonly string[] Emails = ["alice@example.com", "bob@example.com", "carol@example.com"];

    [Function(nameof(CreateOrder))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        var correlationId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var product = Products[Random.Shared.Next(Products.Length)];
        var email = Emails[Random.Shared.Next(Emails.Length)];
        var amount = Random.Shared.Next(1, 4);

        var orderEvent = new OrderCreated
        {
            CorrelationId = correlationId,
            OrderId = orderId,
            CustomerEmail = email,
            Product = product,
            Amount = amount
        };

        orderStore.Add(orderEvent);

        var sender = serviceBusClient.CreateSender("order-events");
        var message = new ServiceBusMessage(JsonSerializer.SerializeToUtf8Bytes(orderEvent))
        {
            Subject = "OrderCreated"
        };
        message.ApplicationProperties.Add("CorrelationId", correlationId.ToString());
        await sender.SendMessageAsync(message);

        logger.LogInformation(
                "[{CorrelationId}] OrderCreated event published: OrderId={OrderId}",
                orderEvent.CorrelationId,
                orderEvent.OrderId);

        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await response.WriteAsJsonAsync(orderEvent);
        return response;
    }
}
