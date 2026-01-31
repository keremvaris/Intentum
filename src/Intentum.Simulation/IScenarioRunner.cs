using Intentum.Core.Contracts;
using Intentum.Runtime.Policy;

namespace Intentum.Simulation;

/// <summary>
/// Runs a set of behavior scenarios through intent model and policy, returning results for each.
/// </summary>
public interface IScenarioRunner
{
    /// <summary>
    /// Runs all scenarios: builds BehaviorSpace per scenario (FromSequence or GenerateRandom), runs Infer and Decide, returns results.
    /// </summary>
    Task<IReadOnlyList<ScenarioRunResult>> RunAsync(
        IReadOnlyList<BehaviorScenario> scenarios,
        IIntentModel model,
        IntentPolicy policy,
        CancellationToken cancellationToken = default);
}
