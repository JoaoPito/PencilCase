using PencilCase.Shared.LLM.Models;

namespace PencilCase.Shared.LLM;

public interface ILlmApiService
{
    public Task<List<Message>> GenerateContent(List<Message> userPrompt);
}