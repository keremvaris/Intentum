namespace Intentum.Core.Streaming;

/// <summary>
/// Options for streaming signal ingestion: backpressure and window-based buffering.
/// </summary>
/// <param name="BoundedCapacity">Optional capacity for the ingestion channel; when set, full mode applies backpressure (writer waits when full).</param>
/// <param name="WindowDuration">Optional time window for buffering; events are batched until the window elapses or MaxWindowSize is reached.</param>
/// <param name="MaxWindowSize">Optional max events per window batch; when reached, batch is emitted even if window has not elapsed.</param>
public sealed record StreamIngestionOptions(
    int? BoundedCapacity = null,
    TimeSpan? WindowDuration = null,
    int? MaxWindowSize = null
);
