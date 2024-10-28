using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.API.Responses.Blocks;

public record class BlockResponse(
    Guid Id, 
    String Nome, 
    BlockType Type,
    BlockProperties Properties,
    IEnumerable<Guid> ChildrenIds,
    Guid ParentId
    );
