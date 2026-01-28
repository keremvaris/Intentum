using Intentum.Core.Intents;
using Intentum.Runtime.Policy;

namespace Intentum.Runtime.Engine;

/// <summary>
/// Deterministic policy evaluation for inferred intent.
/// </summary>
public sealed class IntentPolicyEngine
{
    public PolicyDecision Evaluate(
        Intent intent,
        IntentPolicy policy)
    {
        foreach (var rule in policy.Rules)
        {
            if (rule.Condition(intent))
                return rule.Decision;
        }

        return PolicyDecision.Observe;
    }
}
