using Intentum.Core.Behavior;
using Intentum.Core.Intents;
using Intentum.Explainability;
using Intentum.Runtime.Policy;

namespace Intentum.Tests;

public sealed class IntentTreeExplainerTests
{
    [Fact]
    public void GetIntentTree_ReturnsDecisionAndIntentSummary()
    {
        var intent = new Intent(
            "Login",
            new List<IntentSignal> { new("auth", "login attempt", 0.9) },
            new IntentConfidence(0.85, "High"));
        var policy = new IntentPolicyBuilder().Allow("AllowHigh", i => i.Confidence.Level == "High").Build();
        var explainer = new IntentTreeExplainer();

        var tree = explainer.GetIntentTree(intent, policy);

        Assert.Equal(PolicyDecision.Allow, tree.Decision);
        Assert.Equal("AllowHigh", tree.MatchedRuleName);
        Assert.Equal("Login", tree.Intent.Name);
        Assert.Equal("High", tree.Intent.ConfidenceLevel);
        Assert.Equal(0.85, tree.Intent.ConfidenceScore);
        Assert.Single(tree.Signals);
        Assert.Equal("auth", tree.Signals[0].Source);
    }

    [Fact]
    public void GetIntentTree_WithBehaviorSpace_IncludesBehaviorSummary()
    {
        var intent = new Intent("X", [], new IntentConfidence(0.5, "Medium"));
        var policy = new IntentPolicyBuilder().Build();
        var space = new BehaviorSpace();
        var t = DateTimeOffset.UtcNow;
        space.Observe(new BehaviorEvent("user", "login", t));
        space.Observe(new BehaviorEvent("user", "submit", t.AddSeconds(1)));
        var explainer = new IntentTreeExplainer();

        var tree = explainer.GetIntentTree(intent, policy, space);

        Assert.NotNull(tree.BehaviorSummary);
        Assert.Contains("user:login", tree.BehaviorSummary);
        Assert.Contains("user:submit", tree.BehaviorSummary);
    }

    [Fact]
    public void GetIntentTree_WithNullBehaviorSpace_BehaviorSummaryNull()
    {
        var intent = new Intent("Y", [], new IntentConfidence(0.5, "Medium"));
        var policy = new IntentPolicyBuilder().Build();
        var explainer = new IntentTreeExplainer();

        var tree = explainer.GetIntentTree(intent, policy);

        Assert.Null(tree.BehaviorSummary);
    }

    [Fact]
    public void GetIntentTree_WhenNoRuleMatches_MatchedRuleNameNull()
    {
        var intent = new Intent("Z", [], new IntentConfidence(0.5, "Medium"));
        var policy = new IntentPolicyBuilder().Block("OnlyLow", i => i.Confidence.Level == "Low").Build();
        var explainer = new IntentTreeExplainer();

        var tree = explainer.GetIntentTree(intent, policy);

        Assert.Equal(PolicyDecision.Observe, tree.Decision);
        Assert.Null(tree.MatchedRuleName);
    }
}
