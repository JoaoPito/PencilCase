using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
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

        var llmGroup = app.MapGroup("api/v{version:apiVersion}/llm")
            .WithApiVersionSet(apiVersionSet)
            .WithTags("LLM");

        group.MapPost("invoke", async ([FromServices] ILlmApiService llmApiService, [FromBody] List<Message> messages) =>
        {
            return await llmApiService.GenerateContent(messages);
        });
        
        var agentGroup = llmGroup.MapGroup("agent")
            .WithTags(["LLM", "Agent"]);;

        agentGroup.MapPost("invoke", async ([FromServices] ILlmApiService llmApiService, [FromBody] List<Message> messages) =>
            {
                return await llmApiService.GenerateContent(messages);
            })
            .WithName("InvokeAgent")
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Invokes the appropriate LLM agent.",
                Description = "Given a prompt, invokes the appropriate LLM agent with it and returns the answer.", 
            });
        
    }
}