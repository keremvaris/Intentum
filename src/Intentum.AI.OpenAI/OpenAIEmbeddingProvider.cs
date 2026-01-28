using Intentum.AI.Embeddings;

namespace Intentum.AI.OpenAI;

public sealed class OpenAIEmbeddingProvider : IIntentEmbeddingProvider
{
    private readonly OpenAIOptions _options;

    public OpenAIEmbeddingProvider(OpenAIOptions options)
    {
        _options = options;
    }

    public IntentEmbedding Embed(string behaviorKey)
    {
        // Deterministic stub to keep CI/offline runs stable.
        // Replace with real OpenAI embeddings when wiring network calls.
        var hash = behaviorKey.GetHashCode();
        var normalized = Math.Abs(hash % 100) / 100.0;

        return new IntentEmbedding(
            Source: behaviorKey,
            Score: normalized
        );
    }
}
