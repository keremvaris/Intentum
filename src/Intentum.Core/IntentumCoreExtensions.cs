using Intentum.Core.Behavior;
using Intentum.Core.Evaluation;

namespace Intentum.Core;

public static class IntentumCoreExtensions
{
    public static BehaviorSpace Observe(
        this BehaviorSpace space,
        string actor,
        string action)
    {
        space.Observe(new BehaviorEvent(
            actor,
            action,
            DateTimeOffset.UtcNow));

        return space;
    }

    public static IntentEvaluationResult EvaluateIntent(
        this BehaviorSpace space,
        string intentName)
    {
        var evaluator = new IntentEvaluator();
        return evaluator.Evaluate(intentName, space);
    }
}
