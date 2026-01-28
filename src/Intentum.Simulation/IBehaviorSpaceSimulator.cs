using Intentum.Core.Behavior;

namespace Intentum.Simulation;

/// <summary>
/// Generates synthetic behavior spaces for testing and simulation.
/// </summary>
public interface IBehaviorSpaceSimulator
{
    /// <summary>
    /// Generates a synthetic behavior space from a sequence of (actor, action) pairs.
    /// </summary>
    /// <param name="sequence">Sequence of (actor, action).</param>
    /// <param name="baseTime">Base timestamp for events (default UtcNow).</param>
    /// <returns>A new behavior space with the given events.</returns>
    BehaviorSpace FromSequence(
        IReadOnlyList<(string Actor, string Action)> sequence,
        DateTimeOffset? baseTime = null);

    /// <summary>
    /// Generates a random behavior space with the given actors, actions, and event count.
    /// </summary>
    /// <param name="actors">Possible actors.</param>
    /// <param name="actions">Possible actions.</param>
    /// <param name="eventCount">Number of events to generate.</param>
    /// <param name="randomSeed">Optional seed for reproducibility.</param>
    /// <returns>A new behavior space with random events.</returns>
    BehaviorSpace GenerateRandom(
        IReadOnlyList<string> actors,
        IReadOnlyList<string> actions,
        int eventCount,
        int? randomSeed = null);
}
