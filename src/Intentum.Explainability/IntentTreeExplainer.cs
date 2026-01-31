using Intentum.Core.Behavior;
using Intentum.Core.Intents;
using Intentum.Runtime.Engine;
using Intentum.Runtime.Policy;

namespace Intentum.Explainability;

/// <summary>
/// Default implementation of intent decision tree: uses policy engine EvaluateWithRule and intent signals.
/// </summary>
public sealed class IntentTreeExplainer : IIntentTreeExplainer
{
    /// <inheritdoc />
    public IntentDecisionTree GetIntentTree(
        Intent intent,
        IntentPolicy policy,
        BehaviorSpace? behaviorSpace = null)
    {
        var (decision, matchedRule) = IntentPolicyEngine.EvaluateWithRule(intent, policy);

        var intentSummary = new IntentTreeIntentSummary(
            intent.Name,
            intent.Confidence.Level,
            intent.Confidence.Score);

        var signals = intent.Signals
            .Select(s => new IntentTreeSignalNode(s.Source, s.Description, s.Weight))
            .ToList();

        string? behaviorSummary = null;
        if (behaviorSpace is { Events.Count: > 0 })
        {
            var parts = behaviorSpace.Events
                .OrderBy(e => e.OccurredAt)
                .Select(e => $"{e.Actor}:{e.Action}")
                .ToList();
            behaviorSummary = string.Join(" → ", parts);
        }

        return new IntentDecisionTree(
            decision,
            matchedRule?.Name,
            intentSummary,
            signals,
            behaviorSummary);
    }
}
