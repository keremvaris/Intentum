namespace Intentum.AI.Similarity;

/// <summary>
/// Standard cosine similarity implementation.
/// </summary>
public static class CosineSimilarityHelper
{
    /// <summary>
    /// Computes cosine similarity between two vectors. Returns a value in [-1, 1] range.
    /// 1 means identical direction, 0 means orthogonal, -1 means opposite direction.
    /// </summary>
    /// <param name="a">First vector.</param>
    /// <param name="b">Second vector.</param>
    /// <returns>Cosine similarity in [-1, 1].</returns>
    /// <exception cref="ArgumentException">Thrown when vectors have different lengths.</exception>
    public static double CosineSimilarity(double[] a, double[] b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException("Vectors must have the same length");

        double dotProduct = 0, normA = 0, normB = 0;

        for (int i = 0; i < a.Length; i++)
        {
            dotProduct += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }

        const double epsilon = 1e-10;
        if (normA < epsilon || normB < epsilon)
            return 0;

        return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }

    /// <summary>
    /// Computes cosine similarity normalized to [0, 1] range.
    /// 1 means identical direction, 0.5 means orthogonal, 0 means opposite direction.
    /// </summary>
    /// <param name="a">First vector.</param>
    /// <param name="b">Second vector.</param>
    /// <returns>Normalized cosine similarity in [0, 1].</returns>
    /// <exception cref="ArgumentException">Thrown when vectors have different lengths.</exception>
    public static double CosineSimilarityNormalized(double[] a, double[] b)
        => (CosineSimilarity(a, b) + 1) / 2;
}