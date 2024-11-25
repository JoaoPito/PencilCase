namespace PencilCase.LLM.RAG.Models;

public class Document
{
    public Guid Id { get; set; } = new Guid();
    public Guid ParentId { get; set; } = new Guid();
    public String Content { get; set; } = string.Empty;
}