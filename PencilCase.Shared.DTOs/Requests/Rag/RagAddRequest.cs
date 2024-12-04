namespace PencilCase.Shared.DTOs.Requests.Rag;

public record RagAddRequest
{
    public Guid Id { get; set; }
    public Guid ParentId { get; set; }
    public string Content { get; set; } = string.Empty;
}