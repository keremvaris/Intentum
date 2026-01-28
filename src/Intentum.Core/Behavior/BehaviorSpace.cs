namespace Intentum.Core.Behavior;

/// <summary>
/// Observed behavior space; replaces scenario-based tests.
/// </summary>
public sealed class BehaviorSpace
{
    private readonly List<BehaviorEvent> _events = [];
    private readonly Dictionary<string, object> _metadata = new();

    /// <summary>All observed behavior events in order.</summary>
    public IReadOnlyCollection<BehaviorEvent> Events => _events;

    /// <summary>Metadata associated with this behavior space (e.g., sector, domain, session ID).</summary>
    public IReadOnlyDictionary<string, object> Metadata => _metadata;

    /// <summary>Records a single behavior event (actor and action).</summary>
    public void Observe(BehaviorEvent behaviorEvent)
        => _events.Add(behaviorEvent);

    /// <summary>Sets metadata for this behavior space.</summary>
    public void SetMetadata(string key, object value)
        => _metadata[key] = value;

    /// <summary>Gets metadata value by key.</summary>
    public T? GetMetadata<T>(string key)
        => _metadata.TryGetValue(key, out var value) && value is T typed ? typed : default;

    /// <summary>Gets events within a time window.</summary>
    public IReadOnlyCollection<BehaviorEvent> GetEventsInWindow(DateTimeOffset start, DateTimeOffset end)
    {
        return _events
            .Where(e => e.OccurredAt >= start && e.OccurredAt <= end)
            .ToList();
    }

    /// <summary>Gets events within a time window relative to now.</summary>
    public IReadOnlyCollection<BehaviorEvent> GetEventsInWindow(TimeSpan window)
    {
        var end = DateTimeOffset.UtcNow;
        var start = end - window;
        return GetEventsInWindow(start, end);
    }

    /// <summary>Gets the time span of all events.</summary>
    public TimeSpan? GetTimeSpan()
    {
        if (_events.Count == 0)
            return null;

        var earliest = _events.Min(e => e.OccurredAt);
        var latest = _events.Max(e => e.OccurredAt);
        return latest - earliest;
    }

    /// <summary>Builds a behavior vector from observed events (actor:action keys with counts).</summary>
    public BehaviorVector ToVector()
    {
        var dimensions = new Dictionary<string, double>();

        foreach (var evt in _events)
        {
            var key = $"{evt.Actor}:{evt.Action}";
            dimensions[key] = dimensions.GetValueOrDefault(key) + 1;
        }

        return new BehaviorVector(dimensions);
    }

    /// <summary>Builds a behavior vector from events within a time window.</summary>
    public BehaviorVector ToVector(DateTimeOffset start, DateTimeOffset end)
    {
        var dimensions = new Dictionary<string, double>();
        var eventsInWindow = GetEventsInWindow(start, end);

        foreach (var evt in eventsInWindow)
        {
            var key = $"{evt.Actor}:{evt.Action}";
            dimensions[key] = dimensions.GetValueOrDefault(key) + 1;
        }

        return new BehaviorVector(dimensions);
    }
}
