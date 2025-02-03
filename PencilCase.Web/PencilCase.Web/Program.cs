using MudBlazor.Services;
using PencilCase.Web.Client.Services.Blocks;
using PencilCase.Web.Client.Services.LLM;
using PencilCase.Web.Client.Services.Sources;
using PencilCase.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(PencilCase.Web.Client._Imports).Assembly);

app.Run();