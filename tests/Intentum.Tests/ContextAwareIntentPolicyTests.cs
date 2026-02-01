using Intentum.Runtime.Policy;

namespace Intentum.Tests;

public sealed class ContextAwareIntentPolicyTests
{
    [Fact]
    public void Constructor_WithNoRules_ExposesEmptyRules()
    {
        var policy = new ContextAwareIntentPolicy();
        Assert.Empty(policy.Rules);
    }

    [Fact]
    public void Constructor_WithRules_ExposesRules()
    {
        var rule = new ContextAwarePolicyRule("R", (_, _) => true, PolicyDecision.Allow);
        var policy = new ContextAwareIntentPolicy([rule]);
        Assert.Single(policy.Rules);
        Assert.Equal("R", policy.Rules.Single().Name);
    }

    [Fact]
    public void AddRule_AddsRuleAndReturnsThis()
    {
        var policy = new ContextAwareIntentPolicy();
        var rule = new ContextAwarePolicyRule("A", (_, _) => false, PolicyDecision.Block);

        var chained = policy.AddRule(rule);

        Assert.Same(policy, chained);
        Assert.Single(policy.Rules);
        Assert.Equal("A", policy.Rules.Single().Name);
    }
}
