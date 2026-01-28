using Intentum.AI.Embeddings;

namespace Intentum.AI.Caching;

/// <summary>
/// Wrapper around an embedding provider that adds caching functionality.
/// </summary>
public sealed class CachedEmbeddingProvider : IIntentEmbeddingProvider
{
    private readonly IIntentEmbeddingProvider _innerProvider;
    private readonly IEmbeddingCache _cache;

    /// <summary>
    /// Creates a cached embedding provider.
    /// </summary>
    public CachedEmbeddingProvider(
        IIntentEmbeddingProvider innerProvider,
        IEmbeddingCache cache)
    {
        _innerProvider = innerProvider ?? throw new ArgumentNullException(nameof(innerProvider));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public IntentEmbedding Embed(string behaviorKey)
    {
        // Try to get from cache first
        var cached = _cache.Get(behaviorKey);
        if (cached != null)
            return cached;

        // If not in cache, get from inner provider
        var embedding = _innerProvider.Embed(behaviorKey);

        // Store in cache
        _cache.Set(behaviorKey, embedding);

        return embedding;
    }
}
