using PencilCase.Shared.LLM.Models;

namespace PencilCase.Shared.LLM;

public interface ILlmApiService
{
    public Task<Message> GenerateContent(List<Message> userPrompt);
}