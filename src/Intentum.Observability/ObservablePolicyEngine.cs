using Intentum.Core.Intents;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

namespace Intentum.Observability;

/// <summary>
/// Extension methods for adding observability to policy decisions.
/// </summary>
public static class ObservablePolicyEngine
{
    /// <summary>
    /// Decides on an intent with policy and records metrics.
    /// </summary>
    public static PolicyDecision DecideWithMetrics(
        this Intent intent,
        IntentPolicy policy)
    {
        var decision = intent.Decide(policy);
        IntentumMetrics.RecordPolicyDecision(decision);
        return decision;
    }
}
