namespace PencilCase.LLM.RAG.Requests;

public class QueryRequest
{
    public String Query { get; set; } = String.Empty;
    public List<Guid> ParentIds { get; set; } = new();
    public int? NResults { get; set; } = 3;
}