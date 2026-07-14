using Azure.Messaging.ServiceBus.Administration;

namespace OrderProcess.Functions.Infrastructure;

public static class ServiceBusSetup
{
    public static async Task EnsureInfrastructureAsync(string connectionString)
    {
        var adminClient = new ServiceBusAdministrationClient(connectionString);

        foreach (var topic in new[] { "orchestrator-commands", "orchestrator-events" })
        {
            if (!await adminClient.TopicExistsAsync(topic))
                await adminClient.CreateTopicAsync(topic);
        }

        await CreateSubscriptionAsync(adminClient, "orchestrator-commands", "book-inventory-sub", "sys.Label = 'BookInventory'");
        await CreateSubscriptionAsync(adminClient, "orchestrator-commands", "send-email-sub", "sys.Label = 'SendEmail'");
        await CreateSubscriptionAsync(adminClient, "orchestrator-commands", "create-delivery-sub", "sys.Label = 'CreateDelivery'");

        await CreateSubscriptionAsync(adminClient, "orchestrator-events", "orchestrator-sub", null);

        Console.WriteLine("Service Bus infrastructure setup complete.");
    }

    private static async Task CreateSubscriptionAsync(
        ServiceBusAdministrationClient adminClient,
        string topicName,
        string subscriptionName,
        string? sqlFilter)
    {
        if (await adminClient.SubscriptionExistsAsync(topicName, subscriptionName))
            return;

        var options = new CreateSubscriptionOptions(topicName, subscriptionName)
        {
            DefaultMessageTimeToLive = TimeSpan.FromDays(1),
            LockDuration = TimeSpan.FromMinutes(1)
        };

        if (sqlFilter is null)
        {
            await adminClient.CreateSubscriptionAsync(options);
        }
        else
        {
            var rule = new CreateRuleOptions
            {
                Name = "event-type-filter",
                Filter = new SqlRuleFilter(sqlFilter)
            };
            await adminClient.CreateSubscriptionAsync(options, rule);
        }
    }
}
