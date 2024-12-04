using PencilCase.Shared.Models.LLM.Agents;

namespace PencilCase.Shared.DTOs.Requests.Llm;

public record LlmMessageInvokeRequest
{
    public List<LlmMessage> ChatMessages { get; init; } = new();
}