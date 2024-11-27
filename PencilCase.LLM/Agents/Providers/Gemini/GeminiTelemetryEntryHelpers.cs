using PencilCase.Shared.Models.Telemetry.LLM.Agents;

namespace PencilCase.LLM.Agents.Providers.Gemini;

public static class GeminiTelemetryEntryHelpers
{
    public static GenerationResultEntry BuildGenerationTelemetryEntry(GeminiApiResponse geminiResponse)
    {
        var finishReasonList = geminiResponse.Candidates
            .Select(c => $"{c.FinishReason}," ?? string.Empty)
            .ToList();
        var finishReason = string.Concat(finishReasonList);
        
        var generationCharCount = geminiResponse.Candidates.Select(c => 
            ( c.Content is not null) ? c.Content.Parts.First().Text.Count() : 0).Sum();

        return new GenerationResultEntry()
        {
            PromptTokenCount = geminiResponse.UsageMetadata?.PromptTokenCount ?? 0,
            GenerationTokenCount = geminiResponse.UsageMetadata?.CandidatesTokenCount ?? 0,
            FinishReason = finishReason,
            GenerationCharCount = generationCharCount,
            Model = geminiResponse.ModelVersion
        };
    }
}