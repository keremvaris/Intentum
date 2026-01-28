using Intentum.Core.Intents;

namespace Intentum.Runtime.Policy;

/// <summary>
/// Fluent builder for creating IntentPolicy instances with a more readable API.
/// </summary>
public sealed class IntentPolicyBuilder
{
    private readonly IntentPolicy _policy = new();

    /// <summary>
    /// Adds a rule that allows when the condition is met.
    /// </summary>
    public IntentPolicyBuilder Allow(string name, Func<Intent, bool> condition)
    {
        _policy.AddRule(new PolicyRule(name, condition, PolicyDecision.Allow));
        return this;
    }

    /// <summary>
    /// Adds a rule that observes when the condition is met.
    /// </summary>
    public IntentPolicyBuilder Observe(string name, Func<Intent, bool> condition)
    {
        _policy.AddRule(new PolicyRule(name, condition, PolicyDecision.Observe));
        return this;
    }

    /// <summary>
    /// Adds a rule that warns when the condition is met.
    /// </summary>
    public IntentPolicyBuilder Warn(string name, Func<Intent, bool> condition)
    {
        _policy.AddRule(new PolicyRule(name, condition, PolicyDecision.Warn));
        return this;
    }

    /// <summary>
    /// Adds a rule that blocks when the condition is met.
    /// </summary>
    public IntentPolicyBuilder Block(string name, Func<Intent, bool> condition)
    {
        _policy.AddRule(new PolicyRule(name, condition, PolicyDecision.Block));
        return this;
    }

    /// <summary>
    /// Adds a rule that escalates when the condition is met.
    /// </summary>
    public IntentPolicyBuilder Escalate(string name, Func<Intent, bool> condition)
    {
        _policy.AddRule(new PolicyRule(name, condition, PolicyDecision.Escalate));
        return this;
    }

    /// <summary>
    /// Adds a rule that requires authentication when the condition is met.
    /// </summary>
    public IntentPolicyBuilder RequireAuth(string name, Func<Intent, bool> condition)
    {
        _policy.AddRule(new PolicyRule(name, condition, PolicyDecision.RequireAuth));
        return this;
    }

    /// <summary>
    /// Adds a rule that applies rate limiting when the condition is met.
    /// </summary>
    public IntentPolicyBuilder RateLimit(string name, Func<Intent, bool> condition)
    {
        _policy.AddRule(new PolicyRule(name, condition, PolicyDecision.RateLimit));
        return this;
    }

    /// <summary>
    /// Adds a custom rule with a specific decision.
    /// </summary>
    public IntentPolicyBuilder Rule(string name, Func<Intent, bool> condition, PolicyDecision decision)
    {
        _policy.AddRule(new PolicyRule(name, condition, decision));
        return this;
    }

    /// <summary>
    /// Builds and returns the IntentPolicy instance.
    /// </summary>
    public IntentPolicy Build()
    {
        return _policy;
    }
}
