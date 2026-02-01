using Intentum.Core.Behavior;
using Intentum.Core.Streaming;

namespace Intentum.Tests;

public sealed class BehaviorEventBatchAndStreamConsumerTests
{
    [Fact]
    public void BehaviorEventBatch_StoresEvents()
    {
        var events = new List<BehaviorEvent>
        {
            new("user", "login", DateTimeOffset.UtcNow),
            new("user", "submit", DateTimeOffset.UtcNow)
        };
        var batch = new BehaviorEventBatch(events);

        Assert.Equal(2, batch.Events.Count);
        Assert.Equal("login", batch.Events[0].Action);
    }

    [Fact]
    public async Task MemoryBehaviorStreamConsumer_PostAndReadAll_ReceivesBatches()
    {
        var consumer = new MemoryBehaviorStreamConsumer();
        var e1 = new BehaviorEvent("a", "x", DateTimeOffset.UtcNow);
        var batch1 = new BehaviorEventBatch([e1]);
        var batch2 = new BehaviorEventBatch([]);

        _ = Task.Run(async () =>
        {
            await consumer.PostAsync(batch1);
            await consumer.PostAsync(batch2);
            consumer.Complete();
        });

        var received = new List<BehaviorEventBatch>();
        await foreach (var b in consumer.ReadAllAsync())
            received.Add(b);

        Assert.Equal(2, received.Count);
        Assert.Single(received[0].Events);
        Assert.Equal("x", received[0].Events[0].Action);
        Assert.Empty(received[1].Events);
    }

    [Fact]
    public async Task MemoryBehaviorStreamConsumer_Complete_ReadAllAsyncExits()
    {
        var consumer = new MemoryBehaviorStreamConsumer();
        consumer.Complete();

        var count = 0;
        await foreach (var _ in consumer.ReadAllAsync())
            count++;

        Assert.Equal(0, count);
    }
}
