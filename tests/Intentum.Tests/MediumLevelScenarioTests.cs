using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

namespace Intentum.Tests;

/// <summary>
/// Medium-complexity scenario tests: ESG reporting with retries, carbon verification,
/// sukuk issuance, compliance checks. Showcase for policy order and signal-based rules.
/// </summary>
public class MediumLevelScenarioTests
{
    private static LlmIntentModel CreateModel()
    {
        return new LlmIntentModel(
            new MockEmbeddingProvider(),
            new SimpleAverageSimilarityEngine());
    }

    [Fact]
    public void ESGReportSubmissionWithRetries_AllowsOrObserves_NotBlock()
    {
        var space = new BehaviorSpace()
            .Observe("analyst", "prepare_esg_report")
            .Observe("analyst", "retry_validation")
            .Observe("analyst", "retry_validation")
            .Observe("system", "report_submitted");
        var model = CreateModel();
        var policy = new IntentPolicy()
            .AddRule(new PolicyRule(
                "ExcessiveRetryBlock",
                i => i.Signals.Count(s => s.Description.Contains("retry", StringComparison.OrdinalIgnoreCase)) >= 3,
                PolicyDecision.Block))
            .AddRule(new PolicyRule(
                "HighConfidenceAllow",
                i => i.Confidence.Level is "High" or "Certain",
                PolicyDecision.Allow))
            .AddRule(new PolicyRule(
                "MediumConfidenceObserve",
                i => i.Confidence.Level == "Medium",
                PolicyDecision.Observe));

        var intent = model.Infer(space);
        var decision = intent.Decide(policy);

        Assert.NotEqual(PolicyDecision.Block, decision);
        Assert.Equal(4, space.Events.Count);
    }

    [Fact]
    public void ESGReportWithExcessiveRetries_ThreeRetries_BlockOrObserve()
    {
        var space = new BehaviorSpace()
            .Observe("analyst", "prepare_esg_report")
            .Observe("analyst", "retry_validation")
            .Observe("analyst", "retry_validation")
            .Observe("analyst", "retry_validation");
        var model = CreateModel();
        var policy = new IntentPolicy()
            .AddRule(new PolicyRule(
                "ExcessiveRetryBlock",
                i => i.Signals.Count(s => s.Description.Contains("retry", StringComparison.OrdinalIgnoreCase)) >= 3,
                PolicyDecision.Block))
            .AddRule(new PolicyRule(
                "HighConfidenceAllow",
                i => i.Confidence.Level is "High" or "Certain",
                PolicyDecision.Allow));

        var intent = model.Infer(space);
        var decision = intent.Decide(policy);

        Assert.Contains(decision, new[] { PolicyDecision.Block, PolicyDecision.Allow, PolicyDecision.Observe, PolicyDecision.Warn });
    }

    [Fact]
    public void CarbonVerificationProcess_ProducesValidDecision()
    {
        var space = new BehaviorSpace()
            .Observe("verifier", "verify_carbon_data")
            .Observe("verifier", "request_correction")
            .Observe("analyst", "submit_correction")
            .Observe("verifier", "approve");
        var model = CreateModel();
        var policy = new IntentPolicy()
            .AddRule(new PolicyRule(
                "ExcessiveRetryBlock",
                i => i.Signals.Count(s => s.Description.Contains("retry", StringComparison.OrdinalIgnoreCase)) >= 3,
                PolicyDecision.Block))
            .AddRule(new PolicyRule(
                "HighConfidenceAllow",
                i => i.Confidence.Level is "High" or "Certain",
                PolicyDecision.Allow))
            .AddRule(new PolicyRule(
                "MediumConfidenceObserve",
                i => i.Confidence.Level == "Medium",
                PolicyDecision.Observe))
            .AddRule(new PolicyRule(
                "LowConfidenceWarn",
                i => i.Confidence.Level == "Low",
                PolicyDecision.Warn));

        var intent = model.Infer(space);
        var decision = intent.Decide(policy);

        Assert.Contains(decision, new[] { PolicyDecision.Allow, PolicyDecision.Observe, PolicyDecision.Warn, PolicyDecision.Block });
        Assert.Equal(4, space.Events.Count);
    }

    [Fact]
    public void SukukIssuanceWithApprovals_ProducesValidDecision()
    {
        var space = new BehaviorSpace()
            .Observe("issuer", "initiate_sukuk")
            .Observe("sharia", "review")
            .Observe("regulator", "approve")
            .Observe("system", "issue_sukuk");
        var model = CreateModel();
        var policy = new IntentPolicy()
            .AddRule(new PolicyRule(
                "ComplianceRiskBlock",
                i => i.Signals.Any(s => s.Description.Contains("compliance", StringComparison.OrdinalIgnoreCase) && 
                                        i.Confidence.Level == "Low"),
                PolicyDecision.Block))
            .AddRule(new PolicyRule(
                "HighConfidenceAllow",
                i => i.Confidence.Level is "High" or "Certain",
                PolicyDecision.Allow));

        var intent = model.Infer(space);
        var decision = intent.Decide(policy);

        Assert.Contains(decision, new[] { PolicyDecision.Allow, PolicyDecision.Observe });
    }
}
