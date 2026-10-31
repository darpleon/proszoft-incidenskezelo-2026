using incidenskezelo_backend.Models.AI;

namespace incidenskezelo_backend.Services.AI;

public sealed class AiServiceResolver(
    GeminiService geminiService,
    LocalAiService localAiService)
{
    public IAiService Resolve(AiProvider provider) =>
        provider switch
        {
            AiProvider.Gemini => geminiService,
            AiProvider.Qwen3 => localAiService,
            _ => throw new ArgumentOutOfRangeException(
                nameof(provider),
                "Unsupported AI provider.")
        };
}