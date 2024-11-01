using PencilCase.Shared.DTOs.Responses.BlockProperties;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.Shared.DTOs.Responses.Blocks;

public record class BlockResponse(
    Guid Id, 
    String Name, 
    BlockType Type,
    BlockPropertiesResponse Properties,
    IEnumerable<Guid> ChildrenIds,
    Guid? ParentId
    );
