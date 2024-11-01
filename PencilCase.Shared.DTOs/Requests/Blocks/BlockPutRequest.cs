using PencilCase.Shared.DTOs.Requests.BlockProperties;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.Shared.DTOs.Requests.Blocks;

public record BlockPutRequest(
    string Name, 
    BlockType Type, 
    BlockPropertiesRequest Properties, 
    IEnumerable<Guid> ChildrenIds,
    Guid? ParentId
    ) : BlockPostRequest(Name, Type, Properties, ChildrenIds, ParentId);
