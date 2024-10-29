namespace PencilCase.API.Requests.BlockProperties;

public record class BlockPropertiesPatchRequest
(
    int? Order,
    DateTime? LastModified
);
