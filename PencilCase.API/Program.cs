using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using PencilCase.API.Extensions;
using PencilCase.Shared.Data.Database;
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

var connectionString = builder.Configuration.GetConnectionString("ApiDatabase");

builder.Services.AddDbContext<BlocksDbContext>(options => {
    options.UseNpgsql(connectionString);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Blocks endpoints
app.AddBlocksEndpointsV1();

// BlockProperties endpoints

app.Run();
