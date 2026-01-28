using Intentum.AI.Embeddings;

namespace Intentum.AI.Mock;

/// <summary>
/// Deterministic mock for offline and CI usage.
/// </summary>
public sealed class MockEmbeddingProvider : IIntentEmbeddingProvider
{
    public IntentEmbedding Embed(string behaviorKey)
    {
        var hash = behaviorKey.GetHashCode();
        var normalized = Math.Abs(hash % 100) / 100.0;

        // Generate a simple deterministic vector for cosine similarity support
        var vector = GenerateDeterministicVector(behaviorKey, dimension: 8);

        return new IntentEmbedding(
            Source: behaviorKey,
            Score: normalized,
            Vector: vector
        );
    }

    /// <summary>
    /// Generates a deterministic vector from a behavior key for testing purposes.
    /// </summary>
    private static double[] GenerateDeterministicVector(string behaviorKey, int dimension)
    {
        var vector = new double[dimension];
        var hash = behaviorKey.GetHashCode();
        var random = new Random(hash); // Deterministic seed

        for (int i = 0; i < dimension; i++)
        {
            vector[i] = random.NextDouble() * 2 - 1; // Range: [-1, 1]
        }

        // Normalize the vector
        var magnitude = Math.Sqrt(vector.Sum(x => x * x));
        if (magnitude > 0)
        {
            for (int i = 0; i < dimension; i++)
            {
                vector[i] /= magnitude;
            }
        }

        return vector;
    }
}
