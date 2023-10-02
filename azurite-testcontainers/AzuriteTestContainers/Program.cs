using Azure.Identity;
using Azure.Storage.Blobs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(x =>
{
    return new BlobServiceClient(new Uri("https://127.0.0.1:10000/devstoreaccount1"), new DefaultAzureCredential());
});

var app = builder.Build();

app.MapGet("/api/file", async (BlobServiceClient blobServiceClient) =>
{
    var containerClient = blobServiceClient.GetBlobContainerClient("data");
    var blobClient = containerClient.GetBlobClient("file.txt");

    var response = await blobClient.DownloadContentAsync();
    return Results.Text(response.Value.Content);
});

app.Run();

public partial class Program { }