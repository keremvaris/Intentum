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

        var seed = randomSeed ?? Environment.TickCount;
        var space = new BehaviorSpace();
        var time = DateTimeOffset.UtcNow;
        var actorCount = actors.Count;
        var actionCount = actions.Count;

        for (var i = 0; i < eventCount; i++)
        {
            var actorIndex = Math.Abs(Hash(seed, i, 0)) % actorCount;
            var actionIndex = Math.Abs(Hash(seed, i, 1)) % actionCount;
            var timeDelta = 1 + (Math.Abs(Hash(seed, i, 2)) % 59);
            space.Observe(new BehaviorEvent(actors[actorIndex], actions[actionIndex], time));
            time = time.AddSeconds(timeDelta);
        }

        return space;
    }

    private static int Hash(int seed, int index, int salt)
    {
        unchecked
        {
            var h = (seed * 31 + index) * 31 + salt;
            return h == int.MinValue ? 0 : h;
        }
    }
}
