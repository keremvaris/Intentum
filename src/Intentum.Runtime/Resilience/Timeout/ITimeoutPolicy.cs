namespace Intentum.Runtime.Resilience.Timeout;

public sealed record TimeoutOptions(
    TimeSpan TimeoutDuration = default)
{
    public TimeSpan TimeoutDuration { get; init; } =
        TimeoutDuration == default ? TimeSpan.FromSeconds(5) : TimeoutDuration;
}

public interface ITimeoutPolicy
{
    Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default);
}
