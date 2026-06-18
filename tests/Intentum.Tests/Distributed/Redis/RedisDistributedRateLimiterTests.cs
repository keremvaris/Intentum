using Intentum.Distributed.RateLimiting;
using Intentum.Distributed.Redis;
using StackExchange.Redis;

namespace Intentum.Tests.Distributed.Redis;

public sealed class RedisDistributedRateLimiterTests
{
    [Fact]
    public async Task TryAcquire_WithinLimit_ReturnsTrue()
    {
        if (!RedisAvailable()) return;

        var muxer = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
        var limiter = new RedisDistributedRateLimiter(muxer);
        var result = await limiter.TryAcquireAsync("test-rate", 5, TimeSpan.FromMinutes(1));
        Assert.True(result);
    }

    private static bool RedisAvailable()
    {
        try
        {
            using var conn = ConnectionMultiplexer.Connect("localhost:6379");
            return conn.IsConnected;
        }
        catch
        {
            return false;
        }
    }
}
