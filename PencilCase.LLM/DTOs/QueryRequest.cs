namespace PencilCase.LLM.DTOs;

public class QueryRequest
{
    public String Query { get; set; } = String.Empty;
    public List<Guid> ParentIds { get; set; } = new();
    public uint? NResults { get; set; } = 3;
}