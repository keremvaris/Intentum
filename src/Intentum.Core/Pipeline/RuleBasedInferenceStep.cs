using Intentum.Core.Behavior;
using Intentum.Core.Intents;
using Intentum.Core.Models;

namespace Intentum.Core.Pipeline;

/// <summary>
/// Rule-based inference step: evaluates rules against behavior space and vector; returns raw result (name, score, signals, reasoning).
/// Used by IntentResolutionPipeline; also used by RuleBasedIntentModel internally.
/// </summary>
public sealed class RuleBasedInferenceStep : IIntentInferenceStep
{
    private readonly IReadOnlyList<Func<BehaviorSpace, RuleMatch?>> _rules;

    /// <summary>
    /// Creates a rule-based inference step with the given rules. First matching rule wins.
    /// </summary>
    public RuleBasedInferenceStep(IEnumerable<Func<BehaviorSpace, RuleMatch?>> rules)
    {
        _rules = rules.ToList();
    }

    /// <inheritdoc />
    public IntentInferenceResult Infer(BehaviorSpace behaviorSpace, BehaviorVector vector)
    {
        foreach (var rule in _rules)
        {
            var match = rule(behaviorSpace);
            if (match == null)
                continue;

            var score = Math.Clamp(match.Score, 0, 1);
            var signals = vector.Dimensions.Select(d =>
                new IntentSignal("rule", d.Key, d.Value)).ToList();

            return new IntentInferenceResult(
                Name: match.Name,
                Score: score,
                Signals: signals,
                Reasoning: match.Reasoning
            );
        }

        var unknownSignals = vector.Dimensions.Select(d =>
            new IntentSignal("rule", d.Key, d.Value)).ToList();

        return new IntentInferenceResult(
            Name: "Unknown",
            Score: 0,
            Signals: unknownSignals,
            Reasoning: "No rule matched"
        );
    }
}
