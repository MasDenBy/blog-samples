using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace AzuriteTestContainers.IntegrationTests;
internal class BlobHelper
{
    private const string ContainerName = "data";

    private readonly BlobContainerClient blobContainerClient;

    public BlobHelper(IServiceProvider serviceProvider)
    {
        var blobServiceClient = serviceProvider.GetService<BlobServiceClient>() 
            ?? throw new ArgumentNullException();

        this.blobContainerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
        this.blobContainerClient.CreateIfNotExists();
    }

    public async Task UploadTextFileAsync(string fileName, string content)
    {
        var blob = this.blobContainerClient.GetBlobClient(fileName);
        using var stream = await blob.OpenWriteAsync(true);
        await stream.WriteAsync(Encoding.UTF8.GetBytes(content));
    }
}
