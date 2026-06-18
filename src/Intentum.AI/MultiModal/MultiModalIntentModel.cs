using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;

namespace Intentum.AI.MultiModal;

public sealed class MultiModalIntentModel : IIntentModel
{
    public Intent Infer(BehaviorSpace behaviorSpace, BehaviorVector? precomputedVector = null)
    {
        var vector = precomputedVector ?? behaviorSpace.ToVector();
        var dims = vector.Dimensions.Values.Select(v => (float)v).ToArray();
        var fused = MultiModalFusion.Fuse(dims, []);

        var score = Math.Min(fused.Average() + 0.3, 1.0);
        return new Intent("MultiModal-Result", [],
            new IntentConfidence(score, IntentConfidence.FromScore(score).Level));
    }
}
