using System.ComponentModel.DataAnnotations;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.API.Requests.Blocks;

public record BlockRequest
(
    [Required] 
    String Name,
    [Required]
    BlockType Type,
    BlockProperties Properties,
    IEnumerable<Guid> ChildrenIds,
    [Required]
    Guid ParentId
);
