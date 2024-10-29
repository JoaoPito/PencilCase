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

        group.MapPost("", async ([FromServices] DAL<Block> dal, [FromBody] BlockRequest request) => 
        {
            var newBlock = MapRequestToEntity(request, dal);
            await dal.Add(newBlock);
            return Results.CreatedAtRoute("GetById", new { id = newBlock.Id }, MapEntityToResponse(newBlock));
        })
        .WithName("CreateNew")
        .WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Create a new block",
            Description = "Creates a new block and returns information about the created object."
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
            Nome: entity.Name,
            Type: entity.Type,
            Properties: properties,
            ChildrenIds: entity.Children.Select(c => c.Id).ToList(),
            ParentId: entity.ParentId
        );
    }

    static Block MapRequestToEntity(BlockRequest request, DAL<Block> dal)
    {
        var block = new Block();

        var properties = new BlockProperties(){
            Parent = block,
            ParentId = block.Id,
            Order = request.Properties.Order
        };

        var parent = dal.GetBy(b => b.Id == request.ParentId);
        if (request.ParentId == null && parent != null)
            throw new InvalidOperationException("Root block already exists!");
        if (request.ParentId != null && parent == null)
            throw new InvalidOperationException("Parent block does not exist!");

        block.Type = request.Type;
        block.Name = request.Name;
        block.Properties = properties;
        block.Children = request.ChildrenIds.Select(id => dal.GetBy(b => b.Id == id) ?? new Block()).ToList();
        block.ParentId = request.ParentId;
        block.Parent = parent;

        return block;
    }
}
