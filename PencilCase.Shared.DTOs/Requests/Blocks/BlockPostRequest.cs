using System.ComponentModel.DataAnnotations;
using PencilCase.Shared.DTOs.Requests.BlockProperties;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.Shared.DTOs.Requests.Blocks;

public record BlockPostRequest
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
