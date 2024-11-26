
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

    private const string ParentIdMetadataKey = "parent_id"; 
    private const string TextMetadataKey = "text"; 
    private const char IdSeparator = ':';

    public PineconeService(IConfiguration configuration)
    {
        var apiKey = configuration["Pinecone:ApiKey"];
        _client = new PineconeClient(apiKey);
        _embedModel = configuration["Pinecone:EmbedModel"] ?? "multilingual-e5-large";
        _defaultIndex = configuration["Pinecone:DefaultIndex"] ?? "pencilcase";
        _defaultNamespace = configuration["Pinecone:DefaultNamespace"] ?? "";
    }
    
    public async Task<List<Document>?> GetDocsByQuery(string query, List<Guid> parentIds, uint nResults = 3)
    {
        var queryEmbedding = await EmbedDocuments(new List<Document> { new(){ Content = query } });

        var queryVector = queryEmbedding.Data.Select((e) => 
            e.Values?.Select(Convert.ToSingle).ToArray()).FirstOrDefault();
        
        var index = _client.Index(_defaultIndex);
        
        var queryResults = await index.QueryAsync(new QueryRequest()
        {
            Vector = new ReadOnlyMemory<float>(queryVector),
            Namespace = _defaultNamespace,
            TopK = nResults,
            IncludeMetadata = true,
            IncludeValues = false,
            Filter = new Metadata()
            {
                [ParentIdMetadataKey] = new Metadata()
                {
                    ["$in"] = parentIds.Select(x => x.ToString()).ToArray()
                }
            }
        });

        var resultDocs = queryResults
            .Matches?
            .Select(MapMatchToDocument)
            .ToList();
        return resultDocs;
    }

    public async Task AddDocs(List<Document> docs)
    {
        var embeddings = await EmbedDocuments(docs);

        var embeddingsData = embeddings.Data;

        var index = _client.Index(_defaultIndex);

        List<Vector> records = embeddingsData.Select((e, idx) => new Vector()
        {
            Id = GetDocumentId(docs[idx]),
            Values = new ReadOnlyMemory<float>(e.Values?.Select(Convert.ToSingle).ToArray()),
            Metadata = new Metadata()
            {
                [ParentIdMetadataKey] = docs[idx].ParentId.ToString(),
                [TextMetadataKey] = docs[idx].Content,
            }
        }).ToList();

        await index.UpsertAsync(new UpsertRequest()
        {
            Vectors = records,
            Namespace = _defaultNamespace
        });
    }

    public async Task DeleteSingleDoc(Document doc)
    {
        try
        {
            var index = _client.Index(_defaultIndex);

            await index.DeleteAsync(new DeleteRequest {
                Ids = new List<string> { GetDocumentId(doc) },
                Namespace = _defaultNamespace,
            });
        }
        catch (PineconeApiException ex)
        {
            if(ex.Message.Contains("Error with gRPC status code 5"))
                throw new ArgumentException(ex.Message);
            else
                throw;
        }
        
    }

    public async Task UpdateDoc(Document doc)
    {
        var index = _client.Index(_defaultIndex);
        
        var queryEmbedding = await EmbedDocuments(new List<Document> { doc });

        var docVector = queryEmbedding.Data.Select((e) => 
            e.Values?.Select(Convert.ToSingle).ToArray()).FirstOrDefault();

        await index.UpdateAsync(new UpdateRequest {
            Id = GetDocumentId(doc),
            Namespace = _defaultNamespace,
            Values = docVector,
            SetMetadata = new Metadata {
                [ParentIdMetadataKey] = new( doc.ParentId.ToString() ),
                [TextMetadataKey] = new ( doc.Content )
            }
        });
    }

    private async Task<EmbeddingsList> EmbedDocuments(List<Document> docs)
    {
        return await _client.Inference.EmbedAsync(new EmbedRequest()
        {
            Model = _embedModel,
            Inputs = docs.Select(i => new EmbedRequestInputsItem() { Text = i.Content }),
            Parameters = new EmbedRequestParameters()
            {
                InputType = "passage",
                Truncate = "END"
            }
        });
    }

    private Document MapMatchToDocument(ScoredVector match)
    {
        var id = Guid.Parse(match.Id.Split(IdSeparator).LastOrDefault() ?? "");
        var parentId = Guid.Parse(match.Metadata?[ParentIdMetadataKey]?.Value.ToString() ?? string.Empty);
        var text = match.Metadata?[TextMetadataKey]?.Value.ToString() ?? string.Empty;
        return new Document()
        {
            Id = id,
            ParentId = parentId,
            Content = text
        };
    }

    private String GetDocumentId(Document doc)
    {
        return $"{doc.ParentId}{IdSeparator}{doc.Id}";
    }
}