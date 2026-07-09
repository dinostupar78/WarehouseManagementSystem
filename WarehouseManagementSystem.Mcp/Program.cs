using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateEmptyApplicationBuilder(settings: null);

builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

builder.Services.AddHttpClient("WmsApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:44377");
});

var app = builder.Build();

await app.RunAsync();