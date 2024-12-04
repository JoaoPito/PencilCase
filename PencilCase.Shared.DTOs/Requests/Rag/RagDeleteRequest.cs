namespace PencilCase.Shared.DTOs.Requests.Rag;

public record RagDeleteRequest
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public string? Content { get; set; }
}