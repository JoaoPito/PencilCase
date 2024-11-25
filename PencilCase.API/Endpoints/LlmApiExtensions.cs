using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using PencilCase.LLM.Agents;
using PencilCase.LLM.Agents.Models;

namespace PencilCase.API.Endpoints;

public static class LlmApiExtensions
{
    public static void AddLlmApiEndpoints(this WebApplication app)
    {
        ApiVersionSet apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var group = app.MapGroup("api/v{version:apiVersion}/llm")
            .WithApiVersionSet(apiVersionSet)
            .WithTags("LLM");

        group.MapPost("invoke", async ([FromServices] ILlmApiService llmApiService, [FromBody] List<Message> messages) =>
        {
            return await llmApiService.GenerateContent(messages);
        });
    }
}