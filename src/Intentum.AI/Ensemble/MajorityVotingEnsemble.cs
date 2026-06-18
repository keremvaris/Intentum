using Intentum.Core.Intents;

namespace Intentum.AI.Ensemble;

public sealed class MajorityVotingEnsemble : IEnsembleStrategy
{
    public Intent Combine(IReadOnlyList<ModelResult> results)
    {
        if (results.Count == 0)
            return new Intent("Unknown", [], new IntentConfidence(0, "Low"));

        var winner = results
            .GroupBy(r => r.Name)
            .OrderByDescending(g => g.Count())
            .ThenByDescending(g => g.Average(r => r.Score))
            .First();

        var avgScore = winner.Average(r => r.Score);
        var level = IntentConfidence.FromScore(avgScore).Level;
        return new Intent(winner.Key, [], new IntentConfidence(avgScore, level));
    }
}
