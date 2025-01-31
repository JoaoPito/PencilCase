namespace PencilCase.Shared.DTOs.Requests.Rag;

public record RagDeleteRequest
{
    public IEnumerable<Guid> ChunksIds { get; set; } = new List<Guid>();
    public Guid DocumentId { get; set; }
}