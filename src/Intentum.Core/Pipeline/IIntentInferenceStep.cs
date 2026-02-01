using Intentum.Core.Behavior;

namespace Intentum.Core.Pipeline;

/// <summary>
/// Infers intent from behavior space and vector (raw name, score, signals, reasoning).
/// Pipeline applies confidence level separately via IConfidenceCalculator.
/// Override this step to plug in rule-based, LLM, or hybrid inference.
/// </summary>
public interface IIntentInferenceStep
{
    /// <summary>Infers intent from the behavior space and precomputed vector.</summary>
    /// <param name="behaviorSpace">The observed behavior space.</param>
    /// <param name="vector">Precomputed behavior vector (e.g. from ISignalToVector).</param>
    IntentInferenceResult Infer(BehaviorSpace behaviorSpace, BehaviorVector vector);
}
