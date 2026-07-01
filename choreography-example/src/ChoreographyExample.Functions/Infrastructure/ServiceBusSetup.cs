using Azure.Messaging.ServiceBus.Administration;

namespace ChoreographyExample.Functions.Infrastructure;

public static class ServiceBusSetup
{
    public static async Task EnsureInfrastructureAsync(string connectionString)
    {
        var adminClient = new ServiceBusAdministrationClient(connectionString);

        foreach (var topic in new[] { "order-events", "delivery-events" })
        {
            if (!await adminClient.TopicExistsAsync(topic))
                await adminClient.CreateTopicAsync(topic);
        }

        await CreateSubscriptionAsync(adminClient, "order-events", "order-created-email-sub", "sys.Label = 'OrderCreated'");
        await CreateSubscriptionAsync(adminClient, "order-events", "order-created-inventory-sub", "sys.Label = 'OrderCreated'");
        await CreateSubscriptionAsync(adminClient, "order-events", "inventory-booked-delivery-sub", "sys.Label = 'OrderInventoryBooked'");

        Console.WriteLine("Service Bus infrastructure setup complete.");
    }

    private static async Task CreateSubscriptionAsync(
        ServiceBusAdministrationClient adminClient,
        string topicName,
        string subscriptionName,
        string sqlFilter)
    {
        if (await adminClient.SubscriptionExistsAsync(topicName, subscriptionName))
            return;

        var options = new CreateSubscriptionOptions(topicName, subscriptionName)
        {
            DefaultMessageTimeToLive = TimeSpan.FromDays(1),
            LockDuration = TimeSpan.FromMinutes(1)
        };

        var rule = new CreateRuleOptions
        {
            Name = "event-type-filter",
            Filter = new SqlRuleFilter(sqlFilter)
        };

        await adminClient.CreateSubscriptionAsync(options, rule);
    }
}
