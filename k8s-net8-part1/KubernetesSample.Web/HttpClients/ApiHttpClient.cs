namespace KubernetesSample.Web.HttpClients;

public class ApiHttpClient(HttpClient httpClient) : IApiHttpClient
{
    private readonly HttpClient httpClient = httpClient;

    public Task<string> GetHelloWorldAsync() => this.httpClient.GetStringAsync("/hello");
}
