namespace PencilCase.Shared.Models.Telemetry.LLM.Agents;

public record GenerationResultEntry()
{
    public Guid Id { get; set; }
    public DateTime Time { get; set; } = DateTime.UtcNow;
    public int PromptTokenCount { get; set; }
    public int GenerationTokenCount { get; set; }
    public int GenerationCharCount { get; set; }
    public string Model { get; set; } = String.Empty;
    public string? FinishReason { get; set; }
}