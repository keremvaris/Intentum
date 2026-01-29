using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core.Behavior;
using Intentum.Runtime;
using Intentum.Runtime.Localization;
using Intentum.Runtime.Policy;
using Intentum.Runtime.RateLimiting;

namespace Intentum.Tests;

public sealed class RuntimeExtensionsTests
{
    [Fact]
    public void Intent_Decide_ReturnsPolicyDecision()
    {
        var model = new LlmIntentModel(new MockEmbeddingProvider(), new SimpleAverageSimilarityEngine());
        var space = new BehaviorSpaceBuilder().WithActor("user").Action("login").Build();
        var policy = new IntentPolicyBuilder()
            .Allow("AllowLogin", _ => true)
            .Build();

        var intent = model.Infer(space);
        var decision = intent.Decide(policy);

        Assert.Equal(PolicyDecision.Allow, decision);
    }

    [Fact]
    public void Intent_DecideWithRateLimit_WhenNotRateLimit_ReturnsDecisionAndNullResult()
    {
        var model = new LlmIntentModel(new MockEmbeddingProvider(), new SimpleAverageSimilarityEngine());
        var space = new BehaviorSpaceBuilder().WithActor("user").Action("login").Build();
        var policy = new IntentPolicyBuilder().Allow("A", _ => true).Build();
        var limiter = new MemoryRateLimiter();
        var options = new RateLimitOptions("key", 10, TimeSpan.FromMinutes(1));

        var intent = model.Infer(space);
        var decision = intent.DecideWithRateLimit(policy, limiter, options, out var rateLimitResult);

        Assert.Equal(PolicyDecision.Allow, decision);
        Assert.Null(rateLimitResult);
    }

    [Fact]
    public void Intent_DecideWithRateLimit_WhenRateLimit_ReturnsDecisionAndRateLimitResult()
    {
        var model = new LlmIntentModel(new MockEmbeddingProvider(), new SimpleAverageSimilarityEngine());
        var space = new BehaviorSpaceBuilder().WithActor("user").Action("login").Build();
        var policy = new IntentPolicyBuilder().RateLimit("R", _ => true).Build();
        var limiter = new MemoryRateLimiter();
        var options = new RateLimitOptions("user-1", 3, TimeSpan.FromMinutes(1));

        var intent = model.Infer(space);
        var decision = intent.DecideWithRateLimit(policy, limiter, options, out var rateLimitResult);

        Assert.Equal(PolicyDecision.RateLimit, decision);
        Assert.NotNull(rateLimitResult);
        Assert.True(rateLimitResult.Allowed);
        Assert.Equal(1, rateLimitResult.CurrentCount);
    }

    [Fact]
    public async Task Intent_DecideWithRateLimitAsync_WhenNotRateLimit_ReturnsDecisionAndNullResult()
    {
        var model = new LlmIntentModel(new MockEmbeddingProvider(), new SimpleAverageSimilarityEngine());
        var space = new BehaviorSpaceBuilder().WithActor("user").Action("login").Build();
        var policy = new IntentPolicyBuilder().Block("B", _ => true).Build();
        var limiter = new MemoryRateLimiter();
        var options = new RateLimitOptions("key", 10, TimeSpan.FromMinutes(1));

        var intent = model.Infer(space);
        var (decision, rateLimitResult) = await intent.DecideWithRateLimitAsync(policy, limiter, options);

        Assert.Equal(PolicyDecision.Block, decision);
        Assert.Null(rateLimitResult);
    }

    [Fact]
    public async Task Intent_DecideWithRateLimitAsync_WhenRateLimit_ReturnsDecisionAndRateLimitResult()
    {
        var model = new LlmIntentModel(new MockEmbeddingProvider(), new SimpleAverageSimilarityEngine());
        var space = new BehaviorSpaceBuilder().WithActor("user").Action("login").Build();
        var policy = new IntentPolicyBuilder().RateLimit("R", _ => true).Build();
        var limiter = new MemoryRateLimiter();
        var options = new RateLimitOptions("user-2", 5, TimeSpan.FromMinutes(1));

        var intent = model.Infer(space);
        var (decision, rateLimitResult) = await intent.DecideWithRateLimitAsync(policy, limiter, options);

        Assert.Equal(PolicyDecision.RateLimit, decision);
        Assert.NotNull(rateLimitResult);
        Assert.True(rateLimitResult.Allowed);
    }
}
