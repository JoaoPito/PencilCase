namespace PencilCase.Shared.Models.Telemetry.LLM.Agents;

public record GenerationResultEntry(
    Guid Id,
    DateTime Time,
    int PromptTokenCount,
    int GenerationTokenCount,
    int GenerationCharCount,
    string FinishReason
    );