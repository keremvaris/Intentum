using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;

namespace Intentum.AI.FewShot;

public sealed class FewShotIntentModel : IIntentModel
{
    private readonly IFewShotStore _store;
    private readonly double _similarityThreshold;

    public FewShotIntentModel(IFewShotStore store, double similarityThreshold = 0.3)
    {
        _store = store;
        _similarityThreshold = similarityThreshold;
    }

    public Intent Infer(BehaviorSpace behaviorSpace, BehaviorVector? precomputedVector = null)
    {
        var keys = behaviorSpace.Events
            .Select(e => e.Action)
            .Distinct()
            .ToArray();

        var matches = _store.FindSimilar(keys, topK: 1);

        if (matches.Count == 0)
            return new Intent("Unknown", [], new IntentConfidence(0, "Low"));

        var best = matches[0];
        var overlap = best.BehaviorKeys.Intersect(keys).Count();
        var maxPossible = Math.Max(best.BehaviorKeys.Count, keys.Length);
        var similarity = maxPossible > 0 ? (double)overlap / maxPossible : 0;

        var score = similarity * best.Confidence;

        if (score < _similarityThreshold)
            return new Intent("Unknown", [], new IntentConfidence(score, "Low"));

        return new Intent(best.IntentName, [], new IntentConfidence(score,
            IntentConfidence.FromScore(score).Level));
    }
}
