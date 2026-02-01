using Intentum.Core.Behavior;
using Intentum.Core.Intents;
using Intentum.Runtime.Policy;
using Serilog;

namespace Intentum.Logging;

/// <summary>
/// Extension methods for Intentum logging.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Logs intent inference with structured data.
    /// </summary>
    public static Intent LogIntentInference(
        this Intent intent,
        ILogger logger,
        BehaviorSpace behaviorSpace,
        TimeSpan? duration = null)
    {
        IntentumLogger.LogIntentInference(logger, behaviorSpace, intent, duration);
        return intent;
    }

    /// <summary>
    /// Logs policy decision with structured data.
    /// </summary>
    public static PolicyDecision LogPolicyDecision(
        this PolicyDecision decision,
        ILogger logger,
        Intent intent,
        IntentPolicy policy)
    {
        IntentumLogger.LogPolicyDecision(logger, intent, policy, decision);
        return decision;
    }

    /// <summary>
    /// Logs behavior space with structured data.
    /// </summary>
    public static BehaviorSpace LogBehaviorSpace(
        this BehaviorSpace space,
        ILogger logger,
        LogLevel level = LogLevel.Information)
    {
        IntentumLogger.LogBehaviorSpace(logger, space, level);
        return space;
    }
}
