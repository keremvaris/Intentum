using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core.Behavior;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

namespace Intentum.Tests;

/// <summary>
/// Tests for fluent API builders: BehaviorSpaceBuilder and IntentPolicyBuilder.
/// </summary>
public class FluentApiTests
{
    [Fact]
    public void IntentPolicyBuilder_Rule_AddsCustomRule()
    {
        var policy = new IntentPolicyBuilder()
            .Rule("CustomObserve", i => i.Confidence.Level == "Medium", PolicyDecision.Observe)
            .Build();
        Assert.Single(policy.Rules);
        Assert.Equal(PolicyDecision.Observe, policy.Rules.First().Decision);
    }

    [Fact]
    public void BehaviorSpaceBuilder_WithActorAndActions_BuildsCorrectly()
    {
        // Arrange & Act
        var space = new BehaviorSpaceBuilder()
            .WithActor("user")
                .Action("login")
                .Action("retry")
                .Action("submit")
            .WithActor("system")
                .Action("validate")
            .Build();

        // Assert
        Assert.Equal(4, space.Events.Count);
        Assert.Equal("user", space.Events.ElementAt(0).Actor);
        Assert.Equal("login", space.Events.ElementAt(0).Action);
        Assert.Equal("user", space.Events.ElementAt(1).Actor);
        Assert.Equal("retry", space.Events.ElementAt(1).Action);
        Assert.Equal("user", space.Events.ElementAt(2).Actor);
        Assert.Equal("submit", space.Events.ElementAt(2).Action);
        Assert.Equal("system", space.Events.ElementAt(3).Actor);
        Assert.Equal("validate", space.Events.ElementAt(3).Action);
    }

    [Fact]
    public void BehaviorSpaceBuilder_ActionWithMetadata_RecordsEventWithMetadata()
    {
        var metadata = new Dictionary<string, object> { ["source"] = "test" };
        var space = new BehaviorSpaceBuilder()
            .WithActor("user")
            .Action("submit", metadata)
            .Build();
        Assert.Single(space.Events);
        Assert.NotNull(space.Events.First().Metadata);
        Assert.Equal("test", space.Events.First().Metadata!["source"]);
    }

    [Fact]
    public void BehaviorSpaceBuilder_WithTimestamp_UsesProvidedTimestamp()
    {
        // Arrange
        var timestamp = DateTimeOffset.UtcNow.AddHours(-1);

        // Act
        var space = new BehaviorSpaceBuilder()
            .WithActor("user")
                .Action("login", timestamp)
            .Build();

        // Assert
        Assert.Single(space.Events);
        Assert.Equal(timestamp, space.Events.First().OccurredAt);
    }

    [Fact]
    public void BehaviorSpaceBuilder_WithMetadata_IncludesMetadata()
    {
        // Arrange
        var metadata = new Dictionary<string, object>
        {
            { "sessionId", "abc123" },
            { "userId", "user456" }
        };

        // Act
        var space = new BehaviorSpaceBuilder()
            .WithActor("user")
                .Action("login", metadata)
            .Build();

        // Assert
        Assert.Single(space.Events);
        Assert.NotNull(space.Events.First().Metadata);
        Assert.Equal("abc123", space.Events.First().Metadata!["sessionId"]);
        Assert.Equal("user456", space.Events.First().Metadata!["userId"]);
    }

    [Fact]
    public void BehaviorSpaceBuilder_ActionWithoutActor_ThrowsException()
    {
        // Arrange
        var builder = new BehaviorSpaceBuilder();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => builder.Action("login"));
    }

    [Fact]
    public void BehaviorSpaceBuilder_ObserveDirectly_AddsEvent()
    {
        // Arrange
        var evt = new BehaviorEvent("user", "login", DateTimeOffset.UtcNow);

        // Act
        var space = new BehaviorSpaceBuilder()
            .Observe(evt)
            .Build();

        // Assert
        Assert.Single(space.Events);
        Assert.Equal(evt, space.Events.First());
    }

    [Fact]
    public void IntentPolicyBuilder_WithRules_BuildsCorrectly()
    {
        // Arrange & Act
        var policy = new IntentPolicyBuilder()
            .Block("ExcessiveRetry", i => i.Signals.Count(s => s.Description.Contains("retry")) >= 3)
            .Allow("HighConfidence", i => i.Confidence.Level is "High" or "Certain")
            .Observe("MediumConfidence", i => i.Confidence.Level == "Medium")
            .Warn("LowConfidence", i => i.Confidence.Level == "Low")
            .Build();

        // Assert
        Assert.Equal(4, policy.Rules.Count);
        Assert.Equal(PolicyDecision.Block, policy.Rules.ElementAt(0).Decision);
        Assert.Equal(PolicyDecision.Allow, policy.Rules.ElementAt(1).Decision);
        Assert.Equal(PolicyDecision.Observe, policy.Rules.ElementAt(2).Decision);
        Assert.Equal(PolicyDecision.Warn, policy.Rules.ElementAt(3).Decision);
    }

    [Fact]
    public void IntentPolicyBuilder_WithCustomRule_AddsRule()
    {
        // Arrange & Act
        var policy = new IntentPolicyBuilder()
            .Rule("CustomRule", i => i.Confidence.Score > 0.5, PolicyDecision.Allow)
            .Build();

        // Assert
        Assert.Single(policy.Rules);
        Assert.Equal("CustomRule", policy.Rules.First().Name);
        Assert.Equal(PolicyDecision.Allow, policy.Rules.First().Decision);
    }

    [Fact]
    public void BehaviorSpaceBuilder_WithLlmIntentModel_WorksCorrectly()
    {
        // Arrange
        var model = new LlmIntentModel(
            new MockEmbeddingProvider(),
            new SimpleAverageSimilarityEngine());

        var space = new BehaviorSpaceBuilder()
            .WithActor("user")
                .Action("login")
                .Action("submit")
            .Build();

        // Act
        var intent = model.Infer(space);

        // Assert
        Assert.NotNull(intent);
        Assert.NotEmpty(intent.Signals);
    }

    [Fact]
    public void IntentPolicyBuilder_WithBehaviorSpace_DecidesCorrectly()
    {
        // Arrange
        var model = new LlmIntentModel(
            new MockEmbeddingProvider(),
            new SimpleAverageSimilarityEngine());

        var space = new BehaviorSpaceBuilder()
            .WithActor("user")
                .Action("login")
                .Action("submit")
            .Build();

        var policy = new IntentPolicyBuilder()
            .Allow("HighConfidence", i => i.Confidence.Level is "High" or "Certain")
            .Observe("MediumConfidence", i => i.Confidence.Level == "Medium")
            .Build();

        // Act
        var intent = model.Infer(space);
        var decision = intent.Decide(policy);

        // Assert
        Assert.Contains(decision, new[] { PolicyDecision.Allow, PolicyDecision.Observe });
    }
}
