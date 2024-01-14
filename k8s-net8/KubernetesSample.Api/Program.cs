var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/hello", () => $"Hello from {Environment.MachineName}");

app.Run();
