using Intentum.Distributed.Locking;
using Intentum.Distributed.Redis;
using StackExchange.Redis;

namespace Intentum.Tests.Distributed.Redis;

public sealed class RedisDistributedLockTests
{
    [Fact(Skip = "Requires running Redis instance")]
    public async Task Acquire_WithTimeout_ReturnsTrue()
    {
        var muxer = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
        var lockObj = new RedisDistributedLock(muxer);
        var acquired = await lockObj.AcquireAsync("test-key", TimeSpan.FromSeconds(30));
        Assert.True(acquired);
        await lockObj.ReleaseAsync("test-key");
    }
}
