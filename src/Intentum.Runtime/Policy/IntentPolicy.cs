namespace Intentum.Runtime.Policy;

/// <summary>
/// Ruleset for evaluating intent decisions.
/// </summary>
public sealed class IntentPolicy
{
    private readonly List<PolicyRule> _rules = [];

    public IReadOnlyCollection<PolicyRule> Rules => _rules;

    /// <summary>Creates an empty policy.</summary>
    public IntentPolicy() { }

    /// <summary>Creates a policy with the given rules (e.g. for composition).</summary>
    public IntentPolicy(IEnumerable<PolicyRule> rules)
    {
        if (rules != null)
            _rules.AddRange(rules);
    }

    public IntentPolicy AddRule(PolicyRule rule)
    {
        _rules.Add(rule);
        return this;
    }

    /// <summary>
    /// Returns a new policy that evaluates this policy's rules after the base policy's rules (base first, then this). First matching rule wins.
    /// </summary>
    public IntentPolicy WithBase(IntentPolicy basePolicy)
    {
        if (basePolicy == null)
            throw new ArgumentNullException(nameof(basePolicy));
        return new IntentPolicy(basePolicy.Rules.Concat(_rules));
    }

    /// <summary>
    /// Returns a new policy that merges all given policies: rules from the first policy first, then the second, etc. First matching rule wins.
    /// </summary>
    public static IntentPolicy Merge(params IntentPolicy[] policies)
    {
        if (policies == null || policies.Length == 0)
            return new IntentPolicy();
        return new IntentPolicy(policies.SelectMany(p => p?.Rules ?? []));
    }
}
