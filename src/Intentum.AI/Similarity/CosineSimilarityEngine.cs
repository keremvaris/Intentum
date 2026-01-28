using Intentum.AI.Embeddings;

namespace Intentum.AI.Similarity;

/// <summary>
/// Similarity engine that uses cosine similarity between embedding vectors.
/// Falls back to simple average if vectors are not available.
/// </summary>
public sealed class CosineSimilarityEngine : IIntentSimilarityEngine
{
    public double CalculateIntentScore(IReadOnlyCollection<IntentEmbedding> embeddings)
    {
        if (embeddings.Count == 0)
            return 0;

        // Check if all embeddings have vectors
        var allHaveVectors = embeddings.All(e => e.Vector != null && e.Vector.Count > 0);
        
        if (!allHaveVectors)
        {
            // Fallback to simple average if vectors are not available
            return embeddings.Average(e => e.Score);
        }

        // Calculate cosine similarity between all pairs and average
        var embeddingList = embeddings.ToList();
        var similarities = new List<double>();

        for (int i = 0; i < embeddingList.Count; i++)
        {
            for (int j = i + 1; j < embeddingList.Count; j++)
            {
                var similarity = CalculateCosineSimilarity(
                    embeddingList[i].Vector!,
                    embeddingList[j].Vector!);
                similarities.Add(similarity);
            }
        }

        // If only one embedding, return its score
        if (similarities.Count == 0)
            return embeddings.First().Score;

        // Average of all pairwise cosine similarities
        return similarities.Average();
    }

    /// <summary>
    /// Calculates cosine similarity between two vectors.
    /// Returns a value between -1 and 1, where 1 means identical, 0 means orthogonal, -1 means opposite.
    /// </summary>
    private static double CalculateCosineSimilarity(
        IReadOnlyList<double> vector1,
        IReadOnlyList<double> vector2)
    {
        if (vector1.Count != vector2.Count)
            throw new ArgumentException("Vectors must have the same length");

        double dotProduct = 0;
        double magnitude1 = 0;
        double magnitude2 = 0;

        for (int i = 0; i < vector1.Count; i++)
        {
            dotProduct += vector1[i] * vector2[i];
            magnitude1 += vector1[i] * vector1[i];
            magnitude2 += vector2[i] * vector2[i];
        }

        magnitude1 = Math.Sqrt(magnitude1);
        magnitude2 = Math.Sqrt(magnitude2);

        const double epsilon = 1e-10;
        if (magnitude1 < epsilon || magnitude2 < epsilon)
            return 0;

        // Cosine similarity: dot product / (magnitude1 * magnitude2)
        // Normalize to 0-1 range for consistency with other engines
        var cosineSimilarity = dotProduct / (magnitude1 * magnitude2);
        
        // Map from [-1, 1] to [0, 1] for consistency
        return (cosineSimilarity + 1) / 2;
    }
}
