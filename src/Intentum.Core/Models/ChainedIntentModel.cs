using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;

namespace Intentum.Core.Models;

/// <summary>
/// Intent model that tries a primary model first; if confidence is below threshold, falls back to a secondary model (e.g. LLM).
/// Reduces cost and latency by using cheap rule/keyword path when confidence is high.
/// </summary>
public sealed class ChainedIntentModel : IIntentModel
{
    private readonly IIntentModel _primary;
    private readonly IIntentModel _fallback;
    private readonly double _confidenceThreshold;

    /// <summary>
    /// Creates a chained intent model.
    /// </summary>
    /// <param name="primary">Model to try first (e.g. RuleBasedIntentModel).</param>
    /// <param name="fallback">Model to use when primary confidence is below threshold (e.g. LlmIntentModel).</param>
    /// <param name="confidenceThreshold">Minimum primary confidence to accept primary result; otherwise use fallback. Typical 0.7–0.85.</param>
    public ChainedIntentModel(IIntentModel primary, IIntentModel fallback, double confidenceThreshold = 0.7)
    {
        _primary = primary ?? throw new ArgumentNullException(nameof(primary));
        _fallback = fallback ?? throw new ArgumentNullException(nameof(fallback));
        _confidenceThreshold = Math.Clamp(confidenceThreshold, 0, 1);
    }

    /// <inheritdoc />
    public Intent Infer(BehaviorSpace behaviorSpace, BehaviorVector? precomputedVector = null)
    {
        var primaryIntent = _primary.Infer(behaviorSpace, precomputedVector);

        if (primaryIntent.Confidence.Score >= _confidenceThreshold)
        {
            var reasoning = primaryIntent.Reasoning != null
                ? $"Primary: {primaryIntent.Reasoning}"
                : "Primary (confidence above threshold)";
            return primaryIntent with { Reasoning = reasoning };
        }

        var fallbackIntent = _fallback.Infer(behaviorSpace, precomputedVector);
        var fallbackReasoning = $"Fallback: {fallbackIntent.Reasoning ?? "LLM (primary confidence below " + _confidenceThreshold + ")"}";
        return fallbackIntent with { Reasoning = fallbackReasoning };
    }
}
