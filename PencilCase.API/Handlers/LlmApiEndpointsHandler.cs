using PencilCase.LLM.Agents.Providers;
using PencilCase.LLM.RAG;

namespace PencilCase.API.Handlers;

public class LlmApiEndpointsHandler : ILlmApiEndpointsHandler
{
    public LlmApiEndpointsHandler(IRagService ragService, ILlmApiService llmApiService)
    {
        
    }
}