namespace Intentum.Runtime.Resilience.Degradation;

public sealed record DegradationOptions(
    int DegradationThreshold = 5,
    TimeSpan CheckInterval = default,
    double DegradedConfidence = 0.3,
    string DegradedLevel = "Low")
{
    public TimeSpan CheckInterval { get; init; } =
        CheckInterval == default ? TimeSpan.FromSeconds(10) : CheckInterval;
}

public interface IDegradationPolicy
{
    bool IsDegraded { get; }
    Task<T> ExecuteAsync<T>(Func<Task<T>> operation, Func<T> degradedFallback);
    void Reset();
}
