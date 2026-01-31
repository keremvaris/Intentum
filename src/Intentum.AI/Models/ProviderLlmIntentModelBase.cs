using Intentum.AI.Embeddings;
using Intentum.AI.Similarity;
using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;

namespace Intentum.AI.Models;

/// <summary>
/// Base implementation for provider-specific LLM intent models (OpenAI, Claude, Gemini, Mistral, Azure OpenAI).
/// Shared logic: embed dimensions, compute score, build signals with a provider source name.
/// </summary>
public abstract class ProviderLlmIntentModelBase(
    string sourceName,
    IIntentEmbeddingProvider embeddingProvider,
    IIntentSimilarityEngine similarityEngine)
    : IIntentModel
{
    /// <inheritdoc />
    public Intent Infer(BehaviorSpace behaviorSpace, BehaviorVector? precomputedVector = null)
    {
        var vector = precomputedVector ?? behaviorSpace.ToVector();

        var embeddings = vector.Dimensions.Keys
            .Select(k => embeddingProvider.Embed(k))
            .ToList();

        var score = similarityEngine.CalculateIntentScore(embeddings);
        var confidence = IntentConfidence.FromScore(score);

        var signals = embeddings.Select(e =>
            new IntentSignal(
                Source: sourceName,
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
