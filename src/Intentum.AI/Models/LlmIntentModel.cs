using Intentum.AI.Embeddings;
using Intentum.AI.Similarity;
using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;

namespace Intentum.AI.Models;

/// <summary>
/// AI-assisted intent inference using embeddings and similarity scoring.
/// Uses dimension counts as weights when the engine supports it; uses time decay when the engine implements ITimeAwareSimilarityEngine.
/// </summary>
public sealed class LlmIntentModel(
    IIntentEmbeddingProvider embeddingProvider,
    IIntentSimilarityEngine similarityEngine) : IIntentModel
{
    public Intent Infer(BehaviorSpace behaviorSpace, BehaviorVector? precomputedVector = null)
    {
        var vector = precomputedVector ?? behaviorSpace.ToVector();

        var embeddings = vector.Dimensions.Keys
            .Select(embeddingProvider.Embed)
            .ToList();

        double score;
        if (similarityEngine is ITimeAwareSimilarityEngine timeAware)
            score = timeAware.CalculateIntentScoreWithTimeDecay(behaviorSpace, embeddings);
        else
            score = similarityEngine.CalculateIntentScore(embeddings, vector.Dimensions);

        var confidence = IntentConfidence.FromScore(score);

        var signals = embeddings.Select(e =>
            new IntentSignal(
                Source: "ai",
                Description: e.Source,
                Weight: e.Score))
            .ToList();

        return new Intent(
            Name: "AI-Inferred-Intent",
            Signals: signals,
            Confidence: confidence,
            Reasoning: null
        );
    }
}
