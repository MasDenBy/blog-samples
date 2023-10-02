using System.Text.RegularExpressions;
using Testcontainers.Azurite;

namespace AzuriteTestContainers.IntegrationTests;

internal class AzuriteTestContainer
{
    private static readonly Regex BlobUriRegex = new(@"BlobEndpoint=([A-Za-z0-9:\\.\/]*)", RegexOptions.Compiled);

    private readonly AzuriteContainer container;
    private Uri blobUri = null!;

    public AzuriteTestContainer()
    {
        this.container = new AzuriteBuilder()
            .WithImage("mcr.microsoft.com/azure-storage/azurite:latest")
            .WithResourceMapping(new DirectoryInfo("./certs"), "/workspace/")
            .WithCommand("azurite", "--blobHost", "0.0.0.0", "--cert", "/workspace/127.0.0.1.pem", "--key", "/workspace/127.0.0.1-key.pem", "--oauth", "basic")
            .Build();
    }

    public Uri BlobUri => this.blobUri ??= this.GetServiceUri(BlobUriRegex);

    public Task StartAsync() => this.container.StartAsync();
    public ValueTask DisposeAsync() => this.container.DisposeAsync();

    private Uri GetServiceUri(Regex regex)
    {
        var connectionString = this.container.GetConnectionString();

        var matchResult = regex.Match(connectionString);

        var uri = matchResult.Success 
            ? new Uri(matchResult.Groups[1].Value)
            : throw new InvalidOperationException("Cannot retrieve uri from connection string");

        var uriBuilder = new UriBuilder(uri)
        {
            Scheme = Uri.UriSchemeHttps
        };

        return uriBuilder.Uri;
    }
}

