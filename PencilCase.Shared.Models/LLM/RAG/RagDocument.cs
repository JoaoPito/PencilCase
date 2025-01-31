namespace PencilCase.Shared.Models.LLM.RAG;

public class RagDocument
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ParentId { get; set; } = new Guid();
    public String Content { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"Id: {Id.ToString()}, Content: \"{String.Concat(Content.Take(50))}\"";
    }

    public override bool Equals(object obj)
    {
        if (obj is not RagDocument) return false;
        return this.Equals((RagDocument)obj);
    }

    protected bool Equals(RagDocument other)
    {
        return Id.Equals(other.Id) && ParentId.Equals(other.ParentId) && Content == other.Content;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, ParentId, Content);
    }
}