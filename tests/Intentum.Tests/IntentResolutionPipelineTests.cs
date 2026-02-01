using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Core.Intents;
using Intentum.Core.Pipeline;

namespace Intentum.Tests;

/// <summary>
/// Tests for the intent resolution pipeline (signal → vector → inference → confidence).
/// </summary>
public sealed class IntentResolutionPipelineTests
{
    [Fact]
    public void Pipeline_WithRuleBasedStep_ProducesSameResultAsRuleBasedIntentModel()
    {
        var rules = new List<Func<BehaviorSpace, Core.Models.RuleMatch?>>
        {
            space => space.Events.Count(e => e.Action == "login.failed") >= 2
                ? new Core.Models.RuleMatch("Suspicious", 0.75, "login.failed>=2")
                : null
        };
        var step = new RuleBasedInferenceStep(rules);
        var pipeline = new IntentResolutionPipeline(step);

        var space = new BehaviorSpace()
            .Observe("user", "login.failed")
            .Observe("user", "login.failed");

        var intent = pipeline.Infer(space);

        Assert.Equal("Suspicious", intent.Name);
        Assert.Equal(0.75, intent.Confidence.Score);
        Assert.Equal("High", intent.Confidence.Level);
        Assert.Equal("login.failed>=2", intent.Reasoning);
    }

    [Fact]
    public void Pipeline_WithCustomConfidenceCalculator_UsesCustomLevels()
    {
        var rules = new List<Func<BehaviorSpace, Core.Models.RuleMatch?>>
        {
            _ => new Core.Models.RuleMatch("X", 0.5)
        };
        var step = new RuleBasedInferenceStep(rules);
        var customConfidence = new CustomConfidenceCalculator();
        var pipeline = new IntentResolutionPipeline(step, confidenceCalculator: customConfidence);

        var space = new BehaviorSpace().Observe("user", "click");
        var intent = pipeline.Infer(space);

        Assert.Equal("X", intent.Name);
        Assert.Equal("Custom", intent.Confidence.Level);
    }

    [Fact]
    public void Pipeline_WithPrecomputedVector_DoesNotCallSignalToVector()
    {
        var rules = new List<Func<BehaviorSpace, Core.Models.RuleMatch?>>
        {
            _ => new Core.Models.RuleMatch("Y", 0.9)
        };
        var step = new RuleBasedInferenceStep(rules);
        var pipeline = new IntentResolutionPipeline(step);

        var space = new BehaviorSpace().Observe("user", "submit");
        var precomputed = new BehaviorVector(new Dictionary<string, double> { ["user:submit"] = 1.0 });

        var intent = pipeline.Infer(space, precomputed);

        Assert.Equal("Y", intent.Name);
        Assert.Equal(0.9, intent.Confidence.Score);
    }

    private sealed class CustomConfidenceCalculator : IConfidenceCalculator
    {
        public IntentConfidence FromScore(double score) => new IntentConfidence(score, "Custom");
    }
}
