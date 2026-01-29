using System.Text.Json;
using Intentum.AI.Embeddings;
using Microsoft.Extensions.Caching.Distributed;

namespace Intentum.AI.Caching.Redis;

/// <summary>
/// Redis-backed distributed cache implementation for embeddings using IDistributedCache.
/// Suitable for multi-node production; use MemoryEmbeddingCache for single-node or development.
/// </summary>
public sealed class RedisEmbeddingCache : IEmbeddingCache
{
    private const string KeyPrefix = "Intentum:Embedding:";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private readonly IDistributedCache _cache;
    private readonly DistributedCacheEntryOptions _defaultOptions;

    /// <summary>
    /// Creates a Redis embedding cache with default expiration (24 hours).
    /// </summary>
    public RedisEmbeddingCache(IDistributedCache cache)
        : this(cache, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
        })
    {
    }

    /// <summary>
    /// Creates a Redis embedding cache with custom cache entry options.
    /// </summary>
    public RedisEmbeddingCache(IDistributedCache cache, DistributedCacheEntryOptions? defaultOptions)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _defaultOptions = defaultOptions ?? new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
        };
    }

    /// <inheritdoc />
    public IntentEmbedding? Get(string behaviorKey)
    {
        var key = GetCacheKey(behaviorKey);
        var bytes = _cache.Get(key);
        if (bytes is null || bytes.Length == 0)
            return null;

        return Deserialize(bytes);
    }

    /// <inheritdoc />
    public void Set(string behaviorKey, IntentEmbedding embedding)
    {
        var key = GetCacheKey(behaviorKey);
        var bytes = Serialize(embedding);
        _cache.Set(key, bytes, _defaultOptions);
    }

    /// <inheritdoc />
    public void Remove(string behaviorKey)
    {
        var key = GetCacheKey(behaviorKey);
        _cache.Remove(key);
    }

    /// <inheritdoc />
    public void Clear()
    {
        // IDistributedCache has no global Clear; callers can use a dedicated Redis key prefix and FLUSHDB in Redis, or track keys externally.
        throw new NotSupportedException(
            "Clear is not supported by Redis embedding cache. Use Redis FLUSHDB for the database or remove keys by prefix externally.");
    }

    private static string GetCacheKey(string behaviorKey)
    {
        return KeyPrefix + behaviorKey;
    }

    private static byte[] Serialize(IntentEmbedding embedding)
    {
        var dto = new EmbeddingDto(embedding.Source, embedding.Score, embedding.Vector?.ToList());
        return JsonSerializer.SerializeToUtf8Bytes(dto, JsonOptions);
    }

    private static IntentEmbedding? Deserialize(byte[] bytes)
    {
        var dto = JsonSerializer.Deserialize<EmbeddingDto>(bytes, JsonOptions);
        if (dto is null)
            return null;
        return new IntentEmbedding(dto.Source, dto.Score, dto.Vector);
    }

    private sealed record EmbeddingDto(string Source, double Score, List<double>? Vector);
}
