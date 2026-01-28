using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Runtime;
using Intentum.Runtime.Policy;
using MediatR;

namespace Intentum.Sample.Web.Features.CarbonFootprintCalculation.Commands;

public sealed class CalculateCarbonCommandHandler : IRequestHandler<CalculateCarbonCommand, ICalculateCarbonResponse>
{
    private readonly IIntentModel _intentModel;
    private readonly IntentPolicy _policy;

    public CalculateCarbonCommandHandler(IIntentModel intentModel, IntentPolicy policy)
    {
        _intentModel = intentModel;
        _policy = policy;
    }

    public async Task<ICalculateCarbonResponse> Handle(CalculateCarbonCommand request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var space = new BehaviorSpace()
            .Observe(request.Actor, "calculate_carbon")
            .Observe("system", "report_generated");

        var intent = _intentModel.Infer(space);
        var decision = intent.Decide(_policy);

        return decision switch
        {
            PolicyDecision.Block => new CalculateCarbonError($"Blocked by policy: {intent.Confidence.Level}"),
            PolicyDecision.Warn => new CalculateCarbonOk(Guid.NewGuid(), "Warn", intent.Confidence.Level),
            PolicyDecision.Observe => new CalculateCarbonOk(Guid.NewGuid(), "Observe", intent.Confidence.Level),
            PolicyDecision.Allow => new CalculateCarbonOk(Guid.NewGuid(), "Allow", intent.Confidence.Level),
            _ => new CalculateCarbonError("Unknown decision")
        };
    }
}
