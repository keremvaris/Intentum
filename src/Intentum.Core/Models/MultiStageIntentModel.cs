using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;

namespace Intentum.Core.Models;

/// <summary>
/// Multi-stage intent model: runs a sequence of models; first result with confidence above threshold wins.
/// E.g. Stage1 = rules, Stage2 = clustering-based, Stage3 = LLM for ambiguous cases only.
/// </summary>
public sealed class MultiStageIntentModel : IIntentModel
{
    private readonly IReadOnlyList<(IIntentModel Model, double ConfidenceThreshold)> _stages;

    /// <summary>
    /// Creates a multi-stage intent model. Each stage is (model, threshold). First stage whose result has confidence >= threshold is returned; otherwise the last stage result is returned.
    /// </summary>
    /// <param name="stages">Ordered list of (model, confidence threshold). Typical: (RuleBasedIntentModel, 0.85), (ClusteringModel, 0.7), (LlmIntentModel, 0). Last threshold is often 0 so the last stage always accepts.</param>
    public MultiStageIntentModel(IEnumerable<(IIntentModel Model, double ConfidenceThreshold)>? stages)
    {
        var list = stages?.ToList() ?? throw new ArgumentNullException(nameof(stages));
        if (list.Count == 0)
            throw new ArgumentException("At least one stage required.", nameof(stages));
        _stages = list.Select(s => (s.Model, Math.Clamp(s.ConfidenceThreshold, 0, 1))).ToList();
    }

    /// <inheritdoc />
    public Intent Infer(BehaviorSpace behaviorSpace, BehaviorVector? precomputedVector = null)
    {
        Intent? lastIntent = null;
        foreach (var (model, threshold) in _stages)
        {
            var intent = model.Infer(behaviorSpace, precomputedVector);
            lastIntent = intent;
            if (intent.Confidence.Score >= threshold)
            {
                var reasoning = intent.Reasoning != null
                    ? intent.Reasoning
                    : $"Stage confidence {intent.Confidence.Score:F2} >= {threshold}";
                return intent with { Reasoning = reasoning };
            }
        }
        var fallback = lastIntent!;
        return fallback with { Reasoning = fallback.Reasoning ?? "Multi-stage: last stage (no threshold met)" };
    }
}
