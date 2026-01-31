using System.Diagnostics;
using Intentum.AI.Models;
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
        using var activity = IntentumActivitySource.Source.StartActivity(IntentumActivitySource.InferSpanName);

        try
        {
            var intent = _innerModel.Infer(behaviorSpace, precomputedVector);

            if (activity != null)
            {
                activity.SetTag("intentum.intent.name", intent.Name);
                activity.SetTag("intentum.intent.confidence.level", intent.Confidence.Level);
                activity.SetTag("intentum.intent.signal.count", intent.Signals.Count);
                activity.SetTag("intentum.behavior.event.count", behaviorSpace.Events.Count);
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
}
