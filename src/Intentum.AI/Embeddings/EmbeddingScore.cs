namespace Intentum.AI.Embeddings;

/// <summary>
/// Shared helpers for embedding score normalization used by provider embedding implementations.
/// </summary>
public static class EmbeddingScore
{
    /// <summary>
    /// Normalizes a list of embedding values to a score in [0, 1] using average of absolute values.
    /// </summary>
    public static double Normalize(IReadOnlyList<double> values)
    {
        if (values.Count == 0)
            return 0;

        var avgAbs = values.Average(Math.Abs);
        return Math.Clamp(avgAbs, 0.0, 1.0);
    }
}
