using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.Shared.DTOs.Requests.BlockProperties;

public record class BlockPropertiesRequest
(
    int Order,
    DateTime LastModified,
    CellType CellType
);
