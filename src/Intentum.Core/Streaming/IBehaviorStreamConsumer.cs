namespace Intentum.Core.Streaming;

/// <summary>
/// Consumes a stream of behavior event batches (e.g. from Kafka, Azure Event Hubs, or in-memory).
/// Used for real-time intent inference: each batch can be turned into a BehaviorSpace, then Infer and Decide.
/// </summary>
public interface IBehaviorStreamConsumer
{
    /// <summary>
    /// Reads all batches from the stream until cancellation. Use in a loop: await foreach (var batch in consumer.ReadAllAsync(ct)) { ... }.
    /// </summary>
    IAsyncEnumerable<BehaviorEventBatch> ReadAllAsync(CancellationToken cancellationToken = default);
}
