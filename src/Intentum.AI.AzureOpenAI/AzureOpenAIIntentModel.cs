using Intentum.AI.Embeddings;
using Intentum.AI.Similarity;
using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;

namespace Intentum.AI.AzureOpenAI;

public sealed class AzureOpenAIIntentModel : IIntentModel
{
    private readonly IIntentEmbeddingProvider _embeddingProvider;
    private readonly IIntentSimilarityEngine _similarityEngine;

    public AzureOpenAIIntentModel(
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
                Source: "azure-openai",
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
