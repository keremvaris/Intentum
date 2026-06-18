using Intentum.Core.Intents;

namespace Intentum.AI.Ensemble;

public sealed class WeightedEnsemble : IEnsembleStrategy
{
    public Intent Combine(IReadOnlyList<ModelResult> results)
    {
        if (results.Count == 0)
            return new Intent("Unknown", [], new IntentConfidence(0, "Low"));

        var totalWeight = results.Sum(r => r.Weight);
        var weightedScore = results.Sum(r => r.Score * r.Weight) / totalWeight;

        var majorityName = results
            .GroupBy(r => r.Name)
            .OrderByDescending(g => g.Sum(r => r.Weight))
            .ThenByDescending(g => g.Average(r => r.Score))
            .First()
            .Key;

        var level = IntentConfidence.FromScore(weightedScore).Level;
        return new Intent(majorityName, [], new IntentConfidence(weightedScore, level));
    }
}
