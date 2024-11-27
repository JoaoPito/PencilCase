namespace PencilCase.Shared.Models.LLM.RAG;

public class RagDocument
{
    public Guid Id { get; set; } = new Guid();
    public Guid ParentId { get; set; } = new Guid();
    public String Content { get; set; } = string.Empty;
}