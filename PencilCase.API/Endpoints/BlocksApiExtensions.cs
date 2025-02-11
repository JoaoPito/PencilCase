using System.Runtime.InteropServices;
using System.Security.Claims;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using PencilCase.Shared.Data.Database;
using PencilCase.Shared.DTOs.Requests.Blocks;
using PencilCase.Shared.DTOs.Responses.BlockProperties;
using PencilCase.Shared.DTOs.Responses.Blocks;
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
                    .WithTags("Blocks")
                    .RequireAuthorization();

        group.MapGet("{id}", (Guid id, 
                [FromServices] IBlocksDal dal,
                ClaimsPrincipal claims) =>
        {
            var block = dal.GetBy(f => f.Id == id);
            if (block is null){
                return Results.NotFound();
            }

            if (ValidateOwner(block, claims))
            {
                return Results.Ok(MapEntityToResponse(block));
            }
            return Results.Unauthorized();
        })
        .WithName("GetById")
        .WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Get Block Contents By Id",
            Description = "Returns information about selected block using its ID."
        });

        group.MapGet("{id}/children", (
                Guid id, 
                ClaimsPrincipal claims,
                [FromServices] IBlocksDal dal) =>
            {
                var parent = dal.GetBy(f => f.Id == id);
                if (parent is null) return Results.NotFound();

                if (ValidateOwner(parent, claims))
                {
                    var childList = parent.Children.ToList();
                    return Results.Ok(MapEntityListToResponseList(childList));
                }

                return Results.Unauthorized();
            })
        .WithName("GetAllChildren")
        .WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Get all children of Parent",
            Description = "Given the parent's Id, returns information about every children of it."
        });

        group.MapPost("", async (
                [FromServices] IBlocksDal dal,
                ClaimsPrincipal claims,
                [FromBody] BlockPostRequest request) => 
        {
            var parent = GetParent(request.ParentId, dal);
            if (parent == null) return Results.BadRequest();
            
            if (ValidateOwner(parent, claims))
            {
                var newBlock = new Block();
                var userIdClaim = claims.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                try
                {
                    newBlock = MapRequestToEntity(request, dal, parent);
                    newBlock.OwnerId = Guid.Parse(userIdClaim);
                }
                catch(InvalidOperationException exc)
                {
                    return Results.BadRequest(new { message = exc.Message });
                }

                await dal.Add(newBlock);
                return Results.CreatedAtRoute("GetById", new { id = newBlock.Id }, MapEntityToResponse(newBlock));
            }
            return Results.Unauthorized();
        })
        .WithName("CreateNew")
        .WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Create a new block",
            Description = "Creates a new block and returns information about the created object. Created and Modified times are assigned to the current UTC time."
        });

        group.MapPut("{id}", async (Guid id, [FromServices] IBlocksDal dal, [FromBody] BlockPutRequest request) => 
        {
            var block = dal.GetBy(b => b.Id == id);
            if(block == null)
                return Results.NotFound();

            var properties = block.Properties ?? new BlockProperties();

            try
            {
                block.Parent = GetParent(request.ParentId, dal);
            }
            catch(InvalidOperationException exc)
            {
                return Results.BadRequest(new { message = exc.Message });
            }

            block.Name = request.Name;
            block.Type = request.Type;
            block.Parent = GetParent(request.ParentId, dal);
            block.ParentId = request.ParentId;
            properties.Order = request.Properties.Order;
            properties.LastModified = DateTime.UtcNow;
            properties.CellType = request.Properties.CellType;
            properties.CellShownAnswerId = request.Properties.CellShownAnswerId;
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

        group.MapPatch("{id}", async (Guid id, [FromServices] IBlocksDal dal, [FromBody] BlockPatchRequest request) => 
        {
            var block = dal.GetBy(b => b.Id == id);
            if(block == null)
                return Results.NotFound();

            var properties = block.Properties ?? new BlockProperties();
            if(request.Properties != null)
            {
                properties.Order = request.Properties.Order ?? properties.Order;
                properties.CellType = request.Properties.CellType ?? properties.CellType;
            }
            properties.LastModified = DateTime.UtcNow;
            block.Properties = properties;

            block.Name = request.Name ?? block.Name;
            block.Type = request.Type ?? block.Type;
            if(request.ParentId != null)
            {
                try
                {
                    block.Parent = GetParent(request.ParentId, dal);
                }
                catch(InvalidOperationException exc)
                {
                    return Results.BadRequest(new { message = exc.Message });
                }
                block.ParentId = request.ParentId;
            }
            block.Children = request.ChildrenIds != null ? dal.GetAllBy(b => request.ChildrenIds.Contains(b.Id)).ToList() : block.Children;

            await dal.Update(block);
            return Results.Ok();
        });

        group.MapDelete("{id}", async ([FromServices] IBlocksDal dal, Guid id) => 
        {
            var block = dal.GetBy(b => b.Id == id);
            if(block == null)
                return Results.NotFound();
            
            if(block.ParentId == null)
                return Results.BadRequest("Cannot delete root block!");
            
            await dal.Delete(block);
            return Results.NoContent();
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
            LastModified: DateTime.UtcNow,
            CellType: CellType.Text,
            CellShownAnswerId: null
        ) :
        new BlockPropertiesResponse(
                Order: entity.Properties!.Order ,
                CreatedOn: entity.Properties!.CreatedOn,
                LastModified: entity.Properties!.LastModified,
                CellType: entity.Properties!.CellType,
                CellShownAnswerId: entity.Properties!.CellShownAnswerId
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

    static Block MapRequestToEntity(BlockPostRequest request, IBlocksDal dal, Block parent)
    {
        var block = new Block();

        var properties = new BlockProperties(){
            Parent = block,
            ParentId = block.Id,
            Order = request.Properties.Order,
            CellType = request.Properties.CellType,
            CellShownAnswerId = request.Properties.CellShownAnswerId
        };

        block.Type = request.Type;
        block.Name = request.Name;
        block.Properties = properties;
        block.Children = request.ChildrenIds.Select(id => dal.GetBy(b => b.Id == id) ?? new Block()).ToList();
        block.ParentId = request.ParentId;
        block.Parent = parent;

        return block;
    }
    
    static Block? GetParent(Guid? parentId, IBlocksDal dal)
    {
        var parent = dal.GetBy(b => b.Id == parentId);
        if (parentId == null && parent != null)
            throw new InvalidOperationException("Root block already exists!");
        if (parentId != null && parent == null)
            throw new InvalidOperationException("Parent block does not exist!");
        return parent;
    }

    static bool ValidateOwner(Block block, ClaimsPrincipal claims)
    {
        var userId = claims.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        var blockOwnerId = block.OwnerId.ToString();
        return (blockOwnerId != string.Empty && blockOwnerId.Equals(userId));
    }
}
