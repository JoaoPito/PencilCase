using PencilCase.Shared.DTOs.Requests.BlockProperties;
using PencilCase.Shared.DTOs.Requests.Blocks;
using PencilCase.Shared.DTOs.Responses.BlockProperties;
using PencilCase.Shared.DTOs.Responses.Blocks;

namespace PencilCase.Web.Client.Services.Blocks;

public class BlockMapper
{
    public BlockViewModel? MapResponseToViewModel(BlockResponse? response)
    {
        if (response == null)
            return null;
        
        return new BlockViewModel()
        {
            Id = response.Id,
            Name = response.Name,
            Properties = MapResponseToPropertiesViewModel(response.Properties),
            ChildrenIds = response.ChildrenIds,
            Type = response.Type,
            ParentId = response.ParentId,
        };
    }

    public BlockPropertiesViewModel MapResponseToPropertiesViewModel(BlockPropertiesResponse response)
    {
        return new BlockPropertiesViewModel()
        {
            CreatedOn = response.CreatedOn,
            LastModified = response.LastModified,
            Order = response.Order,
            CellType = response.CellType,
            CellShownAnswerId = response.CellShownAnswerId,
        };
    }

    public BlockPostRequest? MapViewModelToPostRequest(BlockViewModel? block)
    {
        if (block == null)
            return null;

        return new BlockPostRequest(
            Name: block.Name,
            Type: block.Type,
            Properties: MapPropertiesViewModelToRequest(block.Properties),
            ChildrenIds: block.ChildrenIds,
            ParentId: block.ParentId
        );
    }

    public BlockPropertiesRequest MapPropertiesViewModelToRequest(BlockPropertiesViewModel properties)
    {
        return new BlockPropertiesRequest(
            LastModified: properties.LastModified,
            Order: properties.Order,
            CellType: properties.CellType,
            CellShownAnswerId: properties.CellShownAnswerId
        );
    }

    public BlockPutRequest? MapViewModelToPutRequest(BlockViewModel? block)
    {
        if (block == null)
            return null;
        return new BlockPutRequest(
            Name: block.Name,
            Type: block.Type,
            Properties: MapPropertiesViewModelToRequest(block.Properties),
            ChildrenIds: block.ChildrenIds,
            ParentId: block.ParentId
        );
    }

    public List<BlockViewModel>? MapViewModelsToList(List<BlockResponse>? blocks)
    {
        if(blocks == null)
            return null;
        
        var blockList = new List<BlockViewModel>();

        foreach (var block in blocks)
        {
            blockList.Add(MapResponseToViewModel(block)!);
        }
        return blockList;
    }
}