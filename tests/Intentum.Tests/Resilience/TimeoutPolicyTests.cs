using Intentum.Runtime.Resilience.Timeout;

namespace Intentum.Tests.Resilience;

public sealed class TimeoutPolicyTests
{
    [Fact]
    public async Task ExecuteAsync_WithinTimeout_ReturnsResult()
    {
        var timeout = new MemoryTimeoutPolicy(new TimeoutOptions(
            TimeoutDuration: TimeSpan.FromSeconds(5)));
        var result = await timeout.ExecuteAsync(ct => Task.FromResult(42));

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task ExecuteAsync_ExceedsTimeout_ThrowsTimeoutException()
    {
        var timeout = new MemoryTimeoutPolicy(new TimeoutOptions(
            TimeoutDuration: TimeSpan.FromMilliseconds(50)));

        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            timeout.ExecuteAsync(async ct =>
            {
                await Task.Delay(5000, ct);
                return 42;
            }));
    }

    [Fact]
    public async Task ExecuteAsync_PropagatesCancellationToken()
    {
        var timeout = new MemoryTimeoutPolicy(new TimeoutOptions(
            TimeoutDuration: TimeSpan.FromSeconds(5)));
        using var cts = new CancellationTokenSource();

        var result = await timeout.ExecuteAsync(async ct =>
        {
            await Task.Delay(10, ct);
            ct.ThrowIfCancellationRequested();
            return 42;
        }, cts.Token);

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task ExecuteAsync_OperationException_PropagatesException()
    {
        var timeout = new MemoryTimeoutPolicy(new TimeoutOptions(
            TimeoutDuration: TimeSpan.FromSeconds(5)));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            timeout.ExecuteAsync<int>(ct =>
                throw new InvalidOperationException("Operation failed")));
    }

    [Fact]
    public async Task ExecuteAsync_CompletesBeforeTimeout_ReturnsEarly()
    {
        var timeout = new MemoryTimeoutPolicy(new TimeoutOptions(
            TimeoutDuration: TimeSpan.FromSeconds(5)));

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = await timeout.ExecuteAsync(ct => Task.FromResult(42));
        sw.Stop();

        Assert.Equal(42, result);
        Assert.True(sw.ElapsedMilliseconds < 1000);
    }
}
