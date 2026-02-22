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

    private static readonly Counter<long> EmbeddingCallCount = Meter.CreateCounter<long>(
        "intentum.embedding.call.count",
        description: "Number of embedding API calls");

    private static readonly Histogram<double> EmbeddingCallDuration = Meter.CreateHistogram<double>(
        "intentum.embedding.call.duration",
        unit: "ms",
        description: "Duration of embedding API calls");

    private static readonly Counter<long> EmbeddingCacheHitCount = Meter.CreateCounter<long>(
        "intentum.embedding.cache.hit",
        description: "Number of embedding cache hits");

    private static readonly Counter<long> EmbeddingCacheMissCount = Meter.CreateCounter<long>(
        "intentum.embedding.cache.miss",
        description: "Number of embedding cache misses");

    private static readonly Counter<long> LlmClassificationCount = Meter.CreateCounter<long>(
        "intentum.llm.classification.count",
        description: "Number of LLM classification calls");

    private static readonly Histogram<double> LlmClassificationDuration = Meter.CreateHistogram<double>(
        "intentum.llm.classification.duration",
        unit: "ms",
        description: "Duration of LLM classification calls");

    /// <summary>
    /// Records an embedding API call.
    /// </summary>
    public static void RecordEmbeddingCall(string provider, TimeSpan duration, bool success)
    {
        var tags = new TagList { { "provider", provider }, { "success", success } };
        EmbeddingCallCount.Add(1, tags);
        EmbeddingCallDuration.Record(duration.TotalMilliseconds, tags);
    }

    /// <summary>
    /// Records an embedding cache hit or miss.
    /// </summary>
    public static void RecordEmbeddingCacheAccess(bool hit)
    {
        if (hit) EmbeddingCacheHitCount.Add(1);
        else EmbeddingCacheMissCount.Add(1);
    }

    /// <summary>
    /// Records an LLM classification call.
    /// </summary>
    public static void RecordLlmClassification(string model, TimeSpan duration, bool success)
    {
        var tags = new TagList { { "model", model }, { "success", success } };
        LlmClassificationCount.Add(1, tags);
        LlmClassificationDuration.Record(duration.TotalMilliseconds, tags);
    }
}
