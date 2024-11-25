
using Microsoft.Extensions.Configuration;
using PencilCase.LLM.RAG.Models;
using Pinecone;

namespace PencilCase.LLM.RAG.Providers.Pinecone;

public class PineconeService : IRagService
{
    private readonly string? _embedModel;
    private readonly string _defaultNamespace;
    private readonly PineconeClient _client;

    public PineconeService(IConfiguration configuration)
    {
        var apiKey = configuration["Pinecone:ApiKey"];
        _client = new PineconeClient(apiKey);
        _embedModel = configuration["Pinecone:EmbedModel"] ?? "multilingual-e5-large";
        _defaultNamespace = configuration["Pinecone:DefaultNamespace"] ?? "pencilcase";
    }
    
    public List<Document> GetDocsByQuery(string query, List<Guid> parentIds)
    {
        throw new NotImplementedException();
    }

    public Task AddDocs(List<Document> docs)
    {
        var data = docs.Select(i => new EmbedRequestInputsItem()
        {
            Text = i.Content,
        });
    }

    public Task DeleteDocs(List<Guid> docIds)
    {
        throw new NotImplementedException();
    }

    public Task UpdateDocs(List<Document> docs)
    {
        throw new NotImplementedException();
    }
}