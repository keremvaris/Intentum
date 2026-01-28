using Intentum.AI.Embeddings;

namespace Intentum.AI.Mock;

/// <summary>
/// Deterministic mock for offline and CI usage.
/// </summary>
public sealed class MockEmbeddingProvider : IIntentEmbeddingProvider
{
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
