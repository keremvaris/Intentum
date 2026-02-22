using Intentum.AI.Catalog;
using Intentum.AI.Embeddings;
using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;
using Intentum.Core.Pipeline;

namespace Intentum.AI.Models;

/// <summary>
/// Intent model that classifies behavior against a catalog of known intents
/// using embedding cosine similarity (nearest neighbor classification).
/// Returns the actual intent name from the catalog instead of a generic "AI-Inferred-Intent".
/// </summary>
public sealed class CatalogIntentModel : IIntentModel
{
    private readonly IntentResolutionPipeline _pipeline;

    public CatalogIntentModel(
        IIntentEmbeddingProvider embeddingProvider,
        IntentCatalog catalog)
    {
        var step = new CatalogInferenceStep(
            embeddingProvider ?? throw new ArgumentNullException(nameof(embeddingProvider)),
            catalog ?? throw new ArgumentNullException(nameof(catalog)));
        _pipeline = new IntentResolutionPipeline(step);
    }

    /// <inheritdoc />
    public Intent Infer(BehaviorSpace behaviorSpace, BehaviorVector? precomputedVector = null)
        => _pipeline.Infer(behaviorSpace, precomputedVector);
}
