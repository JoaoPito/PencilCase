using PencilCase.Shared.LLM.Models;

namespace PencilCase.Shared.LLM;

public interface ILlmApiService
{
    public Message GenerateContent(List<Message> userPrompt);
}