using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core.Behavior;
using Intentum.Core.Evaluation;
using Intentum.Core.Intents;
using Intentum.Runtime.Engine;
using Intentum.Runtime.Policy;

namespace Intentum.Tests;

public class IntentumCoreTests
{
    [Fact]
    public void BehaviorSpace_ToVector_CountsActions()
    {
        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("user", "login", DateTimeOffset.UtcNow));
        space.Observe(new BehaviorEvent("user", "login", DateTimeOffset.UtcNow));

        var vector = space.ToVector();

        Assert.True(vector.Dimensions.TryGetValue("user:login", out var count));
        Assert.Equal(2, count);
    }

    [Fact]
    public void IntentEvaluator_ReturnsLowConfidence_WhenSignalsAreWeak()
    {
        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("user", "retry", DateTimeOffset.UtcNow));

        var evaluator = new IntentEvaluator();
        var result = evaluator.Evaluate("AuthIntent", space);

        Assert.Equal("Low", result.Intent.Confidence.Level);
    }

    [Fact]
    public void LlmIntentModel_InfersIntent_FromBehaviorSpace()
    {
        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("user", "submit", DateTimeOffset.UtcNow));

        var model = new LlmIntentModel(
            new MockEmbeddingProvider(),
            new SimpleAverageSimilarityEngine());

        var intent = model.Infer(space);

        Assert.NotNull(intent);
        Assert.NotEmpty(intent.Signals);
    }
}

public class IntentumRuntimeTests
{
    [Fact]
    public void PolicyEngine_ReturnsFirstMatchingDecision()
    {
        var intent = new Intent("Test", [], new IntentConfidence(0.9, "High"));
        var policy = new IntentPolicy()
            .AddRule(new PolicyRule("AllowHigh", i => i.Confidence.Level == "High", PolicyDecision.Allow))
            .AddRule(new PolicyRule("WarnLow", i => i.Confidence.Level == "Low", PolicyDecision.Warn));

        var engine = new IntentPolicyEngine();
        var decision = engine.Evaluate(intent, policy);

        Assert.Equal(PolicyDecision.Allow, decision);
    }

    [Fact]
    public void ComplexScenario_MixedActorsAndDrift_ResolvesDeterministically()
    {
        var space = new BehaviorSpace();
        var now = DateTimeOffset.UtcNow;

        // Mixed actors and bursty retries to simulate a real noisy flow.
        space.Observe(new BehaviorEvent("user", "login", now.AddSeconds(-30)));
        space.Observe(new BehaviorEvent("user", "retry_1", now.AddSeconds(-25)));
        space.Observe(new BehaviorEvent("user", "retry_2", now.AddSeconds(-20)));
        space.Observe(new BehaviorEvent("system", "rate_limit", now.AddSeconds(-18)));
        space.Observe(new BehaviorEvent("user", "retry_3", now.AddSeconds(-15)));
        space.Observe(new BehaviorEvent("system", "challenge", now.AddSeconds(-10)));
        space.Observe(new BehaviorEvent("user", "submit", now.AddSeconds(-5)));

        var model = new LlmIntentModel(
            new MockEmbeddingProvider(),
            new SimpleAverageSimilarityEngine());

        var intent = model.Infer(space);

        var policy = new IntentPolicy()
            .AddRule(new PolicyRule(
                "HardBlockOnRateLimitWithRetries",
                i => i.Signals.Any(s => s.Description.Contains("rate_limit", StringComparison.OrdinalIgnoreCase)) &&
                     i.Signals.Count(s => s.Description.Contains("retry", StringComparison.OrdinalIgnoreCase)) >= 3,
                PolicyDecision.Block))
            .AddRule(new PolicyRule(
                "WarnOnChallenge",
                i => i.Signals.Any(s => s.Description.Contains("challenge", StringComparison.OrdinalIgnoreCase)),
                PolicyDecision.Warn))
            .AddRule(new PolicyRule(
                "AllowOnHighConfidence",
                i => i.Confidence.Level is "High" or "Certain",
                PolicyDecision.Allow));

        var engine = new IntentPolicyEngine();
        var decision = engine.Evaluate(intent, policy);

        Assert.Equal(PolicyDecision.Block, decision);
        Assert.InRange(intent.Confidence.Score, 0.0, 1.0);
        Assert.Contains(
            space.ToVector().Dimensions.Keys,
            k => k.Contains("retry", StringComparison.OrdinalIgnoreCase));
    }
}
