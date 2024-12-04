using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using PencilCase.API.Handlers;
using PencilCase.Shared.DTOs.Requests.Llm;
using PencilCase.Shared.DTOs.Requests.Rag;
using PencilCase.Shared.Models.Notebooks;

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
        
        var agentGroup = llmGroup.MapGroup("agent")
            .WithTags(["LLM", "Agent"]);

        agentGroup.MapPost("invoke", async (
                [FromServices] ILlmApiEndpointsHandler handler, 
                [FromBody] LlmMessageInvokeRequest request) => await handler.InvokeAgentAsync(request))
            .WithName("InvokeAgent")
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Invokes the appropriate LLM agent.",
                Description = "Given a prompt, invokes the appropriate LLM agent with it and returns the answer.", 
            });
        
        var ragGroup = llmGroup.MapGroup("rag")
            .WithTags(["LLM", "RAG"]);
        
        ragGroup.MapPost("search", (
                [FromServices] ILlmApiEndpointsHandler handler, 
                [FromBody] RagSearchRequest request) => handler.SearchForChunksAsync(request))
            .WithName("QueryDocuments")
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Searches for and returns documents given a query.",
                Description = "Given a query, embeds it, then searches in the vector store for similar documents, returns them.", 
            });
        
        ragGroup.MapPost("", async (
                [FromServices] ILlmApiEndpointsHandler handler, 
                [FromBody] List<RagAddRequest> documents) => await handler.AddChunksAsync(documents))
            .WithName("AddDocuments")
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Adds documents to the RAG vector store",
                Description = "Adds documents to an index in the RAG system. Returns information about the created documents. Number of documents must be between 1 and 250", 
            });

        ragGroup.MapDelete("{parentId}/{id}", async (
                [FromServices] ILlmApiEndpointsHandler handler, 
                Guid parentId,
                Guid id) => await handler.DeleteChunksAsync([new RagDeleteRequest { ParentId = parentId, Id = id }]))
            .WithName("DeleteDocuments")
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Erases documents from the RAG vector store.",
                Description = "Deletes the document with the specified ID from the RAG system. If it does not exist, responds with Not Found.", 
            });
    }
}