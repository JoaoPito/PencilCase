using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.Shared.DTOs.Responses.BlockProperties;

public record class BlockPropertiesResponse(
    int Order, 
    DateTime CreatedOn, 
    DateTime LastModified,
    CellType CellType
    );
