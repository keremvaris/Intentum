using Intentum.Runtime.RateLimiting;

namespace Intentum.Tests;

public sealed class RateLimiterTests
{
    [Fact]
    public async Task MemoryRateLimiter_FirstRequest_Allowed()
    {
        var limiter = new MemoryRateLimiter();
        var result = await limiter.TryAcquireAsync("user-1", limit: 3, TimeSpan.FromMinutes(1));
        Assert.True(result.Allowed);
        Assert.Equal(1, result.CurrentCount);
        Assert.Equal(3, result.Limit);
    }

    [Fact]
    public async Task MemoryRateLimiter_WithinLimit_Allowed()
    {
        var limiter = new MemoryRateLimiter();
        for (var i = 0; i < 3; i++)
        {
            var result = await limiter.TryAcquireAsync("user-1", limit: 3, TimeSpan.FromMinutes(1));
            Assert.True(result.Allowed);
            Assert.Equal(i + 1, result.CurrentCount);
        }
    }

    [Fact]
    public async Task MemoryRateLimiter_OverLimit_NotAllowed()
    {
        var limiter = new MemoryRateLimiter();
        for (var i = 0; i < 3; i++)
            await limiter.TryAcquireAsync("user-1", limit: 3, TimeSpan.FromMinutes(1));
        var result = await limiter.TryAcquireAsync("user-1", limit: 3, TimeSpan.FromMinutes(1));
        Assert.False(result.Allowed);
        Assert.Equal(4, result.CurrentCount);
        Assert.NotNull(result.RetryAfter);
    }

    [Fact]
    public async Task MemoryRateLimiter_Reset_AllowsAgain()
    {
        var limiter = new MemoryRateLimiter();
        for (var i = 0; i < 3; i++)
            await limiter.TryAcquireAsync("user-1", limit: 3, TimeSpan.FromMinutes(1));
        limiter.Reset("user-1");
        var result = await limiter.TryAcquireAsync("user-1", limit: 3, TimeSpan.FromMinutes(1));
        Assert.True(result.Allowed);
        Assert.Equal(1, result.CurrentCount);
    }

    [Fact]
    public async Task MemoryRateLimiter_DifferentKeys_Independent()
    {
        var limiter = new MemoryRateLimiter();
        for (var i = 0; i < 3; i++)
            await limiter.TryAcquireAsync("user-1", limit: 2, TimeSpan.FromMinutes(1));
        var r1 = await limiter.TryAcquireAsync("user-1", limit: 2, TimeSpan.FromMinutes(1));
        var r2 = await limiter.TryAcquireAsync("user-2", limit: 2, TimeSpan.FromMinutes(1));
        Assert.False(r1.Allowed);
        Assert.True(r2.Allowed);
        Assert.Equal(1, r2.CurrentCount);
    }
}
