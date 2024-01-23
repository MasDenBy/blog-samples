using System.Diagnostics;
using KubernetesSample.Web.HttpClients;
using KubernetesSample.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace KubernetesSample.Web.Controllers;
public class HomeController(IApiHttpClient apiHttpClient) : Controller
{
    private readonly IApiHttpClient apiHttpClient = apiHttpClient;

    public async Task<IActionResult> Index()
    {
        this.ViewData["Hello"] = await this.apiHttpClient.GetHelloWorldAsync();

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
