namespace AzuriteTestContainers.IntegrationTests;

public class ProgramTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient httpClient;
    private readonly BlobHelper blobHelper;

    public ProgramTests(ApiWebApplicationFactory factory)
    {
        this.httpClient = factory.CreateClient();
        this.blobHelper = new BlobHelper(factory.Services);
    }

    [Fact]
    public async Task Get_ShouldResponseWithTextFileContent()
    {
        // Arrange
        const string content = "The content of the file for test";
        await this.blobHelper.UploadTextFileAsync("file.txt", content);

        // Act
        var response = await this.httpClient.GetAsync("/api/file");
        response.EnsureSuccessStatusCode();

        var actualContent = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(content, actualContent);
    }
}
