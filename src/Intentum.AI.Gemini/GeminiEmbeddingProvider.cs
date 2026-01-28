using Intentum.AI.Embeddings;

namespace Intentum.AI.Gemini;

public sealed class GeminiEmbeddingProvider : IIntentEmbeddingProvider
{
    private readonly GeminiOptions _options;

    public GeminiEmbeddingProvider(GeminiOptions options)
    {
        _options = options;
    }

    public IntentEmbedding Embed(string behaviorKey)
    {
        var hash = behaviorKey.GetHashCode();
        var normalized = Math.Abs(hash % 100) / 100.0;

        return new IntentEmbedding(
            Source: behaviorKey,
            Score: normalized
        );
    }
}
