using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using PencilCase.Web.Client.Services.Blocks;
using PencilCase.Web.Client.Services.LLM;
using PencilCase.Web.Client.Services.Sources;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddMudServices();

builder.Services.AddHttpClient("BlocksAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BlocksAPI:url"]!);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient("LlmAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["LlmAPI:url"]!);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient("SourcesAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["SourcesAPI:url"]!);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddTransient<IBlocksApi, BlocksApi>();
builder.Services.AddTransient<ILlmApi, LlmApi>();
builder.Services.AddTransient<BlockMapper>();
builder.Services.AddTransient<ISourcesApi, SourcesApi>();

await builder.Build().RunAsync();