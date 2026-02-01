using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;
using Intentum.Core.Pipeline;

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
/// Uses the resolution pipeline internally (signal → vector → rule inference → confidence).
/// Use as the primary model in ChainedIntentModel so that LLM is only called when rules do not match or confidence is low.
/// </summary>
public sealed class RuleBasedIntentModel : IIntentModel
{
    private readonly IntentResolutionPipeline _pipeline;

    /// <summary>
    /// Creates a rule-based intent model with the given rules. First matching rule wins.
    /// </summary>
    /// <param name="rules">Ordered list of rules. Each rule returns a RuleMatch when it applies, or null to skip.</param>
    public RuleBasedIntentModel(IEnumerable<Func<BehaviorSpace, RuleMatch?>> rules)
    {
        var step = new RuleBasedInferenceStep(rules ?? throw new ArgumentNullException(nameof(rules)));
        _pipeline = new IntentResolutionPipeline(step);
    }

    /// <inheritdoc />
    public Intent Infer(BehaviorSpace behaviorSpace, BehaviorVector? precomputedVector = null)
        => _pipeline.Infer(behaviorSpace, precomputedVector);
}
