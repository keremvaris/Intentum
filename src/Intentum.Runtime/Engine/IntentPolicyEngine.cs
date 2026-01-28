using System.Linq;
using Intentum.Core.Intents;
using Intentum.Runtime.Policy;

namespace Intentum.Runtime.Engine;

/// <summary>
/// Deterministic policy evaluation for inferred intent.
/// </summary>
public static class IntentPolicyEngine
{
    public static PolicyDecision Evaluate(
        Intent intent,
        IntentPolicy policy)
    {
        var matchingRule = policy.Rules.Where(rule => rule.Condition(intent)).FirstOrDefault();
        return matchingRule?.Decision ?? PolicyDecision.Observe;
    }
}
