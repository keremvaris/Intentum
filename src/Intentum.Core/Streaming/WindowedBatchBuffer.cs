using Intentum.Core.Behavior;

namespace Intentum.Core.Streaming;

/// <summary>
/// Buffers behavior events and emits a batch when the time window elapses or max size is reached (window-based buffering).
/// Call <see cref="Add"/> to append events; call <see cref="FlushWindow"/> to get the current buffer as a batch and clear it.
/// </summary>
public sealed class WindowedBatchBuffer
{
    private readonly List<BehaviorEvent> _buffer = [];
    private readonly TimeSpan? _windowDuration;
    private readonly int? _maxWindowSize;
    private DateTimeOffset? _windowStart;

    /// <summary>
    /// Creates a windowed buffer. When flushing, all events since the last flush (or since first Add) are returned.
    /// </summary>
    /// <param name="windowDuration">Optional: after this duration, FlushWindow returns a batch (caller can call FlushWindow on a timer).</param>
    /// <param name="maxWindowSize">Optional: when buffer reaches this size, FlushWindow is implied (caller should check and flush).</param>
    public WindowedBatchBuffer(TimeSpan? windowDuration = null, int? maxWindowSize = null)
    {
        _windowDuration = windowDuration;
        _maxWindowSize = maxWindowSize;
    }

    /// <summary>
    /// Adds an event to the buffer. If MaxWindowSize is set and buffer reaches it, the buffer is full; caller should flush.
    /// </summary>
    public void Add(BehaviorEvent evt)
    {
        if (_windowStart == null)
            _windowStart = DateTimeOffset.UtcNow;
        _buffer.Add(evt);
    }

    /// <summary>
    /// Returns true if the window should be flushed (duration elapsed or max size reached).
    /// </summary>
    public bool ShouldFlush()
    {
        if (_buffer.Count == 0)
            return false;
        if (_maxWindowSize.HasValue && _buffer.Count >= _maxWindowSize.Value)
            return true;
        if (_windowDuration.HasValue && _windowStart.HasValue &&
            DateTimeOffset.UtcNow - _windowStart.Value >= _windowDuration.Value)
            return true;
        return false;
    }

    /// <summary>
    /// Returns the current buffer as a batch and clears the buffer. Returns null if buffer is empty.
    /// </summary>
    public BehaviorEventBatch? FlushWindow()
    {
        if (_buffer.Count == 0)
            return null;
        var batch = new BehaviorEventBatch(_buffer.ToList());
        _buffer.Clear();
        _windowStart = null;
        return batch;
    }

    /// <summary>
    /// Current number of events in the buffer.
    /// </summary>
    public int Count => _buffer.Count;
}
