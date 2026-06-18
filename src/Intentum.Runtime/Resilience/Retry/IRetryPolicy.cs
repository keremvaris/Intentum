namespace Intentum.Runtime.Resilience.Retry;

public sealed record RetryOptions(
    int MaxRetries = 3,
    TimeSpan BaseDelay = default,
    RetryBackoffType Backoff = RetryBackoffType.Exponential)
{
    public TimeSpan BaseDelay { get; init; } =
        BaseDelay == default ? TimeSpan.FromMilliseconds(100) : BaseDelay;
}

public enum RetryBackoffType { Constant, Linear, Exponential }

public interface IRetryPolicy
{
    Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default);
}
