using Intentum.Core.Intents;
using Intentum.Runtime.Policy;

namespace Intentum.Simulation;

/// <summary>
/// Result of running one scenario: intent, decision, and duration.
/// </summary>
/// <param name="ScenarioId">Scenario identifier.</param>
/// <param name="Intent">Inferred intent.</param>
/// <param name="Decision">Policy decision.</param>
/// <param name="DurationMs">Inference + decision duration in milliseconds.</param>
public sealed record ScenarioRunResult(
    string ScenarioId,
    Intent Intent,
    PolicyDecision Decision,
    double DurationMs);
