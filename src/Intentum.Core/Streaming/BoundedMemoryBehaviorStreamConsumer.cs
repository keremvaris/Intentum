using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Intentum.Core.Streaming;

/// <summary>
/// In-memory behavior stream consumer with bounded capacity for backpressure.
/// When the channel is full, <see cref="PostAsync"/> waits until space is available (backpressure).
/// </summary>
public sealed class BoundedMemoryBehaviorStreamConsumer : IBehaviorStreamConsumer
{
    private readonly Channel<BehaviorEventBatch> _channel;

    /// <summary>
    /// Creates a bounded consumer. When capacity is reached, PostAsync will wait (backpressure).
    /// </summary>
    /// <param name="capacity">Maximum number of batches to buffer; must be positive.</param>
    public BoundedMemoryBehaviorStreamConsumer(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Capacity must be positive.");
        _channel = Channel.CreateBounded<BehaviorEventBatch>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        });
    }

    /// <summary>
    /// Posts a batch to the stream. If the channel is full, waits until space is available (backpressure).
    /// </summary>
    public ValueTask PostAsync(BehaviorEventBatch batch, CancellationToken cancellationToken = default)
    {
        return _channel.Writer.WriteAsync(batch, cancellationToken);
    }

    /// <summary>
    /// Completes the writer so ReadAllAsync will finish after all posted batches are read.
    /// </summary>
    public void Complete()
    {
        _channel.Writer.Complete();
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<BehaviorEventBatch> ReadAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var batch in _channel.Reader.ReadAllAsync(cancellationToken))
            yield return batch;
    }
}
