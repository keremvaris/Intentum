using Intentum.Core.Intents;
using Intentum.Runtime.Engine;
using Intentum.Runtime.Policy;

namespace Intentum.Tests;

public sealed class ContextAwarePolicyEngineTests
{
    [Fact]
    public void Evaluate_WhenNoRuleMatches_ReturnsObserve()
    {
        var intent = new Intent("Test", [], new IntentConfidence(0.5, "Medium"));
        var context = new PolicyContext(intent);
        var policy = new ContextAwareIntentPolicy();

        var decision = ContextAwarePolicyEngine.Evaluate(intent, context, policy);

        Assert.Equal(PolicyDecision.Observe, decision);
    }

    [Fact]
    public void Evaluate_WhenRuleMatches_ReturnsRuleDecision()
    {
        var intent = new Intent("High", [], new IntentConfidence(0.9, "High"));
        var context = new PolicyContext(intent);
        var policy = new ContextAwareIntentPolicy();
        policy.AddRule(new ContextAwarePolicyRule("AllowHigh", (i, _) => i.Confidence.Level == "High", PolicyDecision.Allow));

        var decision = ContextAwarePolicyEngine.Evaluate(intent, context, policy);

        Assert.Equal(PolicyDecision.Allow, decision);
    }

    [Fact]
    public void Evaluate_WhenContextUsedInCondition_MatchesOnContext()
    {
        var intent = new Intent("X", [], new IntentConfidence(0.5, "Medium"));
        var context = new PolicyContext(intent, SystemLoad: 0.95);
        var policy = new ContextAwareIntentPolicy();
        policy.AddRule(new ContextAwarePolicyRule("EscalateHighLoad", (_, c) => c.SystemLoad is > 0.9, PolicyDecision.Escalate));

        var decision = ContextAwarePolicyEngine.Evaluate(intent, context, policy);

        Assert.Equal(PolicyDecision.Escalate, decision);
    }

    [Fact]
    public void EvaluateWithRule_WhenRuleMatches_ReturnsDecisionAndMatchedRule()
    {
        var intent = new Intent("Low", [], new IntentConfidence(0.2, "Low"));
        var context = new PolicyContext(intent);
        var rule = new ContextAwarePolicyRule("BlockLow", (i, _) => i.Confidence.Level == "Low", PolicyDecision.Block);
        var policy = new ContextAwareIntentPolicy();
        policy.AddRule(rule);

        var (decision, matchedRule) = ContextAwarePolicyEngine.EvaluateWithRule(intent, context, policy);

        Assert.Equal(PolicyDecision.Block, decision);
        Assert.NotNull(matchedRule);
        Assert.Equal("BlockLow", matchedRule.Name);
    }

    [Fact]
    public void EvaluateWithRule_WhenNoRuleMatches_ReturnsObserveAndNullRule()
    {
        var intent = new Intent("Y", [], new IntentConfidence(0.5, "Medium"));
        var context = new PolicyContext(intent);
        var policy = new ContextAwareIntentPolicy();

        var (decision, matchedRule) = ContextAwarePolicyEngine.EvaluateWithRule(intent, context, policy);

        Assert.Equal(PolicyDecision.Observe, decision);
        Assert.Null(matchedRule);
    }
}
