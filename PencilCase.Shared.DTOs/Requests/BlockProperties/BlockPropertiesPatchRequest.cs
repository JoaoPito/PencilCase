namespace PencilCase.Shared.DTOs.Requests.BlockProperties;

public record class BlockPropertiesPatchRequest
(
    int? Order,
    DateTime? LastModified
);
