using Intentum.Runtime.Resilience.CircuitBreaker;
using Intentum.Runtime.Resilience.Exceptions;

namespace Intentum.Tests.Resilience;

public sealed class CircuitBreakerTests
{
    [Fact]
    public async Task ExecuteAsync_WhenCircuitIsClosed_ExecutesOperation()
    {
        var breaker = new MemoryCircuitBreaker(new CircuitBreakerOptions());
        var result = await breaker.ExecuteAsync(() => Task.FromResult(42));

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task ExecuteAsync_AfterFailureThreshold_OpensCircuit()
    {
        var breaker = new MemoryCircuitBreaker(new CircuitBreakerOptions(
            FailureThreshold: 2,
            DurationOfBreak: TimeSpan.FromMinutes(5)));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            breaker.ExecuteAsync<int>(() => throw new InvalidOperationException()));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            breaker.ExecuteAsync<int>(() => throw new InvalidOperationException()));

        Assert.Equal(CircuitState.Open, breaker.State);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCircuitIsOpen_ThrowsWithoutExecuting()
    {
        var breaker = new MemoryCircuitBreaker(new CircuitBreakerOptions(
            FailureThreshold: 1,
            DurationOfBreak: TimeSpan.FromMinutes(5)));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            breaker.ExecuteAsync<int>(() => throw new InvalidOperationException()));

        var ex = await Assert.ThrowsAsync<CircuitBreakerOpenException>(() =>
            breaker.ExecuteAsync(() => Task.FromResult(42)));

        Assert.Contains("open", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_AfterDurationOfBreak_TransitionsToHalfOpen()
    {
        var breaker = new MemoryCircuitBreaker(new CircuitBreakerOptions(
            FailureThreshold: 1,
            DurationOfBreak: TimeSpan.FromMilliseconds(50)));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            breaker.ExecuteAsync<int>(() => throw new InvalidOperationException()));

        await Task.Delay(100);

        Assert.Equal(CircuitState.HalfOpen, breaker.State);
    }

    [Fact]
    public async Task ExecuteAsync_HalfOpenSuccess_ClosesCircuit()
    {
        var breaker = new MemoryCircuitBreaker(new CircuitBreakerOptions(
            FailureThreshold: 1,
            DurationOfBreak: TimeSpan.FromMilliseconds(50)));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            breaker.ExecuteAsync<int>(() => throw new InvalidOperationException()));

        await Task.Delay(100);

        var result = await breaker.ExecuteAsync(() => Task.FromResult(42));

        Assert.Equal(CircuitState.Closed, breaker.State);
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task ExecuteAsync_HalfOpenFailure_ReopensCircuit()
    {
        var breaker = new MemoryCircuitBreaker(new CircuitBreakerOptions(
            FailureThreshold: 1,
            DurationOfBreak: TimeSpan.FromMilliseconds(50),
            HalfOpenMaxAttempts: 1));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            breaker.ExecuteAsync<int>(() => throw new InvalidOperationException()));

        await Task.Delay(100);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            breaker.ExecuteAsync<int>(() => throw new InvalidOperationException()));

        Assert.Equal(CircuitState.Open, breaker.State);
    }

    [Fact]
    public async Task Reset_ClosesCircuitAndClearsFailureCount()
    {
        var breaker = new MemoryCircuitBreaker(new CircuitBreakerOptions(FailureThreshold: 1));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            breaker.ExecuteAsync<int>(() => throw new InvalidOperationException()));

        breaker.Reset();

        Assert.Equal(CircuitState.Closed, breaker.State);
    }

    [Fact]
    public async Task State_Initially_ReturnsClosed()
    {
        var breaker = new MemoryCircuitBreaker(new CircuitBreakerOptions());
        Assert.Equal(CircuitState.Closed, breaker.State);
    }
}
