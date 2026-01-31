using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Intentum.Core.Streaming;

/// <summary>
/// In-memory behavior stream consumer: producer posts batches via Post; consumer reads via ReadAllAsync.
/// Useful for testing and single-node scenarios.
/// </summary>
public sealed class MemoryBehaviorStreamConsumer : IBehaviorStreamConsumer
{
    private readonly Channel<BehaviorEventBatch> _channel = Channel.CreateUnbounded<BehaviorEventBatch>(
        new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });

    /// <summary>
    /// Posts a batch to the stream. Consumers will receive it from ReadAllAsync.
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
