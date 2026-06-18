namespace Intentum.Runtime.Resilience.Timeout;

public sealed class MemoryTimeoutPolicy : ITimeoutPolicy
{
    private readonly TimeoutOptions _options;

    public MemoryTimeoutPolicy(TimeoutOptions options)
    {
        _options = options;
    }

    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(_options.TimeoutDuration);

        try
        {
            return await operation(timeoutCts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TaskCanceledException("Operation timed out.");
        }
    }
}
