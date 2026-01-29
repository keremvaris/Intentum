using Intentum.AI.Embeddings;
using Intentum.Core.Behavior;

namespace Intentum.AI.Similarity;

/// <summary>
/// Similarity engine that applies time-based decay to embeddings.
/// More recent events have higher influence on intent inference.
/// This engine requires access to BehaviorSpace to get timestamps.
/// </summary>
public sealed class TimeDecaySimilarityEngine : IIntentSimilarityEngine
{
    private readonly TimeSpan _halfLife;
    private readonly DateTimeOffset _referenceTime;

    /// <summary>
    /// Creates a time decay similarity engine.
    /// </summary>
    /// <param name="halfLife">Time span after which an event's weight is halved. Defaults to 1 hour.</param>
    /// <param name="referenceTime">Reference time for decay calculation. Defaults to UtcNow.</param>
    public TimeDecaySimilarityEngine(
        TimeSpan? halfLife = null,
        DateTimeOffset? referenceTime = null)
    {
        _halfLife = halfLife ?? TimeSpan.FromHours(1);
        _referenceTime = referenceTime ?? DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Calculates intent score with time decay applied.
    /// Note: This overload requires BehaviorSpace to access timestamps.
    /// </summary>
    public double CalculateIntentScoreWithTimeDecay(
        BehaviorSpace behaviorSpace,
        IReadOnlyCollection<IntentEmbedding> embeddings)
    {
        if (embeddings.Count == 0 || behaviorSpace.Events.Count == 0)
            return 0;

        // Create a mapping from source (actor:action) to the most recent timestamp
        var sourceToTimestamp = new Dictionary<string, DateTimeOffset>();
        foreach (var evt in behaviorSpace.Events)
        {
            var source = $"{evt.Actor}:{evt.Action}";
            if (!sourceToTimestamp.TryGetValue(source, out var existing) || evt.OccurredAt > existing)
            {
                sourceToTimestamp[source] = evt.OccurredAt;
            }
        }

        var totalWeightedScore = 0.0;
        var totalWeight = 0.0;

        foreach (var embedding in embeddings)
        {
            if (!sourceToTimestamp.TryGetValue(embedding.Source, out var timestamp))
                continue;

            var age = _referenceTime - timestamp;
            var decayFactor = CalculateDecayFactor(age);

            totalWeightedScore += embedding.Score * decayFactor;
            totalWeight += decayFactor;
        }

        return totalWeight > 0 ? totalWeightedScore / totalWeight : 0;
    }

    /// <summary>
    /// Standard interface implementation - uses simple average (no time decay).
    /// Use CalculateIntentScoreWithTimeDecay for time-based decay.
    /// </summary>
    public double CalculateIntentScore(IReadOnlyCollection<IntentEmbedding> embeddings)
    {
        // Fallback to simple average when timestamps are not available
        if (embeddings.Count == 0)
            return 0;

        return embeddings.Average(e => e.Score);
    }

    private double CalculateDecayFactor(TimeSpan age)
    {
        if (age <= TimeSpan.Zero)
            return 1.0;

        // Exponential decay: weight = 2^(-age/halfLife)
        var halfLifeCount = age.TotalMilliseconds / _halfLife.TotalMilliseconds;
        return Math.Pow(2, -halfLifeCount);
    }
}
