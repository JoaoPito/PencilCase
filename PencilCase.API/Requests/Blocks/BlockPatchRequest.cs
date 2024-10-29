using PencilCase.API.Requests.BlockProperties;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.API.Requests.Blocks;

public record BlockPatchRequest
(
    string? Name, 
    BlockType? Type, 
    BlockPropertiesPatchRequest? Properties, 
    IEnumerable<Guid>? ChildrenIds,
    Guid? ParentId
);
