using System.Diagnostics;
using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;

namespace Intentum.Observability;

/// <summary>
/// Wrapper around IIntentModel that adds observability metrics and OpenTelemetry spans.
/// </summary>
public sealed class ObservableIntentModel : IIntentModel
{
    private readonly IIntentModel _innerModel;

    public ObservableIntentModel(IIntentModel innerModel)
    {
        _innerModel = innerModel ?? throw new ArgumentNullException(nameof(innerModel));
    }

    public Intent Infer(BehaviorSpace behaviorSpace, BehaviorVector? precomputedVector = null)
    {
        var stopwatch = Stopwatch.StartNew();
        using var activity = IntentumActivitySource.Source.StartActivity();

        try
        {
            var intent = _innerModel.Infer(behaviorSpace, precomputedVector);

            if (activity != null)
            {
                activity.DisplayName = IntentumActivitySource.InferSpanName;
                activity.SetTag("intentum.intent.name", intent.Name);
                activity.SetTag("intentum.intent.confidence.level", intent.Confidence.Level);
                activity.SetTag("intentum.intent.confidence.score", intent.Confidence.Score);
                activity.SetTag("intentum.intent.signal.count", intent.Signals.Count);
                activity.SetTag("intentum.behavior.event.count", behaviorSpace.Events.Count);
                var signalSummary = GetBehaviorSignalSummary(behaviorSpace);
                if (signalSummary != null)
                    activity.SetTag("intentum.behavior.signal_summary", signalSummary);
            }

            IntentumMetrics.RecordIntentInference(intent, stopwatch.Elapsed);
            IntentumMetrics.RecordBehaviorSpaceSize(behaviorSpace);

            return intent;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    /// <summary>Builds a short summary of behavior signals for trace correlation (signal → intent).</summary>
    private static string? GetBehaviorSignalSummary(BehaviorSpace behaviorSpace)
    {
        if (behaviorSpace.Events.Count == 0)
            return null;
        const int maxLen = 200;
        var parts = behaviorSpace.Events
            .Select(e => $"{e.Actor}:{e.Action}")
            .ToList();
        var s = string.Join(";", parts);
        return s.Length <= maxLen ? s : s[..maxLen] + "...";
    }
}
