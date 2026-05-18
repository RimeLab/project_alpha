using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppMetadata>(builder.Configuration.GetSection("AppMetadata"));

var app = builder.Build();

app.MapGet("/", () => new { message = "Welcome" });
app.MapGet("/metadata", (IOptions<AppMetadata> metadata) => metadata.Value);

app.Run();
