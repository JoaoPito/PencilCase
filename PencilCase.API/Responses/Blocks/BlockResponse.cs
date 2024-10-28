using PencilCase.API.Responses.BlockProperties;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.API.Responses.Blocks;

public record class BlockResponse(
    Guid Id, 
    String Nome, 
    BlockType Type,
    BlockPropertiesResponse Properties,
    IEnumerable<Guid> ChildrenIds,
    Guid ParentId
    );
