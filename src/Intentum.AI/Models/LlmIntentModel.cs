using Intentum.AI.Embeddings;
using Intentum.AI.Similarity;
using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intent;
using Intentum.Core.Intents;

namespace Intentum.AI.Models;

/// <summary>
/// AI-assisted intent inference using embeddings and similarity scoring.
/// </summary>
public sealed class LlmIntentModel : IIntentModel
{
    private readonly IIntentEmbeddingProvider _embeddingProvider;
    private readonly IIntentSimilarityEngine _similarityEngine;

    public LlmIntentModel(
        IIntentEmbeddingProvider embeddingProvider,
        IIntentSimilarityEngine similarityEngine)
    {
        _embeddingProvider = embeddingProvider;
        _similarityEngine = similarityEngine;
    }

    public Intent Infer(BehaviorSpace behaviorSpace)
    {
        var vector = behaviorSpace.ToVector();

        var embeddings = vector.Dimensions.Keys
            .Select(k => _embeddingProvider.Embed(k))
            .ToList();

        var score = _similarityEngine.CalculateIntentScore(embeddings);
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
            Confidence: confidence
        );
    }
}
