using Intentum.Core.Behavior;

namespace Intentum.Simulation;

/// <summary>
/// Default implementation of behavior space simulation.
/// </summary>
public sealed class BehaviorSpaceSimulator : IBehaviorSpaceSimulator
{
    /// <inheritdoc />
    public BehaviorSpace FromSequence(
        IReadOnlyList<(string Actor, string Action)> sequence,
        DateTimeOffset? baseTime = null)
    {
        var space = new BehaviorSpace();
        var time = baseTime ?? DateTimeOffset.UtcNow;
        foreach (var (actor, action) in sequence)
        {
            space.Observe(new BehaviorEvent(actor, action, time));
            time = time.AddSeconds(1);
        }
        return space;
    }

    /// <inheritdoc />
    public BehaviorSpace GenerateRandom(
        IReadOnlyList<string> actors,
        IReadOnlyList<string> actions,
        int eventCount,
        int? randomSeed = null)
    {
        if (actors is null || actors.Count == 0)
            throw new ArgumentException("At least one actor required.", nameof(actors));
        if (actions is null || actions.Count == 0)
            throw new ArgumentException("At least one action required.", nameof(actions));
        if (eventCount < 0)
            throw new ArgumentOutOfRangeException(nameof(eventCount), "Event count must be non-negative.");

        var rnd = randomSeed is { } seed ? new Random(seed) : Random.Shared;
        var space = new BehaviorSpace();
        var time = DateTimeOffset.UtcNow;

        for (var i = 0; i < eventCount; i++)
        {
            var actor = actors[rnd.Next(actors.Count)];
            var action = actions[rnd.Next(actions.Count)];
            space.Observe(new BehaviorEvent(actor, action, time));
            time = time.AddSeconds(rnd.Next(1, 60));
        }

        return space;
    }
}
