using Intentum.AI.Embeddings;
using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core.Behavior;
using Intentum.Runtime.Policy;

namespace Intentum.Testing;

/// <summary>
/// Test helpers for creating common test objects.
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Creates a default LlmIntentModel with MockEmbeddingProvider and SimpleAverageSimilarityEngine.
    /// </summary>
    public static LlmIntentModel CreateDefaultModel()
    {
        return new LlmIntentModel(
            new MockEmbeddingProvider(),
            new SimpleAverageSimilarityEngine());
    }

    /// <summary>
    /// Creates a LlmIntentModel with custom embedding provider and similarity engine.
    /// </summary>
    public static LlmIntentModel CreateModel(
        IIntentEmbeddingProvider embeddingProvider,
        IIntentSimilarityEngine similarityEngine)
    {
        return new LlmIntentModel(embeddingProvider, similarityEngine);
    }

    /// <summary>
    /// Creates a default IntentPolicy with common rules.
    /// </summary>
    public static IntentPolicy CreateDefaultPolicy()
    {
        return new IntentPolicyBuilder()
            .Block("ExcessiveRetryBlock", i => i.Signals.Count(s => s.Description.Contains("retry", StringComparison.OrdinalIgnoreCase)) >= 3)
            .Allow("HighConfidenceAllow", i => i.Confidence.Level is "High" or "Certain")
            .Observe("MediumConfidenceObserve", i => i.Confidence.Level == "Medium")
            .Warn("LowConfidenceWarn", i => i.Confidence.Level == "Low")
            .Build();
    }

    /// <summary>
    /// Creates a BehaviorSpaceBuilder for fluent test setup.
    /// </summary>
    public static BehaviorSpaceBuilder CreateSpaceBuilder()
    {
        return new BehaviorSpaceBuilder();
    }

    /// <summary>
    /// Creates a simple behavior space with login and submit actions.
    /// </summary>
    public static BehaviorSpace CreateSimpleSpace()
    {
        return new BehaviorSpaceBuilder()
            .WithActor("user")
                .Action("login")
                .Action("submit")
            .Build();
    }

    /// <summary>
    /// Creates a behavior space with retries.
    /// </summary>
    public static BehaviorSpace CreateSpaceWithRetries(int retryCount = 2)
    {
        var builder = new BehaviorSpaceBuilder()
            .WithActor("user")
                .Action("login");

        for (int i = 0; i < retryCount; i++)
        {
            builder.Action("retry");
        }

        return builder
            .Action("submit")
            .Build();
    }
}
