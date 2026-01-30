using Intentum.Core.Intents;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

namespace Intentum.Tests;

public sealed class PolicyCompositionTests
{
    [Fact]
    public void WithBase_BaseRulesEvaluatedFirst_FirstMatchWins()
    {
        var basePolicy = new IntentPolicyBuilder()
            .Block("BaseBlock", i => i.Confidence.Level == "Low")
            .Allow("BaseAllow", i => i.Confidence.Level == "High")
            .Build();

        var derived = new IntentPolicyBuilder()
            .Allow("DerivedAllowLow", i => i.Confidence.Level == "Low")
            .Build();

        var composed = derived.WithBase(basePolicy);

        var lowIntent = new Intent("L", [], new IntentConfidence(0.2, "Low"));
        var highIntent = new Intent("H", [], new IntentConfidence(0.9, "High"));

        Assert.Equal(PolicyDecision.Block, lowIntent.Decide(composed));
        Assert.Equal(PolicyDecision.Allow, highIntent.Decide(composed));
    }

    [Fact]
    public void WithBase_DerivedRulesOverrideWhenBaseDoesNotMatch()
    {
        var basePolicy = new IntentPolicyBuilder()
            .Block("BaseBlock", i => i.Confidence.Level == "Low")
            .Build();

        var derived = new IntentPolicyBuilder()
            .Escalate("DerivedEscalate", i => i.Confidence.Level == "Medium")
            .Build();

        var composed = derived.WithBase(basePolicy);

        var mediumIntent = new Intent("M", [], new IntentConfidence(0.5, "Medium"));
        Assert.Equal(PolicyDecision.Escalate, mediumIntent.Decide(composed));
    }

    [Fact]
    public void Merge_CombinesRulesInOrder_FirstMatchWins()
    {
        var policyA = new IntentPolicyBuilder()
            .Block("A", i => i.Confidence.Level == "Low")
            .Build();
        var policyB = new IntentPolicyBuilder()
            .Allow("B", i => i.Confidence.Level == "Low")
            .Build();

        var merged = IntentPolicy.Merge(policyA, policyB);

        var lowIntent = new Intent("L", [], new IntentConfidence(0.2, "Low"));
        Assert.Equal(PolicyDecision.Block, lowIntent.Decide(merged));
    }

    [Fact]
    public void Merge_EmptyPolicies_ReturnsObserve()
    {
        var merged = IntentPolicy.Merge();
        var intent = new Intent("X", [], new IntentConfidence(0.5, "Medium"));
        Assert.Equal(PolicyDecision.Observe, intent.Decide(merged));
    }

    [Fact]
    public void Merge_SinglePolicy_SameAsOriginal()
    {
        var policy = new IntentPolicyBuilder()
            .Warn("W", i => i.Confidence.Level == "Medium")
            .Build();
        var merged = IntentPolicy.Merge(policy);
        var intent = new Intent("M", [], new IntentConfidence(0.5, "Medium"));
        Assert.Equal(PolicyDecision.Warn, intent.Decide(merged));
    }

    [Fact]
    public void PolicyVariantSet_SelectorPicksPolicy_DecisionFromSelectedPolicy()
    {
        var control = new IntentPolicyBuilder()
            .Allow("AllowHigh", i => i.Confidence.Level == "High")
            .Allow("AllowLow", i => i.Confidence.Level == "Low")
            .Build();
        var treatment = new IntentPolicyBuilder()
            .Block("BlockHigh", i => i.Confidence.Level == "High")
            .Build();

        var variants = new PolicyVariantSet(
            new Dictionary<string, IntentPolicy> { ["control"] = control, ["treatment"] = treatment },
            intent => intent.Confidence.Score > 0.8 ? "treatment" : "control");

        var highIntent = new Intent("H", [], new IntentConfidence(0.9, "High"));
        var lowIntent = new Intent("L", [], new IntentConfidence(0.3, "Low"));

        Assert.Equal(PolicyDecision.Block, highIntent.Decide(variants));
        Assert.Equal(PolicyDecision.Allow, lowIntent.Decide(variants));
    }

    [Fact]
    public void PolicyVariantSet_UnknownVariantName_ReturnsObserve()
    {
        var policy = new IntentPolicyBuilder()
            .Block("B", _ => true)
            .Build();
        var variants = new PolicyVariantSet(
            new Dictionary<string, IntentPolicy> { ["known"] = policy },
            _ => "unknown");

        var intent = new Intent("X", [], new IntentConfidence(0.5, "Medium"));
        Assert.Equal(PolicyDecision.Observe, intent.Decide(variants));
    }

    [Fact]
    public void PolicyVariantSet_GetVariantNames_ReturnsAllNames()
    {
        var p = new IntentPolicy();
        var variants = new PolicyVariantSet(
            new Dictionary<string, IntentPolicy> { ["a"] = p, ["b"] = p },
            _ => "a");
        Assert.Equal(2, variants.GetVariantNames().Count);
        Assert.Contains("a", variants.GetVariantNames());
        Assert.Contains("b", variants.GetVariantNames());
    }
}
