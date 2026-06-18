using Intentum.Runtime.Resilience.Degradation;

namespace Intentum.Tests.Resilience;

public sealed class DegradationPolicyTests
{
    [Fact]
    public async Task ExecuteAsync_NoFailures_ReturnsResult()
    {
        var policy = new MemoryDegradationPolicy(new DegradationOptions(DegradationThreshold: 3));
        var result = await policy.ExecuteAsync(
            () => Task.FromResult(42),
            () => -1);

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task ExecuteAsync_AfterThreshold_EntersDegradedState()
    {
        var policy = new MemoryDegradationPolicy(new DegradationOptions(
            DegradationThreshold: 2,
            CheckInterval: TimeSpan.FromMinutes(5)));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            policy.ExecuteAsync<int>(
                () => throw new InvalidOperationException(),
                () => -1));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            policy.ExecuteAsync<int>(
                () => throw new InvalidOperationException(),
                () => -1));

        Assert.True(policy.IsDegraded);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDegraded_ReturnsFallback()
    {
        var policy = new MemoryDegradationPolicy(new DegradationOptions(
            DegradationThreshold: 1,
            CheckInterval: TimeSpan.FromMinutes(5)));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            policy.ExecuteAsync<int>(
                () => throw new InvalidOperationException(),
                () => -1));

        var result = await policy.ExecuteAsync(
            () => throw new InvalidOperationException(),
            () => 99);

        Assert.Equal(99, result);
    }

    [Fact]
    public async Task ExecuteAsync_AfterCheckInterval_AllowsRetry()
    {
        var policy = new MemoryDegradationPolicy(new DegradationOptions(
            DegradationThreshold: 1,
            CheckInterval: TimeSpan.FromMilliseconds(50)));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            policy.ExecuteAsync<int>(
                () => throw new InvalidOperationException(),
                () => -1));

        Assert.True(policy.IsDegraded);

        await Task.Delay(100);

        var result = await policy.ExecuteAsync(
            () => Task.FromResult(42),
            () => -1);

        Assert.Equal(42, result);
        Assert.False(policy.IsDegraded);
    }

    [Fact]
    public async Task Reset_ClearsDegradedState()
    {
        var policy = new MemoryDegradationPolicy(new DegradationOptions(
            DegradationThreshold: 1,
            CheckInterval: TimeSpan.FromMinutes(5)));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            policy.ExecuteAsync<int>(
                () => throw new InvalidOperationException(),
                () => -1));

        policy.Reset();

        Assert.False(policy.IsDegraded);
    }
}
