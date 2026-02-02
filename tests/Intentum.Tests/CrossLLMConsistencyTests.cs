using Intentum.AI.Embeddings;
using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;

namespace Intentum.Tests;

/// <summary>
/// Cross-provider consistency: same BehaviorSpace, different embedding providers.
/// With mocks we only verify the pipeline runs and produces valid intents; score variance is documented.
/// With real API keys (OpenAI, Azure, Gemini, etc.) run the same space through each provider and compare confidence scores.
/// </summary>
public class CrossLLMConsistencyTests
{
    /// <summary>
    /// Mock that returns a fixed score (e.g. 0.5) for every key, to simulate a "different" provider.
    /// </summary>
    private sealed class FixedScoreEmbeddingProvider(double score) : IIntentEmbeddingProvider
    {
        public IntentEmbedding Embed(string behaviorKey) =>
            new IntentEmbedding(Source: behaviorKey, Score: score);
    }

    [Fact]
    public void CrossProvider_SameSpace_TwoProviders_ProduceValidIntents()
    {
        var space = new BehaviorSpace()
            .Observe("user", "login")
            .Observe("user", "submit");

        var model1 = new LlmIntentModel(new MockEmbeddingProvider(), new SimpleAverageSimilarityEngine());
        var model2 = new LlmIntentModel(new FixedScoreEmbeddingProvider(0.5), new SimpleAverageSimilarityEngine());

        var intent1 = model1.Infer(space);
        var intent2 = model2.Infer(space);

        Assert.NotNull(intent1);
        Assert.NotNull(intent2);
        Assert.InRange(intent1.Confidence.Score, 0.0, 1.0);
        Assert.InRange(intent2.Confidence.Score, 0.0, 1.0);
        // Different providers can yield different scores; document variance in real runs (see docs/case-studies/cross-llm-consistency.md).
        var scoreDiff = Math.Abs(intent1.Confidence.Score - intent2.Confidence.Score);
        Assert.True(scoreDiff is >= 0 and <= 1.0);
    }
}
