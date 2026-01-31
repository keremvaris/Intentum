using Intentum.Core.Behavior;
using Intentum.Core.Intents;
using Intentum.Runtime.Policy;

namespace Intentum.Explainability;

/// <summary>
/// Builds an intent decision tree for root-cause analysis: decision, matched rule, intent summary, and signals.
/// </summary>
public interface IIntentTreeExplainer
{
    /// <summary>
    /// Builds the intent decision tree for the given intent and policy.
    /// Optionally includes behavior event summary when <paramref name="behaviorSpace"/> is provided.
    /// </summary>
    IntentDecisionTree GetIntentTree(
        Intent intent,
        IntentPolicy policy,
        BehaviorSpace? behaviorSpace = null);
}
