namespace Intentum.Core.Caching;

/// <summary>
/// In-memory implementation of <see cref="IIntentResultCache"/> for single-node or testing.
/// For distributed scenarios use Redis (e.g. Intentum.AI.Caching.Redis or custom implementation).
/// </summary>
public sealed class MemoryIntentResultCache : IIntentResultCache
{
    private readonly Dictionary<string, (string Value, DateTimeOffset? ExpiresAt)> _store = new();
    private readonly object _lock = new();

    /// <inheritdoc />
    public Task<(bool Found, string? Value)> TryGetAsync(string key, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (!_store.TryGetValue(key, out var entry))
                return Task.FromResult<(bool Found, string? Value)>((false, null));
            if (entry.ExpiresAt.HasValue && DateTimeOffset.UtcNow >= entry.ExpiresAt.Value)
            {
                _store.Remove(key);
                return Task.FromResult<(bool Found, string? Value)>((false, null));
            }
            return Task.FromResult<(bool Found, string? Value)>((true, entry.Value));
        }
    }

    /// <inheritdoc />
    public Task SetAsync(string key, string value, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        var expiresAt = ttl.HasValue ? DateTimeOffset.UtcNow + ttl.Value : (DateTimeOffset?)null;
        lock (_lock)
            _store[key] = (value, expiresAt);
        return Task.CompletedTask;
    }
}
