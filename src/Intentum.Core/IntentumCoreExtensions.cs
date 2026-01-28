using Intentum.Core.Behavior;
using Intentum.Core.Evaluation;

namespace Intentum.Core;

/// <summary>Extension methods for <see cref="BehaviorSpace"/> and intent evaluation.</summary>
public static class IntentumCoreExtensions
{
    /// <summary>Records an observed event (actor and action) and returns the space for chaining.</summary>
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

    /// <summary>Evaluates intent from the behavior space using the default evaluator.</summary>
    public static IntentEvaluationResult EvaluateIntent(
        this BehaviorSpace space,
        string intentName)
    {
        var evaluator = new IntentEvaluator();
        return evaluator.Evaluate(intentName, space);
    }
}
