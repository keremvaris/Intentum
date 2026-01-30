using Intentum.Core.Intents;
using Intentum.Runtime.Engine;

namespace Intentum.Runtime.Policy;

/// <summary>
/// A/B policy variants: multiple named policies with a selector to choose which policy applies for a given intent.
/// </summary>
public sealed class PolicyVariantSet
{
    private readonly IReadOnlyDictionary<string, IntentPolicy> _variants;
    private readonly Func<Intent, string> _selector;

    /// <summary>
    /// Creates a variant set with named policies and a selector that returns the variant name for a given intent.
    /// </summary>
    /// <param name="variants">Named policies (e.g. "control", "treatment").</param>
    /// <param name="selector">Function that returns which variant name to use for the given intent (e.g. random, by confidence, by experiment key).</param>
    public PolicyVariantSet(
        IReadOnlyDictionary<string, IntentPolicy> variants,
        Func<Intent, string> selector)
    {
        _variants = variants ?? throw new ArgumentNullException(nameof(variants));
        _selector = selector ?? throw new ArgumentNullException(nameof(selector));
    }

    /// <summary>
    /// Evaluates the intent with the policy selected by the selector. If the selected variant name is unknown, returns Observe.
    /// </summary>
    public PolicyDecision Decide(Intent intent)
    {
        var name = _selector(intent);
        if (string.IsNullOrEmpty(name) || !_variants.TryGetValue(name, out var policy))
            return PolicyDecision.Observe;
        return IntentPolicyEngine.Evaluate(intent, policy);
    }

    /// <summary>
    /// Returns the variant names in this set.
    /// </summary>
    public IReadOnlyCollection<string> GetVariantNames() => _variants.Keys.ToList();
}
