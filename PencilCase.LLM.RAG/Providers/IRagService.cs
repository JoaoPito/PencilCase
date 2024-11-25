using PencilCase.LLM.RAG.Models;

namespace PencilCase.LLM.RAG.Providers;

public interface IRagService
{
     public List<Document> GetDocsByQuery(String query, List<Guid> parentIds);
     public Task AddDocs(List<Document> docs);
     public Task DeleteDocs(List<Guid> docIds);
     public Task UpdateDocs(List<Document> docs);
}