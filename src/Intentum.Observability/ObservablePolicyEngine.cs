using System.Diagnostics;
using Intentum.Core.Intents;
using Intentum.Runtime.Engine;
using Intentum.Runtime.Policy;

namespace Intentum.Observability;

/// <summary>
/// Extension methods for adding observability to policy decisions (metrics and OpenTelemetry spans).
/// </summary>
public static class ObservablePolicyEngine
{
    /// <summary>
    /// Decides on an intent with policy, records metrics, and creates a policy.evaluate span.
    /// </summary>
    public static PolicyDecision DecideWithMetrics(
        this Intent intent,
        IntentPolicy policy)
    {
        using var activity = IntentumActivitySource.Source.StartActivity(IntentumActivitySource.PolicyEvaluateSpanName);

        var (decision, matchedRule) = IntentPolicyEngine.EvaluateWithRule(intent, policy);

        if (activity != null)
        {
            activity.SetTag("intentum.policy.decision", decision.ToString());
            if (matchedRule != null)
                activity.SetTag("intentum.policy.matched_rule", matchedRule.Name);
        }

        IntentumMetrics.RecordPolicyDecision(decision);
        return decision;
    }
}
