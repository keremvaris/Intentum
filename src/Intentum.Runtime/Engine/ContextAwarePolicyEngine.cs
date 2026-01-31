using Intentum.Core.Intents;
using Intentum.Runtime.Policy;

namespace Intentum.Runtime.Engine;

/// <summary>
/// Context-aware policy evaluation: decides using both intent and context (system load, region, etc.).
/// </summary>
public static class ContextAwarePolicyEngine
{
    /// <summary>
    /// Evaluates the intent with the given context and context-aware policy.
    /// First matching rule wins; default is Observe.
    /// </summary>
    public static PolicyDecision Evaluate(
        Intent intent,
        PolicyContext context,
        ContextAwareIntentPolicy policy)
    {
        var matchingRule = policy.Rules.FirstOrDefault(rule => rule.Condition(intent, context));
        return matchingRule?.Decision ?? PolicyDecision.Observe;
    }

    /// <summary>
    /// Evaluates and returns the decision plus the matched rule (if any).
    /// </summary>
    public static (PolicyDecision Decision, ContextAwarePolicyRule? MatchedRule) EvaluateWithRule(
        Intent intent,
        PolicyContext context,
        ContextAwareIntentPolicy policy)
    {
        var matchingRule = policy.Rules.FirstOrDefault(rule => rule.Condition(intent, context));
        var decision = matchingRule?.Decision ?? PolicyDecision.Observe;
        return (decision, matchingRule);
    }
}
