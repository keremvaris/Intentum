using Intentum.AI.Embeddings;

namespace Intentum.AI.Similarity;

/// <summary>
/// Similarity engine that averages embedding scores; optionally weighted by source (e.g. event counts).
/// </summary>
public sealed class SimpleAverageSimilarityEngine : IIntentSimilarityEngine
{
    /// <inheritdoc />
    public double CalculateIntentScore(IReadOnlyCollection<IntentEmbedding> embeddings)
        => CalculateIntentScore(embeddings, null);

    /// <inheritdoc />
    public double CalculateIntentScore(IReadOnlyCollection<IntentEmbedding> embeddings, IReadOnlyDictionary<string, double>? sourceWeights)
    {
        if (embeddings.Count == 0)
            return 0;

        if (sourceWeights == null || sourceWeights.Count == 0)
            return embeddings.Average(e => e.Score);

        var totalWeighted = 0.0;
        var totalWeight = 0.0;
        foreach (var e in embeddings)
        {
            var w = sourceWeights.GetValueOrDefault(e.Source, 1.0);
            totalWeighted += e.Score * w;
            totalWeight += w;
        }
        return totalWeight > 0 ? totalWeighted / totalWeight : embeddings.Average(x => x.Score);
    }
}
