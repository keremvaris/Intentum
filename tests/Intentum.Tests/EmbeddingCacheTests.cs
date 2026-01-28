using Intentum.AI.Caching;
using Intentum.AI.Embeddings;
using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core.Behavior;
using Microsoft.Extensions.Caching.Memory;

namespace Intentum.Tests;

/// <summary>
/// Tests for embedding caching functionality.
/// </summary>
public class EmbeddingCacheTests
{
    [Fact]
    public void MemoryEmbeddingCache_GetSet_WorksCorrectly()
    {
        // Arrange
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cache = new MemoryEmbeddingCache(memoryCache);
        var embedding = new IntentEmbedding("user:login", 0.85);

        // Act
        cache.Set("user:login", embedding);
        var retrieved = cache.Get("user:login");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(embedding.Source, retrieved!.Source);
        Assert.Equal(embedding.Score, retrieved.Score);
    }

    [Fact]
    public void MemoryEmbeddingCache_Get_ReturnsNullWhenNotFound()
    {
        // Arrange
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cache = new MemoryEmbeddingCache(memoryCache);

        // Act
        var result = cache.Get("nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void MemoryEmbeddingCache_Remove_RemovesFromCache()
    {
        // Arrange
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cache = new MemoryEmbeddingCache(memoryCache);
        var embedding = new IntentEmbedding("user:login", 0.85);
        cache.Set("user:login", embedding);

        // Act
        cache.Remove("user:login");
        var result = cache.Get("user:login");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void CachedEmbeddingProvider_CachesEmbeddings()
    {
        // Arrange
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cache = new MemoryEmbeddingCache(memoryCache);
        var innerProvider = new MockEmbeddingProvider();
        var cachedProvider = new CachedEmbeddingProvider(innerProvider, cache);

        // Act - First call should hit inner provider
        var embedding1 = cachedProvider.Embed("user:login");

        // Second call should hit cache
        var embedding2 = cachedProvider.Embed("user:login");

        // Assert
        Assert.NotNull(embedding1);
        Assert.NotNull(embedding2);
        Assert.Equal(embedding1.Source, embedding2.Source);
        Assert.Equal(embedding1.Score, embedding2.Score);
    }

    [Fact]
    public void CachedEmbeddingProvider_WithLlmIntentModel_WorksCorrectly()
    {
        // Arrange
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cache = new MemoryEmbeddingCache(memoryCache);
        var innerProvider = new MockEmbeddingProvider();
        var cachedProvider = new CachedEmbeddingProvider(innerProvider, cache);
        var model = new LlmIntentModel(cachedProvider, new SimpleAverageSimilarityEngine());

        var space = new BehaviorSpaceBuilder()
            .WithActor("user")
                .Action("login")
                .Action("submit")
            .Build();

        // Act
        var intent1 = model.Infer(space);
        var intent2 = model.Infer(space);

        // Assert
        Assert.NotNull(intent1);
        Assert.NotNull(intent2);
        // Both should work, second should use cache
    }

    [Fact]
    public void MemoryEmbeddingCache_WithCustomOptions_UsesOptions()
    {
        // Arrange
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var customOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };
        var cache = new MemoryEmbeddingCache(memoryCache, customOptions);
        var embedding = new IntentEmbedding("user:login", 0.85);

        // Act
        cache.Set("user:login", embedding);
        var retrieved = cache.Get("user:login");

        // Assert
        Assert.NotNull(retrieved);
    }
}
