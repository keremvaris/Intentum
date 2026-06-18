using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;

namespace Intentum.Playground.Models;

public class DemoIntentModel : IIntentModel
{
    public Intent Infer(BehaviorSpace behaviorSpace, BehaviorVector? precomputedVector = null)
    {
        var eventCount = behaviorSpace.Events.Count;
        var uniqueActors = behaviorSpace.Events.Select(e => e.Actor).Distinct().Count();

        var intentName = behaviorSpace.Events
            .GroupBy(e => e.Action)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault() ?? "Unknown";

        var score = Math.Min(0.3 + (eventCount * 0.1) + (uniqueActors * 0.1), 0.95);
        var level = score switch
        {
            >= 0.8 => "Certain",
            >= 0.6 => "High",
            >= 0.4 => "Medium",
            _ => "Low"
        };

        return new Intent(intentName, [], new IntentConfidence(score, level));
    }
}
