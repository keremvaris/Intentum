using Intentum.Core.Behavior;
using Intentum.Core.Intents;
using Intentum.Runtime.Policy;
using Serilog;

namespace Intentum.Logging;

/// <summary>
/// Structured logging for Intentum operations using Serilog.
/// </summary>
public static class IntentumLogger
{
    /// <summary>
    /// Logs intent inference with structured data.
    /// </summary>
    public static void LogIntentInference(
        ILogger logger,
        BehaviorSpace behaviorSpace,
        Intent intent,
        TimeSpan? duration = null)
    {
        logger.Information(
            "Intent inferred: {IntentName}, Confidence: {ConfidenceLevel} ({ConfidenceScore}), Signals: {SignalCount}, Events: {EventCount}, Duration: {Duration}ms, Reasoning: {Reasoning}",
            intent.Name,
            intent.Confidence.Level,
            intent.Confidence.Score,
            intent.Signals.Count,
            behaviorSpace.Events.Count,
            duration?.TotalMilliseconds ?? 0,
            intent.Reasoning ?? "(none)");
    }

    /// <summary>
    /// Logs policy decision with structured data.
    /// </summary>
    public static void LogPolicyDecision(
        ILogger logger,
        Intent intent,
        IntentPolicy policy,
        PolicyDecision decision)
    {
        logger.Information(
            "Policy decision: {Decision}, Intent: {IntentName}, Confidence: {ConfidenceLevel}, Rules: {RuleCount}, Reasoning: {Reasoning}",
            decision,
            intent.Name,
            intent.Confidence.Level,
            policy.Rules.Count,
            intent.Reasoning ?? "(none)");
    }

    /// <summary>
    /// Logs behavior space observation with structured data.
    /// </summary>
    public static void LogBehaviorObservation(
        ILogger logger,
        BehaviorSpace behaviorSpace,
        BehaviorEvent behaviorEvent)
    {
        logger.Debug(
            "Behavior observed: {Actor}:{Action}, Timestamp: {Timestamp}, SpaceSize: {SpaceSize}",
            behaviorEvent.Actor,
            behaviorEvent.Action,
            behaviorEvent.OccurredAt,
            behaviorSpace.Events.Count);
    }

    /// <summary>
    /// Logs behavior space serialization (JSON).
    /// </summary>
    public static void LogBehaviorSpace(
        ILogger logger,
        BehaviorSpace behaviorSpace,
        LogLevel level = LogLevel.Information)
    {
        var json = SerializeBehaviorSpace(behaviorSpace);
        
        switch (level)
        {
            case LogLevel.Debug:
                logger.Debug("Behavior space: {BehaviorSpaceJson}", json);
                break;
            case LogLevel.Warning:
                logger.Warning("Behavior space: {BehaviorSpaceJson}", json);
                break;
            case LogLevel.Error:
                logger.Error("Behavior space: {BehaviorSpaceJson}", json);
                break;
            default:
                logger.Information("Behavior space: {BehaviorSpaceJson}", json);
                break;
        }
    }

    private static string SerializeBehaviorSpace(BehaviorSpace space)
    {
        var events = space.Events.Select(e => new
        {
            e.Actor,
            e.Action,
            e.OccurredAt,
            e.Metadata
        }).ToList();

        var data = new
        {
            EventCount = space.Events.Count,
            Events = events,
            Metadata = space.Metadata
        };

        return System.Text.Json.JsonSerializer.Serialize(data);
    }
}

/// <summary>
/// Log level enumeration.
/// </summary>
public enum LogLevel
{
    Debug,
    Information,
    Warning,
    Error
}
