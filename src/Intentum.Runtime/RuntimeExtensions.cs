using Intentum.Core.Intents;
using Intentum.Runtime.Engine;
using Intentum.Runtime.Policy;

namespace Intentum.Runtime;

public static class RuntimeExtensions
{
    public static PolicyDecision Decide(
        this Intent intent,
        IntentPolicy policy)
    {
        var engine = new IntentPolicyEngine();
        return engine.Evaluate(intent, policy);
    }
}
