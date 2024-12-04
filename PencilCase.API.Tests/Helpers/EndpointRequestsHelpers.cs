using PencilCase.Shared.DTOs.Requests.Rag;
using PencilCase.Shared.Models.LLM.RAG;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.API.Tests.Helpers;

public static class EndpointRequestsHelpers
{
    public static bool IsEqualTo(this RagDocument ragDocument, Block block)
    {
        return ragDocument.Content == block.Name &&
               ragDocument.Id == block.Id && 
               ragDocument.ParentId == block.ParentId;
    }

    public static Block ToBlock(this RagDocument ragDocument)
    {
        return new Block()
        {
            Id = ragDocument.Id,
            ParentId = (ragDocument.ParentId == Guid.Parse("00000000-0000-0000-0000-000000000000")) ? null : ragDocument.ParentId,
            Name = ragDocument.Content,
            Type = BlockType.Cell
        };
    }

    public static RagSearchRequest ToSearchRequest(this Block block)
    {
        return new RagSearchRequest()
        {
            Content = block.Name,
            NotebookId = block.ParentId,
        };
    }
}