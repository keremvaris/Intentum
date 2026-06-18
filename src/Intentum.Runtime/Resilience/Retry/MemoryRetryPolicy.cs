namespace Intentum.Runtime.Resilience.Retry;

public sealed class MemoryRetryPolicy : IRetryPolicy
{
    private readonly RetryOptions _options;

    public MemoryRetryPolicy(RetryOptions options)
    {
        _options = options;
    }

    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)
    {
        var attempts = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                attempts++;
                return await operation(cancellationToken);
            }
            catch (Exception) when (attempts <= _options.MaxRetries)
            {
                var delay = CalculateDelay(attempts);
                await Task.Delay(delay, cancellationToken);
            }
        }
    }

    private TimeSpan CalculateDelay(int attempt)
    {
        return _options.Backoff switch
        {
            RetryBackoffType.Constant => _options.BaseDelay,
            RetryBackoffType.Linear => _options.BaseDelay * attempt,
            RetryBackoffType.Exponential => TimeSpan.FromMilliseconds(
                _options.BaseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1)),
            _ => _options.BaseDelay
        };
    }
}
