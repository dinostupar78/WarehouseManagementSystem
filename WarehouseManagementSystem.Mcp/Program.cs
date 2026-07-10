using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), optional: false, reloadOnChange: false);

var wmsApiBaseUrl = builder.Configuration["WmsApi:BaseUrl"];

if (!Uri.TryCreate(wmsApiBaseUrl, UriKind.Absolute, out var wmsApiUri))
{
    throw new InvalidOperationException(
        "Set WmsApi:BaseUrl in WarehouseManagementSystem.Mcp/appsettings.json to the running Web API URL.");
}

builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

builder.Services.AddHttpClient("WmsApi", client =>
{
    client.BaseAddress = wmsApiUri;
});

var app = builder.Build();

await app.RunAsync();
