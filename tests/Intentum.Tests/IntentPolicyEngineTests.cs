using Intentum.Core.Intents;
using Intentum.Runtime.Engine;
using Intentum.Runtime.Policy;

namespace Intentum.Tests;

public sealed class IntentPolicyEngineTests
{
    [Fact]
    public void Evaluate_WhenNoRuleMatches_ReturnsObserve()
    {
        var intent = new Intent("Test", [], new IntentConfidence(0.5, "Medium"));
        var policy = new IntentPolicyBuilder()
            .Allow("AllowHigh", i => i.Confidence.Level == "High")
            .Block("BlockLow", i => i.Confidence.Level == "Low")
            .Build();

        var decision = IntentPolicyEngine.Evaluate(intent, policy);

        Assert.Equal(PolicyDecision.Observe, decision);
    }

    [Fact]
    public void Evaluate_WhenPolicyHasNoRules_ReturnsObserve()
    {
        var intent = new Intent("Test", [], new IntentConfidence(0.9, "High"));
        var policy = new IntentPolicy();

        var decision = IntentPolicyEngine.Evaluate(intent, policy);

        Assert.Equal(PolicyDecision.Observe, decision);
    }

    [Fact]
    public void Evaluate_WhenRuleMatches_ReturnsRuleDecision()
    {
        var intent = new Intent("Test", [], new IntentConfidence(0.2, "Low"));
        var policy = new IntentPolicyBuilder()
            .Warn("WarnLow", i => i.Confidence.Level == "Low")
            .Build();

        var decision = IntentPolicyEngine.Evaluate(intent, policy);

        Assert.Equal(PolicyDecision.Warn, decision);
    }
}
