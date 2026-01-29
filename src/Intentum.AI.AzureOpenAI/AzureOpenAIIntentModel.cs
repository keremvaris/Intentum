using Intentum.AI.Embeddings;
using Intentum.AI.Similarity;
using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;

namespace Intentum.AI.AzureOpenAI;

public sealed class AzureOpenAIIntentModel(
    IIntentEmbeddingProvider embeddingProvider,
    IIntentSimilarityEngine similarityEngine)
    : IIntentModel
{
    public Intent Infer(BehaviorSpace behaviorSpace)
    {
        var vector = behaviorSpace.ToVector();

        var embeddings = vector.Dimensions.Keys
            .Select(embeddingProvider.Embed)
            .ToList();

        var score = similarityEngine.CalculateIntentScore(embeddings);
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
