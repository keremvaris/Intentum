using Intentum.Runtime.Resilience.Bulkhead;
using Intentum.Runtime.Resilience.Exceptions;

namespace Intentum.Tests.Resilience;

public sealed class BulkheadTests
{
    [Fact]
    public async Task ExecuteAsync_WithinLimit_ExecutesOperation()
    {
        var bulkhead = new MemoryBulkhead(new BulkheadOptions(MaxParallelization: 2));
        var result = await bulkhead.ExecuteAsync(() => Task.FromResult(42));

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task ExecuteAsync_ExceedsMaxParallelization_QueuesAndExecutes()
    {
        var bulkhead = new MemoryBulkhead(new BulkheadOptions(
            MaxParallelization: 1,
            QueueTimeout: TimeSpan.FromSeconds(5)));

        var task1 = bulkhead.ExecuteAsync(async () =>
        {
            await Task.Delay(500);
            return 1;
        });

        var task2 = bulkhead.ExecuteAsync(async () =>
        {
            await Task.Delay(100);
            return 2;
        });

        var results = await Task.WhenAll(task1, task2);
        Assert.Contains(1, results);
        Assert.Contains(2, results);
    }

    [Fact]
    public async Task ExecuteAsync_ExceedsTimeout_Throws()
    {
        var bulkhead = new MemoryBulkhead(new BulkheadOptions(
            MaxParallelization: 1,
            QueueTimeout: TimeSpan.FromMilliseconds(100)));

        var longTask = bulkhead.ExecuteAsync(async () =>
        {
            await Task.Delay(5000);
            return 1;
        });

        await Task.Delay(50);

        await Assert.ThrowsAsync<BulkheadFullException>(() =>
            bulkhead.ExecuteAsync(() => Task.FromResult(3)));
    }

    [Fact]
    public async Task AvailableSlots_DecreasesDuringExecution()
    {
        var bulkhead = new MemoryBulkhead(new BulkheadOptions(MaxParallelization: 2));

        var task = bulkhead.ExecuteAsync(async () =>
        {
            await Task.Delay(500);
            return 1;
        });

        await Task.Delay(50);
        Assert.Equal(1, bulkhead.AvailableSlots);
        await task;
        Assert.Equal(2, bulkhead.AvailableSlots);
    }

    [Fact]
    public async Task ExecuteAsync_WithCancellationToken_CancelsQueuedOperation()
    {
        var bulkhead = new MemoryBulkhead(new BulkheadOptions(
            MaxParallelization: 1,
            QueueTimeout: TimeSpan.FromSeconds(5)));

        var longTask = bulkhead.ExecuteAsync(async () =>
        {
            await Task.Delay(5000);
            return 1;
        });

        await Task.Delay(50);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(50);

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            bulkhead.ExecuteAsync(() => Task.FromResult(2), cts.Token));
    }

    [Fact]
    public async Task ExecuteAsync_ZeroParallelization_Throws()
    {
        var bulkhead = new MemoryBulkhead(new BulkheadOptions(
            MaxParallelization: 0,
            QueueTimeout: TimeSpan.FromMilliseconds(50)));

        await Assert.ThrowsAsync<BulkheadFullException>(() =>
            bulkhead.ExecuteAsync(() => Task.FromResult(42)));
    }
}
