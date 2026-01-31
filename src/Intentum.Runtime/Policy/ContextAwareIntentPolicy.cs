namespace Intentum.Runtime.Policy;

/// <summary>
/// Policy that evaluates rules with both intent and context (system load, region, etc.).
/// </summary>
public sealed class ContextAwareIntentPolicy
{
    private readonly List<ContextAwarePolicyRule> _rules = [];

    public IReadOnlyCollection<ContextAwarePolicyRule> Rules => _rules;

    public ContextAwareIntentPolicy() { }

    public ContextAwareIntentPolicy(IEnumerable<ContextAwarePolicyRule> rules)
    {
        _rules.AddRange(rules);
    }

    public ContextAwareIntentPolicy AddRule(ContextAwarePolicyRule rule)
    {
        _rules.Add(rule);
        return this;
    }
}
