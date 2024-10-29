using System.ComponentModel.DataAnnotations;
using PencilCase.API.Requests.BlockProperties;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.API.Requests.Blocks;

public record BlockPutRequest(
    string Name, 
    BlockType Type, 
    BlockPropertiesRequest Properties, 
    IEnumerable<Guid> ChildrenIds,
    Guid? ParentId
    ) : BlockPostRequest(Name, Type, Properties, ChildrenIds, ParentId);
