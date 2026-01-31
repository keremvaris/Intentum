namespace Intentum.Simulation;

/// <summary>
/// A scenario definition: either a fixed sequence of (actor, action) or simulator parameters for random generation.
/// </summary>
/// <param name="Id">Scenario identifier.</param>
/// <param name="Name">Human-readable name.</param>
/// <param name="Sequence">Optional fixed sequence (actor, action). When set, used to build BehaviorSpace via FromSequence.</param>
/// <param name="Actors">For random generation: list of actor names.</param>
/// <param name="Actions">For random generation: list of action names.</param>
/// <param name="EventCount">For random generation: number of events.</param>
/// <param name="RandomSeed">For random generation: optional seed.</param>
public sealed record BehaviorScenario(
    string Id,
    string Name,
    IReadOnlyList<(string Actor, string Action)>? Sequence = null,
    IReadOnlyList<string>? Actors = null,
    IReadOnlyList<string>? Actions = null,
    int EventCount = 10,
    int? RandomSeed = null);
