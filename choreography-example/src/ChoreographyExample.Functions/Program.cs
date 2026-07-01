using Azure.Messaging.ServiceBus;
using ChoreographyExample.Functions.Infrastructure;
using ChoreographyExample.Functions.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var connectionString = Environment.GetEnvironmentVariable("ServiceBusConnection")
    ?? throw new InvalidOperationException("ServiceBusConnection environment variable is not set.");

try
{
   await ServiceBusSetup.EnsureInfrastructureAsync(connectionString);
}
catch (Exception ex)
{
   Console.WriteLine($"Warning: Could not configure Service Bus infrastructure: {ex.Message}");
   Console.WriteLine("Make sure topics/subscriptions exist (they are pre-configured in Config.json for the emulator).");
}

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddSingleton(new ServiceBusClient(connectionString));
        services.AddSingleton<FakeOrderStore>();
        services.AddSingleton<FakeInventoryStore>();
        services.AddSingleton<FakeDeliveryStore>();
    })
    .Build();

await host.RunAsync();
