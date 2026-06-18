using Intentum.Runtime.Resilience.Exceptions;

namespace Intentum.Runtime.Resilience.CircuitBreaker;

public sealed record CircuitBreakerOptions(
    int FailureThreshold = 3,
    TimeSpan DurationOfBreak = default,
    int HalfOpenMaxAttempts = 1)
{
    public TimeSpan DurationOfBreak { get; init; } =
        DurationOfBreak == default ? TimeSpan.FromSeconds(30) : DurationOfBreak;
}

public enum CircuitState { Closed, Open, HalfOpen }

public interface ICircuitBreaker
{
    CircuitState State { get; }
    Task<T> ExecuteAsync<T>(Func<Task<T>> operation);
    void Reset();
}
