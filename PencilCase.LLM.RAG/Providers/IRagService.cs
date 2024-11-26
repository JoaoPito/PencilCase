using PencilCase.LLM.RAG.Models;

namespace PencilCase.LLM.RAG.Providers;

public interface IRagService
{
     public Task<List<Document>> GetDocsByQuery(String query, List<Guid> parentIds, int nResults = 3);
     public Task AddDocs(List<Document> docs);
     public Task DeleteDocs(List<Guid> docIds);
     public Task UpdateDocs(List<Document> docs);
}