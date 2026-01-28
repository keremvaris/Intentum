using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

namespace Intentum.Tests;

/// <summary>
/// Sector-based scenario tests: ESG (reporting, compliance), Carbon Accounting (verification, audit),
/// Sukuk & Islamic Finance (issuance, ICMA compliance). Showcase for domain-specific Observe → Infer → Decide flows.
/// </summary>
public class SectorScenarioTests
{
    private static LlmIntentModel CreateModel()
    {
        return new LlmIntentModel(
            new MockEmbeddingProvider(),
            new SimpleAverageSimilarityEngine());
    }

    [Fact]
    public void ESG_ReportHappyPath_AllowsOrObserves()
    {
        var space = new BehaviorSpace()
            .Observe("analyst", "prepare_esg_report")
            .Observe("compliance", "approve")
            .Observe("system", "publish_esg");
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

        Assert.Contains(decision, new[] { PolicyDecision.Allow, PolicyDecision.Observe });
    }

    [Fact]
    public void ESG_ReportWithComplianceIssues_BlockOrAllow()
    {
        var space = new BehaviorSpace()
            .Observe("analyst", "prepare_esg_report")
            .Observe("compliance", "flag_issue")
            .Observe("analyst", "retry_correction")
            .Observe("analyst", "retry_correction")
            .Observe("compliance", "approve");
        var model = CreateModel();
        var policy = new IntentPolicy()
            .AddRule(new PolicyRule(
                "ExcessiveRetryBlock",
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

        Assert.Contains(decision, new[] { PolicyDecision.Block, PolicyDecision.Allow, PolicyDecision.Observe, PolicyDecision.Warn });
    }

    [Fact]
    public void Carbon_VerificationWithCorrections_ProducesValidDecision()
    {
        var space = new BehaviorSpace()
            .Observe("analyst", "calculate_carbon")
            .Observe("verifier", "verify")
            .Observe("verifier", "request_correction")
            .Observe("analyst", "correct")
            .Observe("verifier", "approve");
        var model = CreateModel();
        var policy = new IntentPolicy()
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

        Assert.Contains(decision, new[] { PolicyDecision.Allow, PolicyDecision.Observe });
    }

    [Fact]
    public void Sukuk_IssuanceWithShariaReview_AllowsOrObserves()
    {
        var space = new BehaviorSpace()
            .Observe("issuer", "initiate_sukuk")
            .Observe("sharia", "review")
            .Observe("sharia", "approve")
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
    public void Sukuk_ICMAComplianceCheck_BlocksOrObserves()
    {
        var space = new BehaviorSpace()
            .Observe("issuer", "initiate_sukuk")
            .Observe("sharia", "review")
            .Observe("icma", "check_compliance")
            .Observe("icma", "request_adjustment")
            .Observe("issuer", "adjust")
            .Observe("icma", "approve")
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

        Assert.Contains(decision, new[] { PolicyDecision.Block, PolicyDecision.Allow, PolicyDecision.Observe, PolicyDecision.Warn });
    }

    // ---- Classic examples (Payment, Support, E‑commerce) ----
    [Fact]
    public void Fintech_PaymentHappyPath_AllowsOrObserves()
    {
        var space = new BehaviorSpace()
            .Observe("user", "login")
            .Observe("user", "submit");
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

        Assert.Contains(decision, new[] { PolicyDecision.Allow, PolicyDecision.Observe });
    }

    [Fact]
    public void Support_EscalationAfterRetries_ProducesValidDecision()
    {
        var space = new BehaviorSpace()
            .Observe("user", "ask")
            .Observe("user", "ask")
            .Observe("system", "escalate");
        var model = CreateModel();
        var policy = new IntentPolicy()
            .AddRule(new PolicyRule(
                "WarnOnEscalate",
                i => i.Signals.Any(s => s.Description.Contains("escalate", StringComparison.OrdinalIgnoreCase)),
                PolicyDecision.Warn))
            .AddRule(new PolicyRule(
                "HighConfidenceAllow",
                i => i.Confidence.Level is "High" or "Certain",
                PolicyDecision.Allow))
            .AddRule(new PolicyRule("DefaultObserve", _ => true, PolicyDecision.Observe));

        var intent = model.Infer(space);
        var decision = intent.Decide(policy);

        Assert.Contains(decision, new[] { PolicyDecision.Warn, PolicyDecision.Allow, PolicyDecision.Observe });
    }

    [Fact]
    public void ECommerce_CheckoutWithRetries_AllowsOrObserves()
    {
        var space = new BehaviorSpace()
            .Observe("user", "cart")
            .Observe("user", "checkout")
            .Observe("user", "retry")
            .Observe("user", "submit");
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
}
