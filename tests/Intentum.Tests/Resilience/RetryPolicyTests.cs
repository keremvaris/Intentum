using Intentum.Runtime.Resilience.Retry;

namespace Intentum.Tests.Resilience;

public sealed class RetryPolicyTests
{
    [Fact]
    public async Task ExecuteAsync_NoFailure_ReturnsResult()
    {
        var retry = new MemoryRetryPolicy(new RetryOptions());
        var result = await retry.ExecuteAsync((_) => Task.FromResult(42));

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task ExecuteAsync_TransientFailure_RetriesAndSucceeds()
    {
        var retry = new MemoryRetryPolicy(new RetryOptions(MaxRetries: 3, BaseDelay: TimeSpan.FromMilliseconds(10)));
        var attempts = 0;

        var result = await retry.ExecuteAsync<int>((_) =>
        {
            attempts++;
            return attempts < 3
                ? throw new InvalidOperationException("Transient")
                : Task.FromResult(42);
        });

        Assert.Equal(42, result);
        Assert.Equal(3, attempts);
    }

    [Fact]
    public async Task ExecuteAsync_ExceedsMaxRetries_ThrowsLastException()
    {
        var retry = new MemoryRetryPolicy(new RetryOptions(MaxRetries: 2, BaseDelay: TimeSpan.FromMilliseconds(10)));
        var attempts = 0;

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            retry.ExecuteAsync<int>((_) =>
            {
                attempts++;
                throw new InvalidOperationException($"Attempt {attempts}");
            }));

        Assert.Contains("Attempt 3", ex.Message);
        Assert.Equal(3, attempts);
    }

    [Fact]
    public async Task ExecuteAsync_ExponentialBackoff_IncreasingDelays()
    {
        var retry = new MemoryRetryPolicy(new RetryOptions(
            MaxRetries: 3,
            BaseDelay: TimeSpan.FromMilliseconds(10),
            Backoff: RetryBackoffType.Exponential));
        var attempts = 0;

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            retry.ExecuteAsync<int>((_) =>
            {
                attempts++;
                throw new InvalidOperationException();
            }));

        Assert.Equal(4, attempts);
    }

    [Fact]
    public async Task ExecuteAsync_WithCancellationToken_CancelsOperation()
    {
        var retry = new MemoryRetryPolicy(new RetryOptions(MaxRetries: 3, BaseDelay: TimeSpan.FromSeconds(1)));
        using var cts = new CancellationTokenSource();

        var task = retry.ExecuteAsync<int>(async ct =>
        {
            await Task.Delay(100, ct);
            throw new InvalidOperationException();
        }, cts.Token);

        cts.Cancel();
        await Assert.ThrowsAsync<TaskCanceledException>(() => task);
    }

    [Fact]
    public async Task ExecuteAsync_ZeroRetries_DoesNotRetry()
    {
        var retry = new MemoryRetryPolicy(new RetryOptions(MaxRetries: 0, BaseDelay: TimeSpan.FromMilliseconds(10)));
        var attempts = 0;

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            retry.ExecuteAsync<int>((_) =>
            {
                attempts++;
                throw new InvalidOperationException();
            }));

        Assert.Equal(1, attempts);
    }
}
