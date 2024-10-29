using System.ComponentModel.DataAnnotations;
using PencilCase.API.Requests.BlockProperties;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.API.Requests.Blocks;

public record BlockRequest
(
    [Required] 
    String Name,
    [Required]
    BlockType Type,
    BlockPropertiesRequest Properties,
    IEnumerable<Guid> ChildrenIds,
    [Required]
    Guid? ParentId
);
