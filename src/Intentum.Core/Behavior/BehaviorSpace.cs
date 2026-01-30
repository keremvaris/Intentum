namespace Intentum.Core.Behavior;

/// <summary>
/// Observed behavior space; replaces scenario-based tests.
/// </summary>
public sealed class BehaviorSpace
{
    private readonly List<BehaviorEvent> _events = [];
    private readonly Dictionary<string, object> _metadata = new();
    private BehaviorVector? _cachedVector;

    /// <summary>All observed behavior events in order.</summary>
    public IReadOnlyCollection<BehaviorEvent> Events => _events;

    /// <summary>Metadata associated with this behavior space (e.g., sector, domain, session ID).</summary>
    public IReadOnlyDictionary<string, object> Metadata => _metadata;

    /// <summary>Records a single behavior event (actor and action). Invalidates cached vector.</summary>
    public void Observe(BehaviorEvent behaviorEvent)
    {
        _cachedVector = null;
        _events.Add(behaviorEvent);
    }

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

    /// <summary>Builds a behavior vector from observed events (actor:action keys with counts). Result is cached until Observe is called.</summary>
    public BehaviorVector ToVector()
        => ToVector(null);

    /// <summary>Builds a behavior vector with optional normalization/cap. Result is cached until Observe is called only when options is null.</summary>
    public BehaviorVector ToVector(ToVectorOptions? options)
    {
        if (options == null || (options.Normalization == VectorNormalization.None && options.CapPerDimension <= 0))
        {
            if (_cachedVector != null)
                return _cachedVector;
        }

        var dimensions = new Dictionary<string, double>();

        foreach (var evt in _events)
        {
            var key = $"{evt.Actor}:{evt.Action}";
            dimensions[key] = dimensions.GetValueOrDefault(key) + 1;
        }

        dimensions = ApplyOptions(dimensions, options);
        var result = new BehaviorVector(dimensions);

        if (options == null || (options.Normalization == VectorNormalization.None && options.CapPerDimension <= 0))
            _cachedVector = result;

        return result;
    }

    /// <summary>Builds a behavior vector from events within a time window.</summary>
    public BehaviorVector ToVector(DateTimeOffset start, DateTimeOffset end)
        => ToVector(start, end, null);

    /// <summary>Builds a behavior vector from events within a time window with optional normalization.</summary>
    public BehaviorVector ToVector(DateTimeOffset start, DateTimeOffset end, ToVectorOptions? options)
    {
        var dimensions = new Dictionary<string, double>();
        var eventsInWindow = GetEventsInWindow(start, end);

        foreach (var evt in eventsInWindow)
        {
            var key = $"{evt.Actor}:{evt.Action}";
            dimensions[key] = dimensions.GetValueOrDefault(key) + 1;
        }

        dimensions = ApplyOptions(dimensions, options);
        return new BehaviorVector(dimensions);
    }

    private static Dictionary<string, double> ApplyOptions(Dictionary<string, double> dimensions, ToVectorOptions? options)
    {
        if (options == null || (options.Normalization == VectorNormalization.None && options.CapPerDimension <= 0))
            return dimensions;

        var cap = options.CapPerDimension > 0 ? options.CapPerDimension : (double?)null;

        switch (options.Normalization)
        {
            case VectorNormalization.Cap when cap.HasValue:
                foreach (var k in dimensions.Keys.ToList())
                {
                    if (dimensions[k] > cap.Value)
                        dimensions[k] = cap.Value;
                }
                break;
            case VectorNormalization.SoftCap when cap.HasValue:
                foreach (var k in dimensions.Keys.ToList())
                    dimensions[k] = Math.Min(1.0, dimensions[k] / cap.Value);
                break;
            case VectorNormalization.L1:
                var sum = dimensions.Values.Sum();
                if (sum > 0)
                {
                    foreach (var k in dimensions.Keys.ToList())
                        dimensions[k] /= sum;
                }
                break;
            default:
                if (cap.HasValue && options.Normalization == VectorNormalization.None)
                {
                    foreach (var k in dimensions.Keys.ToList())
                    {
                        if (dimensions[k] > cap.Value)
                            dimensions[k] = cap.Value;
                    }
                }
                break;
        }

        return dimensions;
    }
}
