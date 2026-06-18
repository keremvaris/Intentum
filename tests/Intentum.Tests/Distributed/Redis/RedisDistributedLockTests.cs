using Intentum.Distributed.Locking;
using Intentum.Distributed.Redis;
using StackExchange.Redis;

namespace Intentum.Tests.Distributed.Redis;

public sealed class RedisDistributedLockTests
{
    [Fact]
    public async Task Acquire_WithTimeout_ReturnsTrue()
    {
        if (!RedisAvailable()) return;

        var muxer = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
        var lockObj = new RedisDistributedLock(muxer);
        var acquired = await lockObj.AcquireAsync("test-key", TimeSpan.FromSeconds(30));
        Assert.True(acquired);
        await lockObj.ReleaseAsync("test-key");
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
