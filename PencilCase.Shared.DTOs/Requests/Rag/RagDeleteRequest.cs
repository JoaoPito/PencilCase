namespace PencilCase.Shared.DTOs.Requests.Rag;

public record RagDeleteRequest
{
    public IEnumerable<Guid> ChunksIds { get; set; }
    public Guid? ParentId { get; set; }
}