using Intentum.Core.Behavior;
using Intentum.Core.Intents;
using Intentum.Playground.Models;
using Intentum.Runtime.Policy;

namespace Intentum.Playground.Services;

public class PlaygroundService
{
    private readonly BehaviorSpace _space = new();
    private IntentPolicy _policy = new();
    private readonly DemoIntentModel _model = new();

    public BehaviorSpace Space => _space;
    public IntentPolicy Policy => _policy;

    public void AddEvent(BehaviorEvent evt) => _space.Observe(evt);
    public void SetPolicy(IntentPolicy policy) => _policy = policy;

    public Intent Infer() => _model.Infer(_space);

    public PolicyDecision Evaluate(Intent intent)
        => Intentum.Runtime.Engine.IntentPolicyEngine.Evaluate(intent, _policy);
}
