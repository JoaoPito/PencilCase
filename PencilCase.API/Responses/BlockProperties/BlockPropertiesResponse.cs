namespace PencilCase.API.Responses.BlockProperties;

public record class BlockPropertiesResponse(
    int Order, 
    DateTime CreatedOn, 
    DateTime LastModified
    );
