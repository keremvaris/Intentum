using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;

namespace Intentum.AI.Mock;

public sealed class MockIntentModel : IIntentModel
{
    public Intent Infer(BehaviorSpace behaviorSpace, BehaviorVector? precomputedVector = null)
    {
        var events = behaviorSpace.Events;
        var name = events.Count > 0
            ? $"{events.Last().Actor}:{events.Last().Action}"
            : "unknown";
        var confidence = IntentConfidence.FromScore(0.5);
        return new Intent(name, [], confidence);
    }
}
