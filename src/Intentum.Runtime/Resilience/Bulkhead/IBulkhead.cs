namespace Intentum.Runtime.Resilience.Bulkhead;

public sealed record BulkheadOptions(
    int MaxParallelization = 10,
    int MaxQueuingItems = 10,
    TimeSpan QueueTimeout = default)
{
    public TimeSpan QueueTimeout { get; init; } =
        QueueTimeout == default ? TimeSpan.FromSeconds(30) : QueueTimeout;
}

public interface IBulkhead
{
    int AvailableSlots { get; }
    int QueueSize { get; }
    Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default);
}
