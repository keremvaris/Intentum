using System.Diagnostics;
using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

namespace Intentum.Simulation;

/// <summary>
/// Runs behavior scenarios through intent model and policy using BehaviorSpaceSimulator.
/// </summary>
public sealed class IntentScenarioRunner(IBehaviorSpaceSimulator simulator) : IScenarioRunner
{
    private readonly IBehaviorSpaceSimulator _simulator = simulator ?? throw new ArgumentNullException(nameof(simulator));

    /// <inheritdoc />
    public async Task<IReadOnlyList<ScenarioRunResult>> RunAsync(
        IReadOnlyList<BehaviorScenario>? scenarios,
        IIntentModel model,
        IntentPolicy policy,
        CancellationToken cancellationToken = default)
    {
        var results = new List<ScenarioRunResult>();
        foreach (var scenario in scenarios ?? [])
        {
            cancellationToken.ThrowIfCancellationRequested();

            var space = BuildSpace(scenario);
            var sw = Stopwatch.StartNew();
            var intent = model.Infer(space);
            var decision = intent.Decide(policy);
            sw.Stop();

            results.Add(new ScenarioRunResult(
                scenario.Id,
                intent,
                decision,
                sw.Elapsed.TotalMilliseconds));
        }
        return await Task.FromResult(results).ConfigureAwait(false);
    }

    private BehaviorSpace BuildSpace(BehaviorScenario scenario)
    {
        if (scenario.Sequence is { Count: > 0 })
        {
            var seq = scenario.Sequence.Select(t => (t.Actor, t.Action)).ToList();
            return _simulator.FromSequence(seq);
        }
        if (scenario is { Actors: { Count: > 0 }, Actions.Count: > 0 })
            return _simulator.GenerateRandom(scenario.Actors, scenario.Actions, scenario.EventCount, scenario.RandomSeed);
        throw new ArgumentException($"Scenario {scenario.Id}: provide Sequence or (Actors + Actions).", nameof(scenario));
    }
}
