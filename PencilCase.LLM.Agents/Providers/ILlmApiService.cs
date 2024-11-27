using PencilCase.Shared.Models.LLM.Agents;

namespace PencilCase.LLM.Agents;

public interface ILlmApiService
{
    public Task<List<LlmMessage>> GenerateContent(List<LlmMessage> userPrompt, LlmMessage? systemPrompt = null);
}