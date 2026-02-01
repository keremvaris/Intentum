namespace Intentum.Core.Caching;

/// <summary>
/// Cache for intent inference results keyed by behavior vector (or its hash).
/// Use with a similarity threshold or exact key to skip recomputation (see CachedIntentModel in Intentum.AI).
/// Implement with in-memory or Redis for distributed scenarios.
/// </summary>
public interface IIntentResultCache
{
    /// <summary>
    /// Tries to get a cached intent result by key (e.g. hash of behavior vector).
    /// </summary>
    /// <param name="key">Cache key (e.g. vector hash).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if found, and the cached value (opaque string, e.g. JSON-serialized intent).</returns>
    Task<(bool Found, string? Value)> TryGetAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a cached intent result.
    /// </summary>
    /// <param name="key">Cache key (e.g. vector hash).</param>
    /// <param name="value">Value to cache (e.g. JSON-serialized intent).</param>
    /// <param name="ttl">Optional time-to-live.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SetAsync(string key, string value, TimeSpan? ttl = null, CancellationToken cancellationToken = default);
}
