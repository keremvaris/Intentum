using Intentum.AI.Embeddings;

namespace Intentum.AI.Similarity;

public sealed class SimpleAverageSimilarityEngine : IIntentSimilarityEngine
{
    public double CalculateIntentScore(IReadOnlyCollection<IntentEmbedding> embeddings)
    {
        if (embeddings.Count == 0)
            return 0;

        return embeddings.Average(e => e.Score);
    }
}
