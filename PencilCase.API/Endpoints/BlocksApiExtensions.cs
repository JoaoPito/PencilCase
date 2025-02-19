using System.Security.Claims;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using PencilCase.API.Handlers;
using PencilCase.Identity.Models;
using PencilCase.Shared.DTOs.Requests.Blocks;

namespace PencilCase.API.Endpoints;

public static class BlocksExtensions
{
    public static void AddBlocksEndpointsV1(this WebApplication app)
    {
        ApiVersionSet apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var group = app.MapGroup("api/v{version:apiVersion}/blocks")
                    .WithApiVersionSet(apiVersionSet)
                    .WithTags("Blocks")
                    .RequireAuthorization();

        group.MapGet("{id}", (
                Guid id, 
                [FromServices] IBlocksApiEndpointsHandler handler,
                ClaimsPrincipal claims) => handler.GetById(id, claims))
        .WithName("GetById")
        .WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Get Block Contents By Id",
            Description = "Returns information about selected block using its ID."
        });

        group.MapGet("{id}/children", (
                [FromServices] IBlocksApiEndpointsHandler handler,
                Guid id, 
                ClaimsPrincipal claims) => handler.GetAllChildren(id, claims))
        .WithName("GetAllChildren")
        .WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Get all children of Parent",
            Description = "Given the parent's Id, returns information about every children of it."
        });

        group.MapPost("", async (
                [FromServices] IBlocksApiEndpointsHandler handler,
                ClaimsPrincipal claims,
                [FromBody] BlockPostRequest request) => await handler.CreateNewAsync(request, claims))
        .WithName("CreateNewAsync")
        .WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Create a new block",
            Description = "Creates a new block and returns information about the created object. Created and Modified times are assigned to the current UTC time."
        });

        group.MapPut("{id}", async (Guid id, 
                [FromServices] IBlocksApiEndpointsHandler handler, 
                [FromBody] BlockPutRequest request,
                ClaimsPrincipal claims) => await handler.UpdateAsync(id, request, claims))
        .WithName("UpdateAsync")
        .WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "UpdateAsync Block contents.",
            Description = "Updates block contents and properties, as well as changes the parent/child relationships."
        });

        group.MapPatch("{id}", async (
            Guid id, 
            ClaimsPrincipal claims,
            [FromServices] IBlocksApiEndpointsHandler handler, 
            [FromBody] BlockPatchRequest request) => await handler.PatchAsync(id, request, claims));

        group.MapDelete("{id}", async (
            [FromServices] IBlocksApiEndpointsHandler handler, 
            Guid id,
            ClaimsPrincipal claims) => await handler.DeleteAsync(id, claims));
        
        group.MapGet("rootBlock", async (
            [FromServices] IBlocksApiEndpointsHandler handler, 
            [FromServices] UserManager<AppUser> userManager,
        ClaimsPrincipal claims) => await handler.GetUserRootBlockAsync(claims,userManager))
            .WithName("GetUserRootBlock")
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Gets the user root block.",
                Description = "Gets the user root block created at registration. Returns a JSON response of the contents of the block."
            });
    }

    
}
