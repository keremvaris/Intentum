using Intentum.AI.Embeddings;
using Intentum.AI.Similarity;
using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;
using Intentum.Core.Pipeline;

namespace Intentum.AI.Models;

/// <summary>
/// AI-assisted intent inference using embeddings and similarity scoring.
/// Uses the resolution pipeline internally (signal → vector → LLM inference → confidence).
/// Uses dimension counts as weights when the engine supports it; uses time decay when the engine implements ITimeAwareSimilarityEngine.
/// </summary>
public sealed class LlmIntentModel : IIntentModel
{
    private readonly IIntentModel _pipeline;

    /// <summary>
    /// Creates an LLM intent model with the given embedding provider and similarity engine.
    /// </summary>
    public LlmIntentModel(
        IIntentEmbeddingProvider embeddingProvider,
        IIntentSimilarityEngine similarityEngine)
    {
        var step = new LlmInferenceStep(
            embeddingProvider ?? throw new ArgumentNullException(nameof(embeddingProvider)),
            similarityEngine ?? throw new ArgumentNullException(nameof(similarityEngine)));
        _pipeline = new IntentResolutionPipeline(step);
    }

    /// <inheritdoc />
    public Intent Infer(BehaviorSpace behaviorSpace, BehaviorVector? precomputedVector = null)
        => _pipeline.Infer(behaviorSpace, precomputedVector);
}
