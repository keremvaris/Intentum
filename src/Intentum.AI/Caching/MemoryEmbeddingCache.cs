using Intentum.AI.Embeddings;
using Microsoft.Extensions.Caching.Memory;

namespace Intentum.AI.Caching;

/// <summary>
/// Memory-based cache implementation for embeddings using IMemoryCache.
/// </summary>
public sealed class MemoryEmbeddingCache : IEmbeddingCache
{
    private readonly IMemoryCache _memoryCache;
    private readonly MemoryCacheEntryOptions? _defaultOptions;

    /// <summary>
    /// Creates a memory embedding cache with default options (1 hour expiration).
    /// </summary>
    public MemoryEmbeddingCache(IMemoryCache memoryCache)
        : this(memoryCache, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
            SlidingExpiration = TimeSpan.FromMinutes(30)
        })
    {
    }

    /// <summary>
    /// Creates a memory embedding cache with custom cache entry options.
    /// </summary>
    public MemoryEmbeddingCache(IMemoryCache memoryCache, MemoryCacheEntryOptions? defaultOptions)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _defaultOptions = defaultOptions;
    }

    public IntentEmbedding? Get(string behaviorKey)
    {
        return _memoryCache.TryGetValue<IntentEmbedding>(GetCacheKey(behaviorKey), out var embedding)
            ? embedding
            : null;
    }

    public void Set(string behaviorKey, IntentEmbedding embedding)
    {
        var key = GetCacheKey(behaviorKey);
        if (_defaultOptions != null)
        {
            _memoryCache.Set(key, embedding, _defaultOptions);
        }
        else
        {
            _memoryCache.Set(key, embedding);
        }
    }

    public void Remove(string behaviorKey)
    {
        _memoryCache.Remove(GetCacheKey(behaviorKey));
    }

    public void Clear()
    {
        // IMemoryCache doesn't have a Clear method, so we need to track keys
        // For simplicity, this implementation doesn't support Clear
        // Users can create a new cache instance or use a custom implementation
        throw new NotSupportedException("Clear is not supported by IMemoryCache. Create a new cache instance or use a custom implementation.");
    }

    private static string GetCacheKey(string behaviorKey)
    {
        return $"Intentum:Embedding:{behaviorKey}";
    }
}
