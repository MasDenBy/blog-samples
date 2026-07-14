using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderOrchestrator;

var host = Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false);
    })
    .ConfigureServices((context, services) =>
    {
        var connectionString = context.Configuration["ServiceBusConnection"]
            ?? throw new InvalidOperationException("ServiceBusConnection is not configured.");

        services.AddSingleton(new ServiceBusClient(connectionString));
        services.AddHostedService<OrderOrchestratorService>();
    })
    .Build();

await host.RunAsync();
