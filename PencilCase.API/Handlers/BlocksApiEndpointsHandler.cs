using System.Security.Claims;
using PencilCase.Shared.Data.Database;
using PencilCase.Shared.DTOs.Requests.Blocks;
using PencilCase.Shared.DTOs.Responses.BlockProperties;
using PencilCase.Shared.DTOs.Responses.Blocks;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.API.Handlers;

public class BlocksApiEndpointsHandler(IBlocksDal dal) : IBlocksApiEndpointsHandler
{
    public IResult GetById(Guid id, ClaimsPrincipal claims)
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
    }

    public IResult GetAllChildren(Guid id, ClaimsPrincipal claims)
    {
        var parent = dal.GetBy(f => f.Id == id);
        if (parent is null) return Results.NotFound();

        if (ValidateOwner(parent, claims))
        {
            var childList = parent.Children.ToList();
            return Results.Ok(MapEntityListToResponseList(childList));
        }

        return Results.Unauthorized();
    }

    public async Task<IResult> CreateNewAsync(BlockPostRequest request, ClaimsPrincipal claims)
    {
        var parent = GetParent(request.ParentId);
        if (parent == null) return Results.BadRequest();
            
        if (ValidateOwner(parent, claims))
        {
            Block newBlock;
            var userIdClaim = claims.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            try
            {
                newBlock = MapRequestToEntity(request, parent);
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
    }

    public async Task<IResult> UpdateAsync(Guid id, 
        BlockPutRequest request,
        ClaimsPrincipal claims)
    {
        var block = dal.GetBy(b => b.Id == id);
        if(block == null)
            return Results.NotFound();

        if (ValidateOwner(block, claims))
        {
            var properties = block.Properties ?? new BlockProperties();

            try
            {
                block.Parent = GetParent(request.ParentId);
            }
            catch(InvalidOperationException exc)
            {
                return Results.BadRequest(new { message = exc.Message });
            }
            try
            {
                block.Name = request.Name;
                block.Type = request.Type;
                block.Parent = GetParent(request.ParentId);
                block.ParentId = request.ParentId;
                properties.Order = request.Properties.Order;
                properties.LastModified = DateTime.UtcNow;
                properties.CellType = request.Properties.CellType;
                properties.CellShownAnswerId = request.Properties.CellShownAnswerId;
                block.Properties = properties;
                block.Children = dal.GetAllBy(b => request.ChildrenIds.Contains(b.Id)).ToList();
            }
            catch (NullReferenceException)
            {
                return Results.BadRequest();
            }

            await dal.Update(block);
            return Results.Ok();
        }

        return Results.Unauthorized();
    }

    public async Task<IResult> PatchAsync(Guid id, BlockPatchRequest request, ClaimsPrincipal claims)
    {
        var block = dal.GetBy(b => b.Id == id);
        if(block == null)
            return Results.NotFound();

        if (ValidateOwner(block, claims))
        {
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
                    block.Parent = GetParent(request.ParentId);
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
        }

        return Results.Unauthorized();
    }

    public async Task<IResult> DeleteAsync(Guid id, ClaimsPrincipal claims)
    {
        var block = dal.GetBy(b => b.Id == id);
        if(block == null)
            return Results.NotFound();
        if (ValidateOwner(block, claims))
        {
            if(block.ParentId == null)
                return Results.BadRequest("Cannot delete root block!");
            
            await dal.Delete(block);
            return Results.NoContent();
        }

        return Results.Unauthorized();
    }

    private IEnumerable<BlockResponse> MapEntityListToResponseList(IEnumerable<Block> entities)
    {
        return entities.Select(MapEntityToResponse).ToList();
    }

    private BlockResponse MapEntityToResponse(Block entity)
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

    private Block MapRequestToEntity(BlockPostRequest request, Block parent)
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
    
    private Block? GetParent(Guid? parentId)
    {
        var parent = dal.GetBy(b => b.Id == parentId);
        if (parentId == null && parent != null)
            throw new InvalidOperationException("Root block already exists!");
        if (parentId != null && parent == null)
            throw new InvalidOperationException("Parent block does not exist!");
        return parent;
    }

    private bool ValidateOwner(Block block, ClaimsPrincipal claims)
    {
        var userId = claims.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        var blockOwnerId = block.OwnerId.ToString();
        return (blockOwnerId != string.Empty && blockOwnerId.Equals(userId));
    }
}