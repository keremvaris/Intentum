using Intentum.AI.Caching;
using Intentum.AI.Embeddings;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace Intentum.AI.Caching.FusionCache;

/// <summary>
/// FusionCache implementation of IEmbeddingCache.
/// Provides hybrid caching (L1 memory + L2 distributed) with advanced resiliency features.
/// </summary>
public sealed class FusionCacheEmbeddingCache : IEmbeddingCache
{
    private readonly ZiggyCreatures.FusionCache.IFusionCache _fusionCache;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of FusionCacheEmbeddingCache.
    /// </summary>
    /// <param name="fusionCache">The FusionCache instance to use.</param>
    public FusionCacheEmbeddingCache(ZiggyCreatures.FusionCache.IFusionCache fusionCache)
    {
        _fusionCache = fusionCache ?? throw new ArgumentNullException(nameof(fusionCache));
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    /// <summary>
    /// Gets an embedding from the cache.
    /// </summary>
    public IntentEmbedding? Get(string behaviorKey)
    {
        var cacheKey = GetCacheKey(behaviorKey);
        var cached = _fusionCache.Get<IntentEmbeddingDto?>(cacheKey);
        return cached?.ToIntentEmbedding();
    }

    /// <summary>
    /// Sets an embedding in the cache.
    /// </summary>
    public void Set(string behaviorKey, IntentEmbedding embedding)
    {
        var cacheKey = GetCacheKey(behaviorKey);
        var dto = IntentEmbeddingDto.FromIntentEmbedding(embedding);
        _fusionCache.Set(cacheKey, dto);
    }

    /// <summary>
    /// Sets an embedding in the cache with custom options.
    /// </summary>
    public void Set(string behaviorKey, IntentEmbedding embedding, ZiggyCreatures.FusionCache.FusionCacheEntryOptions options)
    {
        var cacheKey = GetCacheKey(behaviorKey);
        var dto = IntentEmbeddingDto.FromIntentEmbedding(embedding);
        
        if (options != null)
        {
            _fusionCache.Set(cacheKey, dto, options);
        }
        else
        {
            _fusionCache.Set(cacheKey, dto);
        }
    }

    /// <summary>
    /// Removes an embedding from the cache.
    /// </summary>
    public void Remove(string behaviorKey)
    {
        var cacheKey = GetCacheKey(behaviorKey);
        _fusionCache.Remove(cacheKey);
    }

    /// <summary>
    /// Clears all embeddings from the cache.
    /// </summary>
    public void Clear()
    {
        // FusionCache doesn't have a direct Clear() method for all entries
        // This would require tracking all keys, which is not practical
        // Instead, we throw NotSupportedException to indicate this limitation
        throw new NotSupportedException(
            "FusionCache does not support clearing all entries. " +
            "Use Remove() for specific keys or configure FusionCache with a cache key prefix for isolation.");
    }

    private static string GetCacheKey(string behaviorKey)
    {
        return $"intentum:embedding:{behaviorKey}";
    }

    /// <summary>
    /// DTO for serializing IntentEmbedding to/from FusionCache.
    /// </summary>
    private sealed record IntentEmbeddingDto(
        string Source,
        double Score,
        IReadOnlyList<double>? Vector)
    {
        public static IntentEmbeddingDto FromIntentEmbedding(IntentEmbedding embedding)
        {
            return new IntentEmbeddingDto(
                embedding.Source,
                embedding.Score,
                embedding.Vector);
        }

        public IntentEmbedding ToIntentEmbedding()
        {
            return new IntentEmbedding(
                Source,
                Score,
                Vector);
        }
    }
}
