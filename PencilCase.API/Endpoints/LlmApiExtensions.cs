using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using PencilCase.LLM.Agents;
using PencilCase.LLM.Providers;
using PencilCase.LLM.Requests;
using PencilCase.Shared.Models.LLM.Agents;
using PencilCase.Shared.Models.LLM.RAG;

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
            .WithTags(["LLM", "Agent"]);;

        agentGroup.MapPost("invoke", async (
                [FromServices] ILlmApiService llmApiService, 
                [FromBody] List<LlmMessage> messages) =>
            {
                return Results.Ok(await llmApiService.GenerateContent(messages));
            })
            .WithName("InvokeAgent")
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Invokes the appropriate LLM agent.",
                Description = "Given a prompt, invokes the appropriate LLM agent with it and returns the answer.", 
            });
        
        var ragGroup = llmGroup.MapGroup("rag")
            .WithTags(["LLM", "RAG"]);;
        
        ragGroup.MapPost("search", async (
                [FromServices] IRagService ragService, 
                [FromBody] QueryRequest request) =>
            {
                if(request.NResults <= 0)
                    return Results.BadRequest();
                
                var docs = await ragService.GetDocsByQuery(
                    request.Query, 
                    request.ParentIds, 
                    request.NResults ?? 3);
                return Results.Ok(docs);
            })
            .WithName("QueryDocuments")
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Searches for and returns documents given a query.",
                Description = "Given a query, embeds it, then searches in the vector store for similar documents, returns them.", 
            });
        
        ragGroup.MapPost("", async (
                [FromServices] IRagService ragService, 
                [FromBody] List<RagDocument> documents) =>
            {
                await ragService.AddDocs(documents);
                return Results.Created();
            })
            .WithName("AddDocuments")
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Adds documents to the RAG vector store",
                Description = "Adds documents to an index in the RAG system. Returns information about the created documents.", 
            });

        ragGroup.MapDelete("{parentId}/{id}", async (
                [FromServices] IRagService ragService, 
                Guid parentId,
                Guid id) =>
            {
                var doc = new RagDocument() { Id = id, ParentId = parentId };
                
                try
                {
                    await ragService.DeleteSingleDoc(doc);
                }
                catch (ArgumentException)
                {
                    return Results.NotFound();
                }

                return Results.NoContent();
            })
            .WithName("DeleteDocuments")
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Erases documents from the RAG vector store.",
                Description = "Deletes the document with the specified ID from the RAG system. If it does not exist, responds with Not Found.", 
            });
        
        ragGroup.MapPut("", async (
                [FromServices] IRagService ragService, 
                [FromBody] RagDocument doc) =>
            {
                await ragService.UpdateDoc(doc);
                return Results.Ok();
            })
            .WithName("UpdateDocuments")
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Updates documents from the RAG vector store.",
                Description = "Given a document with an already existing Id, updates all content of it.", 
            });
    }
}