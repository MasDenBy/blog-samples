
namespace KubernetesSample.Web.HttpClients;

public interface IApiHttpClient
{
    Task<string> GetHelloWorldAsync();
}