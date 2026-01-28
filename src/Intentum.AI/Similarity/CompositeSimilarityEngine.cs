using Intentum.AI.Embeddings;

namespace Intentum.AI.Similarity;

/// <summary>
/// Similarity engine that combines multiple similarity engines using weighted combination.
/// Useful for A/B testing or combining different similarity strategies.
/// </summary>
public sealed class CompositeSimilarityEngine : IIntentSimilarityEngine
{
    private readonly List<(IIntentSimilarityEngine Engine, double Weight)> _engines;

    /// <summary>
    /// Creates a composite similarity engine with equal weights for all engines.
    /// </summary>
    public CompositeSimilarityEngine(IEnumerable<IIntentSimilarityEngine> engines)
        : this(engines.Select(e => (e, 1.0)).ToArray())
    {
    }

    /// <summary>
    /// Creates a composite similarity engine with custom weights.
    /// </summary>
    public CompositeSimilarityEngine(params (IIntentSimilarityEngine Engine, double Weight)[] engines)
    {
        if (engines == null || engines.Length == 0)
            throw new ArgumentException("At least one engine is required", nameof(engines));

        _engines = engines.ToList();
    }

    public double CalculateIntentScore(IReadOnlyCollection<IntentEmbedding> embeddings)
    {
        if (embeddings.Count == 0)
            return 0;

        var totalWeightedScore = 0.0;
        var totalWeight = 0.0;

        foreach (var (engine, weight) in _engines)
        {
            var score = engine.CalculateIntentScore(embeddings);
            totalWeightedScore += score * weight;
            totalWeight += weight;
        }

        return totalWeight > 0 ? totalWeightedScore / totalWeight : 0;
    }
}
