using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using PencilCase.Web;
using MudBlazor.Services;
using PencilCase.Web.Services;
using PencilCase.Shared.Files.FileExporters;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["APIServer:url"]!);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
builder.Services.AddTransient<FragmentApi>();
builder.Services.AddTransient<MarkdownExporter>();
builder.Services.AddTransient<PdfExporter>();
builder.Services.AddTransient<PdfAttributes, PdfDefaultAttributes>();

builder.Services.AddMudServices();
builder.Services.AddMudMarkdownServices();

await builder.Build().RunAsync();
