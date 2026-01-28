using System.Diagnostics;
using Intentum.AI.Models;
using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;

namespace Intentum.Observability;

/// <summary>
/// Wrapper around IIntentModel that adds observability metrics.
/// </summary>
public sealed class ObservableIntentModel : IIntentModel
{
    private readonly IIntentModel _innerModel;

    public ObservableIntentModel(IIntentModel innerModel)
    {
        _innerModel = innerModel ?? throw new ArgumentNullException(nameof(innerModel));
    }

    public Intent Infer(BehaviorSpace behaviorSpace)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var intent = _innerModel.Infer(behaviorSpace);
            
            IntentumMetrics.RecordIntentInference(intent, stopwatch.Elapsed);
            IntentumMetrics.RecordBehaviorSpaceSize(behaviorSpace);
            
            return intent;
        }
        finally
        {
            stopwatch.Stop();
        }
    }
}
