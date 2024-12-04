namespace PencilCase.Shared.DTOs.Requests.Rag;

public record RagSearchRequest
{
    public Guid? NotebookId { get; set; }
    public string Content { get; set; } = string.Empty;
    public IEnumerable<Guid>? FilterIds { get; set; }
};