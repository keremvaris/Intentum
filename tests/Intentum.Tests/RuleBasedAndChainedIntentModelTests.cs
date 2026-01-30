using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Core.Models;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

namespace Intentum.Tests;

/// <summary>
/// Tests for RuleBasedIntentModel and ChainedIntentModel.
/// </summary>
public class RuleBasedAndChainedIntentModelTests
{
    [Fact]
    public void RuleBasedIntentModel_WhenRuleMatches_ReturnsIntentWithReasoning()
    {
        var rules = new List<Func<BehaviorSpace, RuleMatch?>>
        {
            space =>
            {
                var loginFails = space.Events.Count(e => e.Action == "login.failed");
                if (loginFails >= 2)
                    return new RuleMatch("SuspiciousAccess", 0.8, "login.failed>=2");
                return null;
            }
        };

        var model = new RuleBasedIntentModel(rules);
        var space = new BehaviorSpace()
            .Observe("user", "login.failed")
            .Observe("user", "login.failed")
            .Observe("user", "ip.changed");

        var intent = model.Infer(space);

        Assert.Equal("SuspiciousAccess", intent.Name);
        Assert.Equal(0.8, intent.Confidence.Score);
        Assert.Equal("login.failed>=2", intent.Reasoning);
    }

    [Fact]
    public void RuleBasedIntentModel_WhenNoRuleMatches_ReturnsUnknownWithReasoning()
    {
        var rules = new List<Func<BehaviorSpace, RuleMatch?>>
        {
            space => space.Events.Count(e => e.Action == "login.failed") >= 5
                ? new RuleMatch("HighRisk", 0.9, "login.failed>=5")
                : null
        };

        var model = new RuleBasedIntentModel(rules);
        var space = new BehaviorSpace()
            .Observe("user", "login.success");

        var intent = model.Infer(space);

        Assert.Equal("Unknown", intent.Name);
        Assert.Equal("No rule matched", intent.Reasoning);
    }

    [Fact]
    public void ChainedIntentModel_WhenPrimaryConfidenceAboveThreshold_ReturnsPrimaryWithReasoning()
    {
        var rules = new List<Func<BehaviorSpace, RuleMatch?>>
        {
            space =>
            {
                if (space.Events.Any(e => e.Action == "password.reset"))
                    return new RuleMatch("AccountRecovery", 0.85, "password.reset");
                return null;
            }
        };

        var primary = new RuleBasedIntentModel(rules);
        var fallback = new LlmIntentModel(new MockEmbeddingProvider(), new SimpleAverageSimilarityEngine());
        var chained = new ChainedIntentModel(primary, fallback);

        var space = new BehaviorSpace()
            .Observe("user", "login.failed")
            .Observe("user", "password.reset")
            .Observe("user", "login.success");

        var intent = chained.Infer(space);

        Assert.Equal("AccountRecovery", intent.Name);
        Assert.True(intent.Reasoning?.StartsWith("Primary:", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    [Fact]
    public void ChainedIntentModel_WhenPrimaryConfidenceBelowThreshold_ReturnsFallbackWithReasoning()
    {
        var rules = new List<Func<BehaviorSpace, RuleMatch?>>
        {
            _ => new RuleMatch("LowConfidence", 0.5, "always match but low")
        };

        var primary = new RuleBasedIntentModel(rules);
        var fallback = new LlmIntentModel(new MockEmbeddingProvider(), new SimpleAverageSimilarityEngine());
        var chained = new ChainedIntentModel(primary, fallback);

        var space = new BehaviorSpace().Observe("user", "login");

        var intent = chained.Infer(space);

        Assert.True(intent.Reasoning?.Contains("Fallback", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    [Fact]
    public void ChainedIntentModel_WithPolicy_DecidesCorrectly()
    {
        var rules = new List<Func<BehaviorSpace, RuleMatch?>>
        {
            space => space.Events.Count(e => e.Action == "login.failed") >= 2
                ? new RuleMatch("Suspicious", 0.9, "login.failed>=2")
                : null
        };

        var primary = new RuleBasedIntentModel(rules);
        var fallback = new LlmIntentModel(new MockEmbeddingProvider(), new SimpleAverageSimilarityEngine());
        var chained = new ChainedIntentModel(primary, fallback);

        var policy = new IntentPolicyBuilder()
            .Block("Suspicious", i => i.Name == "Suspicious")
            .Allow("Default", _ => true)
            .Build();

        var space = new BehaviorSpace()
            .Observe("user", "login.failed")
            .Observe("user", "login.failed");

        var intent = chained.Infer(space);
        var decision = intent.Decide(policy);

        Assert.Equal("Suspicious", intent.Name);
        Assert.Equal(PolicyDecision.Block, decision);
    }
}
