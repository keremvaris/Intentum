using Grpc.Core;
using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;
using Intentum.Runtime.Engine;
using Intentum.Runtime.Policy;

namespace Intentum.Grpc.Services;

public sealed class IntentumGrpcService : IntentumService.IntentumServiceBase
{
    private readonly IIntentModel _model;

    public IntentumGrpcService(IIntentModel model)
    {
        _model = model;
    }

    public override Task<InferResponse> Infer(InferRequest request, ServerCallContext context)
    {
        var space = new BehaviorSpace();
        foreach (var evt in request.Events)
        {
            space.Observe(new Intentum.Core.Behavior.BehaviorEvent(
                evt.Actor,
                evt.Action,
                DateTimeOffset.Parse(evt.OccurredAt)));
        }

        var intent = _model.Infer(space);

        return Task.FromResult(new InferResponse
        {
            Name = intent.Name,
            Confidence = new Confidence
            {
                Score = intent.Confidence.Score,
                Level = intent.Confidence.Level
            }
        });
    }

    public override Task<EvaluateResponse> Evaluate(EvaluateRequest request, ServerCallContext context)
    {
        var intent = new Intent(
            request.IntentName,
            [],
            new IntentConfidence(request.ConfidenceScore, request.ConfidenceLevel));

        var policy = new IntentPolicy();
        var decision = IntentPolicyEngine.Evaluate(intent, policy);

        return Task.FromResult(new EvaluateResponse
        {
            Decision = decision.ToString()
        });
    }
}
