using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;

namespace Intentum.McpServer.McpTools;

public sealed class InferIntentTool
{
    private readonly IIntentModel _model;

    public InferIntentTool(IIntentModel model)
    {
        _model = model;
    }

    public record InferRequest(string Actor, string Action);
    public record InferResponse(string Name, double Score, string Level);

    public InferResponse Execute(IReadOnlyList<InferRequest> events)
    {
        var space = new BehaviorSpace();
        foreach (var evt in events)
            space.Observe(new BehaviorEvent(evt.Actor, evt.Action, DateTimeOffset.UtcNow));

        var intent = _model.Infer(space);
        return new InferResponse(intent.Name, intent.Confidence.Score, intent.Confidence.Level);
    }
}
