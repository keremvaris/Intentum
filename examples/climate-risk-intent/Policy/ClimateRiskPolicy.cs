using Intentum.Core.Intents;
using Intentum.Runtime.Policy;

namespace Intentum.Example.ClimateRisk.Policy;

public static class ClimateRiskPolicy
{
    public static IntentPolicy Create() => new IntentPolicyBuilder()
        .Escalate("CriticalRisk", i => i is { Name: "CriticalClimateRisk", Confidence.Score: >= 0.7 })
        .Warn("ElevatedRisk", i => i is { Name: "ElevatedClimateRisk", Confidence.Score: >= 0.5 })
        .Observe("ModerateRisk", i => i is { Name: "ModerateClimateRisk", Confidence.Score: >= 0.3 })
        .Allow("LowRisk", i => i is { Name: "LowClimateRisk", Confidence.Score: >= 0.1 })
        .Allow("MinimalRisk", _ => true)
        .Build();
}
