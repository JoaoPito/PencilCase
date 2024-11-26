
using Microsoft.Extensions.Configuration;
using PencilCase.LLM.RAG.Models;
using Pinecone;

namespace PencilCase.LLM.RAG.Providers.Pinecone;

public class PineconeService : IRagService
{
    private readonly string _embedModel;
    private readonly string _defaultNamespace;
    private readonly PineconeClient _client;
    private readonly string _defaultIndex;

    public PineconeService(IConfiguration configuration)
    {
        var apiKey = configuration["Pinecone:ApiKey"];
        _client = new PineconeClient(apiKey);
        _embedModel = configuration["Pinecone:EmbedModel"] ?? "multilingual-e5-large";
        _defaultIndex = configuration["Pinecone:DefaultIndex"] ?? "pencilcase";
        _defaultNamespace = configuration["Pinecone:DefaultNamespace"] ?? "";
    }
    
    public List<Document> GetDocsByQuery(string query, List<Guid> parentIds, int nResults = 3)
    {
        throw new NotImplementedException();
    }

    public async Task AddDocs(List<Document> docs)
    {
        var embeddings = await _client.Inference.EmbedAsync(new EmbedRequest()
        {
            Model = _embedModel,
            Inputs = docs.Select(i => new EmbedRequestInputsItem() { Text = i.Content }),
            Parameters = new EmbedRequestParameters()
            {
                InputType = "passage",
                Truncate = "END"
            }
        });

        var embeddingsData = embeddings.Data;

        var index = _client.Index(_defaultIndex);

        List<Vector> records = embeddingsData.Select((e, idx) => new Vector()
        {
            Id = docs[idx].Id.ToString(),
            Values = new ReadOnlyMemory<float>(e.Values?.Select(Convert.ToSingle).ToArray()),
            Metadata = new Metadata()
            {
                ["parent_id"] = docs[idx].ParentId.ToString()
            }
        }).ToList();

        await index.UpsertAsync(new UpsertRequest()
        {
            Vectors = records,
            Namespace = _defaultNamespace
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