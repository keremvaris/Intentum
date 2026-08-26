using Intentum.Runtime.Policy;

namespace Intentum.Sample.Blazor.Components.Services.ClimateData;

public static class ClimateRiskPolicy
{
    public static IntentPolicy Create() => new IntentPolicyBuilder()
        .Escalate("CriticalRisk", i => i is { Name: "Kritik İklim Riski", Confidence.Score: >= 0.70 })
        .Escalate("CriticalRisk", i => i is { Name: "CriticalClimateRisk", Confidence.Score: >= 0.70 })
        .Warn("ElevatedRisk", i => i is { Name: "Yüksek İklim Riski", Confidence.Score: >= 0.50 })
        .Warn("ElevatedRisk", i => i is { Name: "ElevatedClimateRisk", Confidence.Score: >= 0.50 })
        .Observe("ModerateRisk", i => i is { Name: "Orta İklim Riski" })
        .Observe("ModerateRisk", i => i is { Name: "ModerateClimateRisk" })
        .Allow("LowRisk", i => i is { Name: "Düşük İklim Riski" })
        .Allow("LowRisk", i => i is { Name: "LowClimateRisk" })
        .Allow("MinimalRisk", _ => true)
        .Build();

    public static string MapToDecision(string policyDecision) => policyDecision switch
    {
        "Escalate" => "REJECT",
        "Warn" => "REVIEW",
        "Observe" => "REVIEW",
        _ => "ALLOW"
    };
}
