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
        var cached = _cache.Get(behaviorKey);
        if (cached != null)
            return cached;

        var embedding = _innerProvider.Embed(behaviorKey);
        _cache.Set(behaviorKey, embedding);
        return embedding;
    }

    public async Task<IntentEmbedding> EmbedAsync(string behaviorKey, CancellationToken cancellationToken = default)
    {
        var cached = _cache.Get(behaviorKey);
        if (cached != null)
            return cached;

        var embedding = await _innerProvider.EmbedAsync(behaviorKey, cancellationToken);
        _cache.Set(behaviorKey, embedding);
        return embedding;
    }
}
