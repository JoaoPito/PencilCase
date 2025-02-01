using System.Net.Http.Headers;
using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using PencilCase.API.Endpoints;
using PencilCase.API.Handlers;
using PencilCase.Shared.Data.Database;
using PencilCase.LLM.Agents.Providers;
using PencilCase.LLM.Agents.Providers.Gemini;
using PencilCase.LLM.Parser.Services;
using PencilCase.LLM.Parser.Workers;
using PencilCase.LLM.VectorDb;
using PencilCase.LLM.VectorDb.Providers.Pinecone;
using PencilCase.Shared.Models.Telemetry.LLM.Agents;
using PencilCase.Shared.Models.Telemetry.LLM.RAG;
using PencilCase.Telemetry.Data.Database;
using StackExchange.Redis;

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

builder.Services.AddScoped<IBlocksDal, BlocksDAL>();

var blocksDbConnectionString = builder.Configuration.GetConnectionString("ApiDatabase");

builder.Services.AddDbContext<BlocksDbContext>(options => {
    options.UseNpgsql(blocksDbConnectionString)
        .UseLazyLoadingProxies();
});

builder.Services.AddScoped<ILlmApiEndpointsHandler, LlmApiEndpointsHandler>();

builder.Services.AddScoped<PencilCase.Telemetry.Data.Database.DAL<RagOperationEntry>>();
builder.Services.AddScoped<PencilCase.Telemetry.Data.Database.DAL<GenerationResultEntry>>();

var telemetryDbConnectionString = builder.Configuration.GetConnectionString("TelemetryDb");

builder.Services.AddDbContext<TelemetryDbContext>(options =>
{
    options.UseNpgsql(telemetryDbConnectionString);
});

builder.Services.AddHttpClient("GeminiApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["GeminiApi:BaseUrl"]!);
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddScoped<ILlmApiService, GeminiApiService>();
builder.Services.AddScoped<IRagService, PineconeService>();

// RAG File Parser

// Add HttpClient for LLM API
builder.Services.AddHttpClient("LLMApi-FileParser", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["LlmApi:BaseUrl"]!);
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
    client.Timeout = TimeSpan.FromHours(2); // THIS IS TEMPORARY
});
// Add redis
var redisUrl = builder.Configuration["ParserBroker:Url"] ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisUrl, options =>
    {
        options.AbortOnConnectFail = false;
    })
);
// Add ParserConsumerService background service
builder.Services.AddHostedService<ParserConsumerService>();
// Add IParserProducerService scoped service
builder.Services.AddScoped<IParserProducerService, ParserProducerService>();

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

// RAG file parser endpoints
app.AddV1SourceEndpoints();

app.Run();
