using PencilCase.Shared.Models.LLM.RAG;
using PencilCase.Shared.Models.Telemetry.LLM.RAG;
using Pinecone;

namespace PencilCase.LLM.VectorDb.Providers.Pinecone;

public static class PineconeTelemetryEntryHelpers
{
    public static RagOperationEntry BuildQueryTelemetryEntry(
        QueryResponse? queryResponse,
        EmbeddingsList? embeddings,
        IEnumerable<RagDocument>? resultDocs)
    {
        var fileSizeBytes = CalculateFileSize(resultDocs);
        
        return new RagOperationEntry()
        {
            OperationType = RagOperationType.Read,
            TokensUsed = CalculateTokensFromEmbeds(embeddings),
            UnitsUsed = (queryResponse is not null)? queryResponse.Usage?.ReadUnits ?? 0 : 0,
            FileSizeBytes = fileSizeBytes
        };
    }
    
    public static RagOperationEntry BuildWriteTelemetryEntry(
        EmbeddingsList? embeddings,
        IEnumerable<RagDocument> docsAdded)
    {
        var fileSizeBytes = CalculateFileSize(docsAdded);
        
        return new RagOperationEntry()
        {
            OperationType = RagOperationType.Write,
            TokensUsed = CalculateTokensFromEmbeds(embeddings),
            UnitsUsed = 0,
            FileSizeBytes = fileSizeBytes
        };
    }

    public static RagOperationEntry BuildUpdateTelemetryEntry(
        EmbeddingsList? embeddings,
        IEnumerable<RagDocument> docsAdded)
    {
        var fileSizeBytes = CalculateFileSize(docsAdded);
        
        return new RagOperationEntry()
        {
            OperationType = RagOperationType.Update,
            TokensUsed = CalculateTokensFromEmbeds(embeddings),
            UnitsUsed = 0,
            FileSizeBytes = fileSizeBytes
        };
    }

    private static uint CalculateFileSize(IEnumerable<RagDocument>? resultDocs)
    {
        uint fileSizeBytes = 0;
        if (resultDocs is not null)
        {
            foreach (var doc in resultDocs)
            {
                fileSizeBytes += (uint)doc.Content.Count();
            }
        }
        return fileSizeBytes;
    }

    private static uint CalculateTokensFromEmbeds(EmbeddingsList? embeddings)
    {
        return (embeddings is not null) ? (uint)(embeddings.Usage.TotalTokens ?? 0) : 0;
    }
}