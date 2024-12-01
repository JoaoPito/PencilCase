using PencilCase.Shared.Models.LLM.RAG;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.LLM.RAG;

public interface IRagService
{
     public Task<List<RagDocument>?> GetChunksForQuery(String query, List<Guid> filterIds, uint nResults = 3);
     public Task AddChunks(List<RagDocument> docs);
     public Task DeleteChunks(List<RagDocument> chunks);
}