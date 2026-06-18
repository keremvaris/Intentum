using Intentum.Runtime.Resilience.Exceptions;

namespace Intentum.Runtime.Resilience.Bulkhead;

public sealed class MemoryBulkhead : IBulkhead
{
    private readonly BulkheadOptions _options;
    private readonly SemaphoreSlim _semaphore;

    public MemoryBulkhead(BulkheadOptions options)
    {
        _options = options;
        var max = Math.Max(1, options.MaxParallelization);
        _semaphore = new SemaphoreSlim(max, max);
    }

    public int AvailableSlots => _semaphore.CurrentCount;
    public int QueueSize => _options.MaxParallelization - _semaphore.CurrentCount;

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
    {
        if (_options.MaxParallelization <= 0)
            throw new BulkheadFullException();

        if (!await _semaphore.WaitAsync(_options.QueueTimeout, cancellationToken))
            throw new BulkheadFullException();

        try
        {
            return await operation();
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
