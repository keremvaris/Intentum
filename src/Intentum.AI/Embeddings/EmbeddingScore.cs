using Intentum.AI.Similarity;

namespace Intentum.AI.Embeddings;

/// <summary>
/// Shared helpers for embedding score normalization used by provider embedding implementations.
/// </summary>
public static class EmbeddingScore
{
    /// <summary>
    /// Computes a scalar score from an embedding vector using L2 magnitude,
    /// normalized to [0, 1]. This provides a rough "activation strength" indicator.
    /// For meaningful similarity, use cosine similarity between two vectors instead.
    /// </summary>
    public static double Normalize(IReadOnlyList<double> values)
    {
        if (values.Count == 0)
            return 0;

        var sumOfSquares = 0.0;
        foreach (var v in values)
            sumOfSquares += v * v;

        var magnitude = Math.Sqrt(sumOfSquares / values.Count);
        return Math.Clamp(magnitude, 0.0, 1.0);
    }

    /// <summary>
    /// Computes cosine similarity between two embedding vectors. Returns a value in [0, 1]
    /// where 1 means identical direction and 0 means orthogonal or opposite.
    /// </summary>
    public static double CosineSimilarity(IReadOnlyList<double> a, IReadOnlyList<double> b)
    {
        if (a.Count == 0 || b.Count == 0 || a.Count != b.Count)
            return 0;

        return CosineSimilarityHelper.CosineSimilarityNormalized(a.ToArray(), b.ToArray());
    }
}
