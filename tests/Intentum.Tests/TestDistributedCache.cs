using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Distributed;

namespace Intentum.Tests;

/// <summary>
/// In-memory IDistributedCache for testing RedisEmbeddingCache without Redis.
/// </summary>
internal sealed class TestDistributedCache : IDistributedCache
{
    private readonly ConcurrentDictionary<string, byte[]> _store = new();

    public byte[]? Get(string key) => _store.TryGetValue(key, out var v) ? v : null;

    public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
        => Task.FromResult(Get(key));

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        => _store[key] = value;

    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        Set(key, value, options);
        return Task.CompletedTask;
    }

    public void Remove(string key) => _store.TryRemove(key, out _);

    public Task RemoveAsync(string key, CancellationToken token = default)
    {
        Remove(key);
        return Task.CompletedTask;
    }

    public void Refresh(string key) { }

    public Task RefreshAsync(string key, CancellationToken token = default) => Task.CompletedTask;
}
