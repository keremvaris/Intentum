using Intentum.AI.Caching.Redis;
using Intentum.AI.Embeddings;

namespace Intentum.Tests;

public class RedisEmbeddingCacheTests
{
    private static readonly double[] TestVector = [0.1, 0.2];

    [Fact]
    public void GetSet_WithTestCache_WorksCorrectly()
    {
        var cache = new TestDistributedCache();
        var redisCache = new RedisEmbeddingCache(cache);
        var embedding = new IntentEmbedding("user:login", 0.85, TestVector);

        redisCache.Set("user:login", embedding);
        var retrieved = redisCache.Get("user:login");

        Assert.NotNull(retrieved);
        Assert.Equal(embedding.Source, retrieved!.Source);
        Assert.Equal(embedding.Score, retrieved.Score);
    }

    [Fact]
    public void Get_WhenNotFound_ReturnsNull()
    {
        var cache = new TestDistributedCache();
        var redisCache = new RedisEmbeddingCache(cache);
        Assert.Null(redisCache.Get("nonexistent"));
    }

    [Fact]
    public void Remove_RemovesFromCache()
    {
        var cache = new TestDistributedCache();
        var redisCache = new RedisEmbeddingCache(cache);
        redisCache.Set("key", new IntentEmbedding("key", 0.5));
        redisCache.Remove("key");
        Assert.Null(redisCache.Get("key"));
    }
}
