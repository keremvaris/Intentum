namespace Intentum.Core.Behavior;

/// <summary>
/// Observed behavior space; replaces scenario-based tests.
/// </summary>
public sealed class BehaviorSpace
{
    private readonly List<BehaviorEvent> _events = [];

    /// <summary>All observed behavior events in order.</summary>
    public IReadOnlyCollection<BehaviorEvent> Events => _events;

    /// <summary>Records a single behavior event (actor and action).</summary>
    public void Observe(BehaviorEvent behaviorEvent)
        => _events.Add(behaviorEvent);

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
}
