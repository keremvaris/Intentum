using Intentum.Core.Intents;
using Intentum.Runtime.Policy;

namespace Intentum.Runtime.Engine;

/// <summary>
/// Deterministic policy evaluation for inferred intent.
/// </summary>
public static class IntentPolicyEngine
{
    /// <summary>
    /// Evaluates the intent against the policy and returns the decision.
    /// </summary>
    public static PolicyDecision Evaluate(
        Intent intent,
        IntentPolicy policy)
    {
        var (decision, _) = EvaluateWithRule(intent, policy);
        return decision;
    }

    /// <summary>
    /// Evaluates the intent against the policy and returns the decision plus the matched rule (if any).
    /// </summary>
    public static (PolicyDecision Decision, PolicyRule? MatchedRule) EvaluateWithRule(
        Intent intent,
        IntentPolicy policy)
    {
        var matchingRule = policy.Rules.FirstOrDefault(rule => rule.Condition(intent));
        var decision = matchingRule?.Decision ?? PolicyDecision.Observe;
        return (decision, matchingRule);
    }
}
