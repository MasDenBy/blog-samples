using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace AzuriteTestContainers.IntegrationTests;

public class ApiWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly AzuriteTestContainer azuriteTestContainer;

    public ApiWebApplicationFactory()
    {
         this.azuriteTestContainer = new AzuriteTestContainer();
    }

    public async Task InitializeAsync()
    {
        await this.azuriteTestContainer.StartAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddSingleton(new BlobServiceClient(azuriteTestContainer.BlobUri, new DefaultAzureCredential()));
        });
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await this.azuriteTestContainer.DisposeAsync();
    }
}
