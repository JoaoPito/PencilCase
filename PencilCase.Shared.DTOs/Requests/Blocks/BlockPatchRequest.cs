using PencilCase.Shared.DTOs.Requests.BlockProperties;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.Shared.DTOs.Requests.Blocks;

public record BlockPatchRequest
(
    string? Name, 
    BlockType? Type, 
    BlockPropertiesPatchRequest? Properties, 
    IEnumerable<Guid>? ChildrenIds,
    Guid? ParentId
);
