using Intentum.Core.Intents;

namespace Intentum.Core.Evaluation;

/// <summary>Result of evaluating a behavior space: the inferred intent and the behavior vector used.</summary>
/// <param name="Intent">The inferred intent (confidence and signals).</param>
/// <param name="BehaviorVector">The behavior vector built from observed events.</param>
public sealed record IntentEvaluationResult(
    Intent Intent,
    Behavior.BehaviorVector BehaviorVector
);
