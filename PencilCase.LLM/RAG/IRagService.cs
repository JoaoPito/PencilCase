using PencilCase.Shared.Models.LLM.RAG;

namespace PencilCase.LLM.RAG;

public interface IRagService
{
     public Task<List<RagDocument>?> GetDocsByQuery(String query, List<Guid> parentIds, uint nResults = 3);
     public Task AddDocs(List<RagDocument> docs);
     public Task DeleteSingleDoc(RagDocument doc);
     public Task UpdateDoc(RagDocument doc);
}