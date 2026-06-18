using Intentum.Distributed.RateLimiting;
using Intentum.Distributed.Redis;
using StackExchange.Redis;

namespace Intentum.Tests.Distributed.Redis;

public sealed class RedisDistributedRateLimiterTests
{
    [Fact(Skip = "Requires running Redis instance")]
    public async Task TryAcquire_WithinLimit_ReturnsTrue()
    {
        var muxer = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
        var limiter = new RedisDistributedRateLimiter(muxer);
        var result = await limiter.TryAcquireAsync("test-rate", 5, TimeSpan.FromMinutes(1));
        Assert.True(result);
    }
}
