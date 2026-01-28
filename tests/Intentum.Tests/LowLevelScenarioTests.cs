using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

namespace Intentum.Tests;

/// <summary>
/// Low-complexity scenario tests: simple BehaviorSpace, few events, basic policy (Allow/Observe/Warn).
/// Showcase for minimal Observe → Infer → Decide flows.
/// </summary>
public class LowLevelScenarioTests
{
    private static LlmIntentModel CreateModel()
    {
        return new LlmIntentModel(
            new MockEmbeddingProvider(),
            new SimpleAverageSimilarityEngine());
    }

    private static IntentPolicy CreateDefaultPolicy()
    {
        return new IntentPolicy()
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
    }

    [Fact]
    public void CarbonFootprintCalculation_AllowsOrObserves_ByConfidence()
    {
        var space = new BehaviorSpace()
            .Observe("analyst", "calculate_carbon")
            .Observe("system", "report_generated");
        var model = CreateModel();
        var policy = CreateDefaultPolicy();

        var intent = model.Infer(space);
        var decision = intent.Decide(policy);

        Assert.Contains(decision, new[] { PolicyDecision.Allow, PolicyDecision.Observe, PolicyDecision.Warn });
        Assert.Equal(2, space.Events.Count);
    }

    [Fact]
    public void ESGReportSubmission_AllowsOrObserves_ByConfidence()
    {
        var space = new BehaviorSpace()
            .Observe("analyst", "prepare_esg_report")
            .Observe("compliance", "approve")
            .Observe("system", "publish_esg");
        var model = CreateModel();
        var policy = CreateDefaultPolicy();

        var intent = model.Infer(space);
        var decision = intent.Decide(policy);

        Assert.Contains(decision, new[] { PolicyDecision.Allow, PolicyDecision.Observe, PolicyDecision.Warn });
        Assert.Equal(3, space.Events.Count);
    }

    [Fact]
    public void ESGMetricView_ProducesWarnOrObserve()
    {
        var space = new BehaviorSpace()
            .Observe("user", "view_esg_metric");
        var model = CreateModel();
        var policy = CreateDefaultPolicy();

        var intent = model.Infer(space);
        var decision = intent.Decide(policy);

        Assert.Contains(decision, new[] { PolicyDecision.Warn, PolicyDecision.Observe, PolicyDecision.Allow });
        Assert.Single(space.Events);
    }

    [Fact]
    public void ECommerce_AddToCart_ProducesValidIntentAndDecision()
    {
        var space = new BehaviorSpace()
            .Observe("user", "view_product")
            .Observe("user", "add_to_cart");
        var model = CreateModel();
        var policy = CreateDefaultPolicy();

        var intent = model.Infer(space);
        var decision = intent.Decide(policy);

        Assert.NotNull(intent);
        Assert.InRange(intent.Confidence.Score, 0.0, 1.0);
        Assert.Contains(decision, new[] { PolicyDecision.Allow, PolicyDecision.Observe, PolicyDecision.Warn });
        Assert.Equal(2, space.Events.Count);
    }
}
