using Intentum.AI.Embeddings;

namespace Intentum.AI.Claude;

public sealed class ClaudeEmbeddingProvider : IIntentEmbeddingProvider
{
    private readonly ClaudeOptions _options;

    public ClaudeEmbeddingProvider(ClaudeOptions options)
    {
        _options = options;
        _options.Validate();
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
