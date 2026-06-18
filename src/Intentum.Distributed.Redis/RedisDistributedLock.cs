using Intentum.Distributed.Locking;
using StackExchange.Redis;

namespace Intentum.Distributed.Redis;

public sealed class RedisDistributedLock : IDistributedLock
{
    private readonly IDatabase _db;
    private readonly string _keyPrefix;

    public RedisDistributedLock(IConnectionMultiplexer connection, string keyPrefix = "intentum:lock:")
    {
        _db = connection.GetDatabase();
        _keyPrefix = keyPrefix;
    }

    public async Task<bool> AcquireAsync(string key, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        var redisKey = $"{_keyPrefix}{key}";
        var token = Environment.MachineName + Guid.NewGuid().ToString("N");
        return await _db.LockTakeAsync(redisKey, token, timeout);
    }

    public async Task ReleaseAsync(string key, CancellationToken cancellationToken = default)
    {
        var redisKey = $"{_keyPrefix}{key}";
        await _db.KeyDeleteAsync(redisKey);
    }
}
