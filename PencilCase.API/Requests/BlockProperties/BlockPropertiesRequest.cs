namespace PencilCase.API.Requests.BlockProperties;

public record class BlockPropertiesRequest
(
    int Order, 
    DateTime CreatedOn, 
    DateTime LastModified
);
