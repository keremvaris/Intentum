using Intentum.AI.Embeddings;

namespace Intentum.AI.Similarity;

/// <summary>
/// Combines embeddings into a single intent score (e.g. average, weighted, time-decay).
/// </summary>
public interface IIntentSimilarityEngine
{
    /// <summary>Calculates intent score from embeddings.</summary>
    double CalculateIntentScore(IReadOnlyCollection<IntentEmbedding> embeddings);

    /// <summary>Calculates intent score with optional per-source weights (e.g. event counts). Default implementation ignores weights.</summary>
    double CalculateIntentScore(IReadOnlyCollection<IntentEmbedding> embeddings, IReadOnlyDictionary<string, double>? sourceWeights)
        => CalculateIntentScore(embeddings);
}
