using PencilCase.LLM.Agents.Models;

namespace PencilCase.LLM.Agents;

public interface ILlmApiService
{
    public Task<List<Message>> GenerateContent(List<Message> userPrompt, Message? systemPrompt = null);
}