using Intentum.Core.Behavior;
using Intentum.Core.Intent;
using Intentum.Core.Intents;

namespace Intentum.Core.Evaluation;

/// <summary>
/// Infers intent and confidence from a behavior space.
/// </summary>
public sealed class IntentEvaluator
{
    /// <summary>Evaluates the given behavior space and returns an intent with confidence and signals.</summary>
    public IntentEvaluationResult Evaluate(
        string intentName,
        BehaviorSpace behaviorSpace)
    {
        var vector = behaviorSpace.ToVector();

        var signals = vector.Dimensions.Select(d =>
            new IntentSignal(
                Source: "behavior",
                Description: d.Key,
                Weight: Normalize(d.Value)
            )).ToList();

        var score = signals.Sum(s => s.Weight) / Math.Max(1, signals.Count);

        var confidence = IntentConfidence.FromScore(score);

        var intent = new Intents.Intent(
            intentName,
            signals,
            confidence
        );

        return new IntentEvaluationResult(intent, vector);
    }

    private static double Normalize(double value)
        => Math.Min(1.0, value / 10.0);
}
