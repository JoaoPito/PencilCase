using System.Net.Http.Headers;
using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using PencilCase.API.Endpoints;
using PencilCase.Shared.Data.Database;
using PencilCase.LLM.Agents;
using PencilCase.LLM.Providers;
using PencilCase.LLM.Providers.Pinecone;
using PencilCase.LLM.Agents.Providers;
using PencilCase.LLM.Agents.Providers.Gemini;
using PencilCase.Shared.Models.Notebooks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiVersioning(options => {
    options.DefaultApiVersion = new ApiVersion(1);
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"));
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'V";
        options.SubstituteApiVersionInUrl = true;
    });

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<DAL<Block>>();

builder.Services.AddHttpClient("GeminiApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["GeminiApi:BaseUrl"]!);
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddScoped<ILlmApiService, GeminiApiService>();
builder.Services.AddScoped<IRagService, PineconeService>();

var blocksDbConnectionString = builder.Configuration.GetConnectionString("ApiDatabase");

builder.Services.AddDbContext<BlocksDbContext>(options => {
    options.UseNpgsql(blocksDbConnectionString)
        .UseLazyLoadingProxies();
});

builder.Services.AddCors(
    options => options.AddPolicy(
        "wasm-frontend",
        policy => policy.WithOrigins([builder.Configuration["BackendUrl"] ?? "http://localhost:5147",
                builder.Configuration["FrontendUrl"] ?? "http://localhost:5096"])
            .AllowAnyMethod()
            .SetIsOriginAllowed(pol => true)
            .AllowAnyHeader()
            .AllowCredentials()));

var app = builder.Build();

app.UseCors("wasm-frontend");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Blocks endpoints
app.AddBlocksEndpointsV1();

// LLM endpoints
app.AddLlmApiEndpoints();

app.Run();
