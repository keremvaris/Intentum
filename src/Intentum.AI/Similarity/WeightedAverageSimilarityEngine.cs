using Intentum.AI.Embeddings;

namespace Intentum.AI.Similarity;

/// <summary>
/// Similarity engine that applies weights to embeddings based on their source (actor:action).
/// Useful when certain behaviors should have more influence on intent inference.
/// </summary>
public sealed class WeightedAverageSimilarityEngine : IIntentSimilarityEngine
{
    private readonly IReadOnlyDictionary<string, double> _weights;
    private readonly double _defaultWeight;

    /// <summary>
    /// Creates a weighted average similarity engine.
    /// </summary>
    /// <param name="weights">Dictionary mapping source keys (actor:action) to weights. If a source is not found, defaultWeight is used.</param>
    /// <param name="defaultWeight">Default weight for sources not in the weights dictionary. Defaults to 1.0.</param>
    public WeightedAverageSimilarityEngine(
        IReadOnlyDictionary<string, double>? weights = null,
        double defaultWeight = 1.0)
    {
        _weights = weights ?? new Dictionary<string, double>();
        _defaultWeight = defaultWeight;
    }

    public double CalculateIntentScore(IReadOnlyCollection<IntentEmbedding> embeddings)
        => CalculateIntentScore(embeddings, null);

    /// <inheritdoc />
    public double CalculateIntentScore(IReadOnlyCollection<IntentEmbedding> embeddings, IReadOnlyDictionary<string, double>? sourceWeights)
    {
        if (embeddings.Count == 0)
            return 0;

        var totalWeightedScore = 0.0;
        var totalWeight = 0.0;

        foreach (var embedding in embeddings)
        {
            var weight = sourceWeights?.GetValueOrDefault(embedding.Source, _defaultWeight)
                ?? _weights.GetValueOrDefault(embedding.Source, _defaultWeight);
            totalWeightedScore += embedding.Score * weight;
            totalWeight += weight;
        }

        return totalWeight > 0 ? totalWeightedScore / totalWeight : 0;
    }
}
