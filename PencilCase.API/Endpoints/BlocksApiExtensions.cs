using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using PencilCase.API.Requests.Blocks;
using PencilCase.API.Responses.BlockProperties;
using PencilCase.API.Responses.Blocks;
using PencilCase.Shared.Data.Database;
using PencilCase.Shared.Models.Notebooks;

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
                    .WithTags("Blocks");

        group.MapGet("{id}", (Guid id, [FromServices] DAL<Block> dal) =>
        {
            var block = dal.GetBy(f => f.Id == id);
            if (block is null){
                return Results.NotFound();
            }
            return Results.Ok(MapEntityToResponse(block));
        })
        .WithName("GetById")
        .WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Get Block Contents By Id",
            Description = "Returns information about selected block using its ID."
        });

        group.MapPost("", async ([FromServices] DAL<Block> dal, [FromBody] BlockPostRequest request) => 
        {
            var newBlock = new Block();
            try
            {
                newBlock = MapRequestToEntity(request, dal);
            }
            catch(InvalidOperationException exc)
            {
                return Results.BadRequest(new { message = exc.Message });
            }

            await dal.Add(newBlock);
            return Results.CreatedAtRoute("GetById", new { id = newBlock.Id }, MapEntityToResponse(newBlock));
        })
        .WithName("CreateNew")
        .WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Create a new block",
            Description = "Creates a new block and returns information about the created object. Created and Modified times are assigned to the current UTC time."
        });

        group.MapPut("{id}", async (Guid id, [FromServices] DAL<Block> dal, [FromBody] BlockPutRequest request) => 
        {
            var block = dal.GetBy(b => b.Id == id);
            if(block == null)
                return Results.NotFound();

            var properties = block.Properties ?? new BlockProperties();

            block.Name = request.Name;
            block.Type = request.Type;
            block.Parent = TryGetParent(request.ParentId, dal);
            block.ParentId = request.ParentId;
            properties.Order = request.Properties.Order;
            properties.LastModified = DateTime.UtcNow;
            block.Properties = properties;
            block.Children = dal.GetAllBy(b => request.ChildrenIds.Contains(b.Id)).ToList();

            await dal.Update(block);
            return Results.Ok();
        })
        .WithName("Update")
        .WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Update Block contents.",
            Description = "Updates block contents and properties, as well as changes the parent/child relationships."
        });

        group.MapPatch("{id}", async (Guid id, [FromServices] DAL<Block> dal, [FromBody] BlockPatchRequest request) => 
        {
            var block = dal.GetBy(b => b.Id == id);
            if(block == null)
                return Results.NotFound();

            var properties = block.Properties ?? new BlockProperties();
            if(request.Properties != null)
            {
                properties.Order = request.Properties.Order ?? properties.Order;
            }
            properties.LastModified = DateTime.UtcNow;
            block.Properties = properties;

            block.Name = request.Name ?? block.Name;
            block.Type = request.Type ?? block.Type;
            if(request.ParentId != null)
            {
                block.Parent = TryGetParent(request.ParentId, dal);
                block.ParentId = request.ParentId;
            }
            block.Children = request.ChildrenIds != null ? dal.GetAllBy(b => request.ChildrenIds.Contains(b.Id)).ToList() : block.Children;

            await dal.Update(block);
            return Results.Ok();
        });
    }

    static IEnumerable<BlockResponse> MapEntityListToResponseList(IEnumerable<Block> entities)
    {
        return entities.Select(e => MapEntityToResponse(e)).ToList();
    }

    static BlockResponse MapEntityToResponse(Block entity)
    {
        var properties = entity.Properties == null ? new BlockPropertiesResponse(
            Order: 0,
            CreatedOn: DateTime.UtcNow,
            LastModified: DateTime.UtcNow
        ) :
        new BlockPropertiesResponse(
                Order: entity.Properties!.Order ,
                CreatedOn: entity.Properties!.CreatedOn,
                LastModified: entity.Properties!.LastModified
            );

        return new BlockResponse(
            Id: entity.Id,
            Name: entity.Name,
            Type: entity.Type,
            Properties: properties,
            ChildrenIds: entity.Children.Select(c => c.Id).ToList(),
            ParentId: entity.ParentId
        );
    }

    static Block MapRequestToEntity(BlockPostRequest request, DAL<Block> dal)
    {
        var block = new Block();

        var properties = new BlockProperties(){
            Parent = block,
            ParentId = block.Id,
            Order = request.Properties.Order
        };

        var parent = TryGetParent(request.ParentId, dal);

        block.Type = request.Type;
        block.Name = request.Name;
        block.Properties = properties;
        block.Children = request.ChildrenIds.Select(id => dal.GetBy(b => b.Id == id) ?? new Block()).ToList();
        block.ParentId = request.ParentId;
        block.Parent = parent;

        return block;
    }

    static Block? TryGetParent(Guid? parentId, DAL<Block> dal)
    {
        var parent = dal.GetBy(b => b.Id == parentId);
        if (parentId == null && parent != null)
            throw new InvalidOperationException("Root block already exists!");
        if (parentId != null && parent == null)
            throw new InvalidOperationException("Parent block does not exist!");
        return parent;
    }
}
