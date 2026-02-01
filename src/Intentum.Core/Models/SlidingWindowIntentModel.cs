using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;

namespace Intentum.Core.Models;

/// <summary>
/// Wraps an intent model and restricts inference to a sliding time window (configurable window, deterministic for testing).
/// Use when you want intent resolution based only on recent events (e.g. last 5 minutes).
/// For decay (recent events weigh more), use an inner model with Intentum.AI.Similarity.TimeDecaySimilarityEngine.
/// </summary>
public sealed class SlidingWindowIntentModel : IIntentModel
{
    private readonly IIntentModel _inner;
    private readonly TimeSpan _windowSize;
    private readonly DateTimeOffset? _referenceTimeForDeterminism;

    /// <summary>
    /// Creates a sliding-window intent model.
    /// </summary>
    /// <param name="inner">The inner intent model (e.g. rule-based or LLM).</param>
    /// <param name="windowSize">Time window (e.g. last 5 minutes). Only events within this window are used for inference.</param>
    /// <param name="referenceTimeForDeterminism">Optional: fix the "now" for the window end so that tests are deterministic. When null, DateTimeOffset.UtcNow is used.</param>
    public SlidingWindowIntentModel(
        IIntentModel inner,
        TimeSpan windowSize,
        DateTimeOffset? referenceTimeForDeterminism = null)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _windowSize = windowSize;
        _referenceTimeForDeterminism = referenceTimeForDeterminism;
    }

    /// <inheritdoc />
    public Intent Infer(BehaviorSpace behaviorSpace, BehaviorVector? precomputedVector = null)
    {
        if (behaviorSpace.Events.Count == 0)
            return _inner.Infer(behaviorSpace, precomputedVector);

        var end = _referenceTimeForDeterminism ?? DateTimeOffset.UtcNow;
        var start = end - _windowSize;
        var eventsInWindow = behaviorSpace.GetEventsInWindow(start, end);

        if (eventsInWindow.Count == 0)
            return _inner.Infer(behaviorSpace, precomputedVector);

        var windowedSpace = new BehaviorSpace();
        foreach (var evt in eventsInWindow)
            windowedSpace.Observe(evt);
        foreach (var kv in behaviorSpace.Metadata)
            windowedSpace.SetMetadata(kv.Key, kv.Value);

        return _inner.Infer(windowedSpace, precomputedVector: null);
    }
}
