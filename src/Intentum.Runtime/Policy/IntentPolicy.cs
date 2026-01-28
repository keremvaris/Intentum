namespace Intentum.Runtime.Policy;

/// <summary>
/// Ruleset for evaluating intent decisions.
/// </summary>
public sealed class IntentPolicy
{
    private readonly List<PolicyRule> _rules = [];

    public IReadOnlyCollection<PolicyRule> Rules => _rules;

    public IntentPolicy AddRule(PolicyRule rule)
    {
        _rules.Add(rule);
        return this;
    }
}
