using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;

namespace Intentum.Core.Models;

/// <summary>
/// Result of a rule evaluation: intent name, confidence score, and optional reasoning.
/// </summary>
/// <param name="Name">Intent name when the rule matches.</param>
/// <param name="Score">Confidence score in [0, 1].</param>
/// <param name="Reasoning">Optional human-readable explanation (e.g. "login.failed >= 2 and password.reset").</param>
public sealed record RuleMatch(string Name, double Score, string? Reasoning = null);

/// <summary>
/// Intent inference using rule-based evaluation (no LLM). Fast, deterministic, explainable.
/// Use as the primary model in ChainedIntentModel so that LLM is only called when rules do not match or confidence is low.
/// </summary>
public sealed class RuleBasedIntentModel : IIntentModel
{
    private readonly IReadOnlyList<Func<BehaviorSpace, RuleMatch?>> _rules;

    /// <summary>
    /// Creates a rule-based intent model with the given rules. First matching rule wins.
    /// </summary>
    /// <param name="rules">Ordered list of rules. Each rule returns a RuleMatch when it applies, or null to skip.</param>
    public RuleBasedIntentModel(IEnumerable<Func<BehaviorSpace, RuleMatch?>> rules)
    {
        _rules = rules.ToList();
    }

    /// <inheritdoc />
    public Intent Infer(BehaviorSpace behaviorSpace, BehaviorVector? precomputedVector = null)
    {
        var vector = precomputedVector ?? behaviorSpace.ToVector();

        foreach (var rule in _rules)
        {
            var match = rule(behaviorSpace);
            if (match == null)
                continue;

            var confidence = IntentConfidence.FromScore(Math.Clamp(match.Score, 0, 1));
            var signals = vector.Dimensions.Select(d =>
                new IntentSignal("rule", d.Key, d.Value)).ToList();

            return new Intent(
                Name: match.Name,
                Signals: signals,
                Confidence: confidence,
                Reasoning: match.Reasoning
            );
        }

        var unknownConfidence = IntentConfidence.FromScore(0);
        var unknownSignals = vector.Dimensions.Select(d =>
            new IntentSignal("rule", d.Key, d.Value)).ToList();

        return new Intent(
            Name: "Unknown",
            Signals: unknownSignals,
            Confidence: unknownConfidence,
            Reasoning: "No rule matched"
        );
    }
}
