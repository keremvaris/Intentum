using Intentum.Core.Behavior;

namespace Intentum.Core.Streaming;

/// <summary>
/// A batch of behavior events from a stream (e.g. Kafka, in-memory queue).
/// </summary>
/// <param name="Events">Ordered list of behavior events.</param>
public sealed record BehaviorEventBatch(IReadOnlyList<BehaviorEvent> Events);
