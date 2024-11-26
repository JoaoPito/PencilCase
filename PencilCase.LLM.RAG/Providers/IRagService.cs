using PencilCase.LLM.RAG.Models;

namespace PencilCase.LLM.RAG.Providers;

public interface IRagService
{
     public Task<List<Document>?> GetDocsByQuery(String query, List<Guid> parentIds, uint nResults = 3);
     public Task AddDocs(List<Document> docs);
     public Task DeleteSingleDoc(Document doc);
     public Task UpdateDoc(Document doc);
}