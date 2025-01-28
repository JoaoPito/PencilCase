using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using PencilCase.Web;
using MudBlazor.Services;
using PencilCase.Web.Services;
using PencilCase.Shared.Files.FileExporters;
using PencilCase.Web.Services.LLM;
using PencilCase.Web.Services.Notebooks;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["APIServer:url"]!);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

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

builder.Services.AddTransient<FragmentApi>();
builder.Services.AddTransient<IBlocksApi, BlocksApi>();
builder.Services.AddTransient<ILlmApi, LlmApi>();
builder.Services.AddTransient<BlockMapper>();
builder.Services.AddTransient<ISourcesApi, SourcesApi>();

builder.Services.AddTransient<MarkdownExporter>();

builder.Services.AddMudServices();
builder.Services.AddMudMarkdownServices();

await builder.Build().RunAsync();
