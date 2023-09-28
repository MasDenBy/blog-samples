using Testcontainers.Azurite;

namespace AzuriteTestContainers.IntegrationTests;

internal class AzuriteTestContainer
{
    private readonly AzuriteContainer container;

    public AzuriteTestContainer()
    {
        this.container = new AzuriteBuilder()
            .WithImage("mcr.microsoft.com/azure-storage/azurite:latest")
            .Build();
    }

    public string ConnectionString => container.GetConnectionString();

    public Task StartAsync() => this.container.StartAsync();
    public ValueTask DisposeAsync() => this.container.DisposeAsync();
}
