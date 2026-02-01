using Intentum.AI.Embeddings;
using Intentum.AI.Similarity;
using Intentum.Core.Behavior;
using Intentum.Core.Intents;
using Intentum.Core.Pipeline;

namespace Intentum.AI.Models;

/// <summary>
/// LLM-based inference step: embeddings + similarity → raw result (name, score, signals, reasoning).
/// Used by IntentResolutionPipeline; also used by LlmIntentModel internally.
/// </summary>
public sealed class LlmInferenceStep : IIntentInferenceStep
{
    private readonly IIntentEmbeddingProvider _embeddingProvider;
    private readonly IIntentSimilarityEngine _similarityEngine;

    /// <summary>
    /// Creates an LLM inference step with the given embedding provider and similarity engine.
    /// </summary>
    public LlmInferenceStep(
        IIntentEmbeddingProvider embeddingProvider,
        IIntentSimilarityEngine similarityEngine)
    {
        _embeddingProvider = embeddingProvider ?? throw new ArgumentNullException(nameof(embeddingProvider));
        _similarityEngine = similarityEngine ?? throw new ArgumentNullException(nameof(similarityEngine));
    }

    /// <inheritdoc />
    public IntentInferenceResult Infer(BehaviorSpace behaviorSpace, BehaviorVector vector)
    {
        var embeddings = vector.Dimensions.Keys
            .Select(_embeddingProvider.Embed)
            .ToList();

        double score;
        if (_similarityEngine is ITimeAwareSimilarityEngine timeAware)
            score = timeAware.CalculateIntentScoreWithTimeDecay(behaviorSpace, embeddings);
        else
            score = _similarityEngine.CalculateIntentScore(embeddings, vector.Dimensions);

        var signals = embeddings.Select(e =>
            new IntentSignal(
                Source: "ai",
                Description: e.Source,
                Weight: e.Score))
            .ToList();

        return new IntentInferenceResult(
            Name: "AI-Inferred-Intent",
            Score: score,
            Signals: signals,
            Reasoning: null
        );
    }
}
