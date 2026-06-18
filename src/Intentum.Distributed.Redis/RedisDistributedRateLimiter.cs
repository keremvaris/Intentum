using Intentum.Distributed.RateLimiting;
using StackExchange.Redis;

namespace Intentum.Distributed.Redis;

public sealed class RedisDistributedRateLimiter : IDistributedRateLimiter
{
    private readonly IDatabase _db;
    private readonly string _keyPrefix;

    public RedisDistributedRateLimiter(IConnectionMultiplexer connection, string keyPrefix = "intentum:ratelimit:")
    {
        _db = connection.GetDatabase();
        _keyPrefix = keyPrefix;
    }

    public async Task<bool> TryAcquireAsync(string key, int limit, TimeSpan window, CancellationToken cancellationToken = default)
    {
        var redisKey = $"{_keyPrefix}{key}";
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var windowStart = now - (long)window.TotalMilliseconds;

        await _db.SortedSetRemoveRangeByScoreAsync(redisKey, double.MinValue, windowStart);
        var currentCount = await _db.SortedSetLengthAsync(redisKey);

        if (currentCount >= limit)
            return false;

        await _db.SortedSetAddAsync(redisKey, Guid.NewGuid().ToString(), now);
        await _db.KeyExpireAsync(redisKey, window);
        return true;
    }
}
