using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core.Behavior;
using Intentum.Runtime;
using Intentum.Runtime.Localization;
using Intentum.Runtime.Policy;

namespace Intentum.Tests;

/// <summary>
/// Tests for new Policy Decision Types: Escalate, RequireAuth, RateLimit.
/// </summary>
public class PolicyDecisionTypesTests
{
    [Fact]
    public void PolicyDecision_NewTypes_Exist()
    {
        // Assert
        Assert.Contains(PolicyDecision.Escalate, Enum.GetValues<PolicyDecision>());
        Assert.Contains(PolicyDecision.RequireAuth, Enum.GetValues<PolicyDecision>());
        Assert.Contains(PolicyDecision.RateLimit, Enum.GetValues<PolicyDecision>());
    }

    [Fact]
    public void IntentPolicyBuilder_Escalate_AddsEscalateRule()
    {
        // Arrange & Act
        var policy = new IntentPolicyBuilder()
            .Escalate("HighRiskEscalate", i => i.Confidence.Level == "Low")
            .Build();

        // Assert
        Assert.Single(policy.Rules);
        Assert.Equal(PolicyDecision.Escalate, policy.Rules.First().Decision);
    }

    [Fact]
    public void IntentPolicyBuilder_RequireAuth_AddsRequireAuthRule()
    {
        // Arrange & Act
        var policy = new IntentPolicyBuilder()
            .RequireAuth("SensitiveAction", i => i.Signals.Any(s => s.Description.Contains("sensitive")))
            .Build();

        // Assert
        Assert.Single(policy.Rules);
        Assert.Equal(PolicyDecision.RequireAuth, policy.Rules.First().Decision);
    }

    [Fact]
    public void IntentPolicyBuilder_RateLimit_AddsRateLimitRule()
    {
        // Arrange & Act
        var policy = new IntentPolicyBuilder()
            .RateLimit("HighFrequency", i => i.Signals.Count > 10)
            .Build();

        // Assert
        Assert.Single(policy.Rules);
        Assert.Equal(PolicyDecision.RateLimit, policy.Rules.First().Decision);
    }

    [Fact]
    public void PolicyDecision_ToLocalizedString_Allow_Observe_Warn_Block_ReturnsLocalized()
    {
        var localizer = new DefaultLocalizer();
        Assert.Equal("Allow", PolicyDecision.Allow.ToLocalizedString(localizer));
        Assert.Equal("Observe", PolicyDecision.Observe.ToLocalizedString(localizer));
        Assert.Equal("Warn", PolicyDecision.Warn.ToLocalizedString(localizer));
        Assert.Equal("Block", PolicyDecision.Block.ToLocalizedString(localizer));
    }

    [Fact]
    public void PolicyDecision_ToLocalizedString_Escalate_ReturnsLocalized()
    {
        // Arrange
        var localizer = new DefaultLocalizer();

        // Act
        var result = PolicyDecision.Escalate.ToLocalizedString(localizer);

        // Assert
        Assert.Equal("Escalate", result);
    }

    [Fact]
    public void PolicyDecision_ToLocalizedString_RequireAuth_ReturnsLocalized()
    {
        // Arrange
        var localizer = new DefaultLocalizer();

        // Act
        var result = PolicyDecision.RequireAuth.ToLocalizedString(localizer);

        // Assert
        Assert.Equal("Require Authentication", result);
    }

    [Fact]
    public void PolicyDecision_ToLocalizedString_RateLimit_ReturnsLocalized()
    {
        // Arrange
        var localizer = new DefaultLocalizer();

        // Act
        var result = PolicyDecision.RateLimit.ToLocalizedString(localizer);

        // Assert
        Assert.Equal("Rate Limit", result);
    }

    [Fact]
    public void PolicyDecision_ToLocalizedString_Turkish_ReturnsTurkish()
    {
        // Arrange
        var localizer = new DefaultLocalizer("tr");

        // Act & Assert
        Assert.Equal("Yükselt", PolicyDecision.Escalate.ToLocalizedString(localizer));
        Assert.Equal("Kimlik Doğrulama Gerekli", PolicyDecision.RequireAuth.ToLocalizedString(localizer));
        Assert.Equal("Hız Sınırı", PolicyDecision.RateLimit.ToLocalizedString(localizer));
    }

    [Fact]
    public void IntentPolicyBuilder_WithNewDecisionTypes_DecidesCorrectly()
    {
        // Arrange
        var model = new LlmIntentModel(
            new MockEmbeddingProvider(),
            new SimpleAverageSimilarityEngine());

        var space = new BehaviorSpaceBuilder()
            .WithActor("user")
                .Action("login")
            .Build();

        var policy = new IntentPolicyBuilder()
            .Escalate("LowConfidenceEscalate", i => i.Confidence.Level == "Low")
            .RequireAuth("MediumConfidenceAuth", i => i.Confidence.Level == "Medium")
            .RateLimit("HighConfidenceRateLimit", i => i.Confidence.Level == "High")
            .Allow("CertainAllow", i => i.Confidence.Level == "Certain")
            .Build();

        // Act
        var intent = model.Infer(space);
        var decision = intent.Decide(policy);

        // Assert
        Assert.Contains(decision, Enum.GetValues<PolicyDecision>());
    }
}
