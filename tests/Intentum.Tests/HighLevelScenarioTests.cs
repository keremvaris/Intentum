using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

namespace Intentum.Tests;

/// <summary>
/// High-complexity scenario tests: multi-actor ESG compliance, carbon accounting with validators,
/// ESG risk assessment. Showcase for complex policy and signal combinations.
/// </summary>
public class HighLevelScenarioTests
{
    private static LlmIntentModel CreateModel()
    {
        return new LlmIntentModel(
            new MockEmbeddingProvider(),
            new SimpleAverageSimilarityEngine());
    }

    [Fact]
    public void ESGComplianceAuditTrail_MultiActor_BlockOrAllow()
    {
        var space = new BehaviorSpace()
            .Observe("analyst", "prepare_esg_report")
            .Observe("compliance", "review_esg")
            .Observe("compliance", "flag_discrepancy")
            .Observe("analyst", "retry_correction")
            .Observe("compliance", "approve")
            .Observe("system", "publish_esg");
        var model = CreateModel();
        var policy = new IntentPolicy()
            .AddRule(new PolicyRule(
                "BlockExcessiveRetry",
                i => i.Signals.Count(s => s.Description.Contains("retry", StringComparison.OrdinalIgnoreCase)) >= 3,
                PolicyDecision.Block))
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

        Assert.Contains(decision, new[] { PolicyDecision.Block, PolicyDecision.Allow, PolicyDecision.Observe });
        Assert.Equal(6, space.Events.Count);
    }

    [Fact]
    public void CarbonAccountingWithMultipleValidators_ProducesValidIntentAndDecision()
    {
        var space = new BehaviorSpace()
            .Observe("analyst", "calculate_carbon")
            .Observe("internal_audit", "review")
            .Observe("external_verifier", "verify")
            .Observe("external_verifier", "request_changes")
            .Observe("analyst", "update")
            .Observe("external_verifier", "certify");
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
        Assert.Equal(6, space.Events.Count);
        Assert.InRange(intent.Confidence.Score, 0.0, 1.0);
    }

    [Fact]
    public void ESGRiskAssessmentWithMultipleStakeholders_ProducesValidDecision()
    {
        var space = new BehaviorSpace()
            .Observe("analyst", "assess_esg_risk")
            .Observe("risk_committee", "review")
            .Observe("risk_committee", "request_details")
            .Observe("analyst", "provide_details")
            .Observe("risk_committee", "approve")
            .Observe("board", "final_approval");
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
                PolicyDecision.Allow))
            .AddRule(new PolicyRule(
                "DefaultObserve",
                _ => true,
                PolicyDecision.Observe));

        var intent = model.Infer(space);
        var decision = intent.Decide(policy);

        Assert.Contains(decision, new[] { PolicyDecision.Block, PolicyDecision.Allow, PolicyDecision.Observe });
        Assert.Equal(6, space.Events.Count);
    }
}
