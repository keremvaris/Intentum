using Intentum.AI.Embeddings;

namespace Intentum.AI.Mistral;

public sealed class MistralEmbeddingProvider : IIntentEmbeddingProvider
{
    private readonly MistralOptions _options;

    public MistralEmbeddingProvider(MistralOptions options)
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
