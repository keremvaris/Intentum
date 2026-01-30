using Intentum.AI.Embeddings;
using JetBrains.Annotations;

namespace Intentum.AI.Caching;

/// <summary>
/// Cache interface for embedding results.
/// </summary>
public interface IEmbeddingCache
{
    /// <summary>
    /// Gets an embedding from cache, or returns null if not found.
    /// </summary>
    IntentEmbedding? Get(string behaviorKey);

    /// <summary>
    /// Sets an embedding in the cache.
    /// </summary>
    void Set(string behaviorKey, IntentEmbedding embedding);

    /// <summary>
    /// Removes an embedding from the cache.
    /// </summary>
    [UsedImplicitly]
    void Remove(string behaviorKey);

    /// <summary>
    /// Clears all cached embeddings.
    /// </summary>
    [UsedImplicitly]
    void Clear();
}
