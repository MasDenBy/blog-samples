var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

const string configFileName = "/configuration/step.config";

app.MapGet("/api/state", () =>
{
    if(File.Exists(configFileName))
    {
        return Results.Ok(File.ReadAllText(configFileName));
    }

    return Results.BadRequest();
});

app.MapGet("/api/health", () =>
{
    return Results.Ok(Environment.MachineName);
});

app.Run();
