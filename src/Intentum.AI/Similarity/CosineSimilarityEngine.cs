using Intentum.AI.Embeddings;
using Intentum.AI.Similarity;

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
        var allHaveVectors = embeddings.All(e => e.Vector is { Count: > 0 });

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

        return CosineSimilarityHelper.CosineSimilarityNormalized(vector1.ToArray(), vector2.ToArray());
    }
}
