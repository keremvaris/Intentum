using System.Diagnostics;
using System.Diagnostics.Metrics;
using Intentum.Core.Behavior;
using Intentum.Core.Intents;
using Intentum.Runtime.Policy;

namespace Intentum.Observability;

/// <summary>
/// OpenTelemetry metrics for Intentum operations.
/// </summary>
public static class IntentumMetrics
{
    private static readonly Meter Meter = new("Intentum", "1.0.0");
    
    private static readonly Counter<long> IntentInferenceCount = Meter.CreateCounter<long>(
        "intentum.intent.inference.count",
        description: "Number of intent inferences performed");

    private static readonly Histogram<double> IntentInferenceDuration = Meter.CreateHistogram<double>(
        "intentum.intent.inference.duration",
        unit: "ms",
        description: "Duration of intent inference operations");

    private static readonly Histogram<double> IntentConfidenceScore = Meter.CreateHistogram<double>(
        "intentum.intent.confidence.score",
        description: "Confidence score of inferred intents");

    private static readonly Counter<long> PolicyDecisionCount = Meter.CreateCounter<long>(
        "intentum.policy.decision.count",
        description: "Number of policy decisions made");

    private static readonly Histogram<int> BehaviorSpaceSize = Meter.CreateHistogram<int>(
        "intentum.behavior.space.size",
        description: "Size of behavior spaces");

    /// <summary>
    /// Records an intent inference operation.
    /// </summary>
    public static void RecordIntentInference(Intent intent, TimeSpan duration)
    {
        var tags = new TagList
        {
            { "confidence.level", intent.Confidence.Level },
            { "signal.count", intent.Signals.Count }
        };

        IntentInferenceCount.Add(1, tags);
        IntentInferenceDuration.Record(duration.TotalMilliseconds, tags);
        IntentConfidenceScore.Record(intent.Confidence.Score, tags);
    }

    /// <summary>
    /// Records a policy decision.
    /// </summary>
    public static void RecordPolicyDecision(PolicyDecision decision)
    {
        var tags = new TagList
        {
            { "decision", decision.ToString() }
        };

        PolicyDecisionCount.Add(1, tags);
    }

    /// <summary>
    /// Records behavior space size.
    /// </summary>
    public static void RecordBehaviorSpaceSize(BehaviorSpace space)
    {
        var tags = new TagList
        {
            { "event.count", space.Events.Count }
        };

        BehaviorSpaceSize.Record(space.Events.Count, tags);
    }
}
