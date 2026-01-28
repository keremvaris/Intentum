namespace Intentum.Core.Behavior;

/// <summary>
/// Observed behavior space; replaces scenario-based tests.
/// </summary>
public sealed class BehaviorSpace
{
    private readonly List<BehaviorEvent> _events = [];

    public IReadOnlyCollection<BehaviorEvent> Events => _events;

    public void Observe(BehaviorEvent behaviorEvent)
        => _events.Add(behaviorEvent);

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
