using System.ComponentModel.DataAnnotations;
using PencilCase.API.Requests.BlockProperties;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.API.Requests.Blocks;

public record BlockEditRequest(
    [Required] Guid Id, 
    string Name, 
    BlockType Type, 
    BlockPropertiesRequest Properties, 
    IEnumerable<Guid> ChildrenIds,
    Guid? ParentId
    ) : BlockRequest(Name, Type, Properties, ChildrenIds, ParentId);
