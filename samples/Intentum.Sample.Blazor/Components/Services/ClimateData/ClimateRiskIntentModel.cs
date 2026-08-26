using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;

namespace Intentum.Sample.Blazor.Components.Services.ClimateData;

/// <summary>Gerçek Intentum niyet modeli — örnek projeden uyarlandı, canlı veriyle çalışır.</summary>
public sealed class ClimateRiskIntentModel : IIntentModel
{
    private static readonly Dictionary<string, double> SignalWeights = new(StringComparer.OrdinalIgnoreCase)
    {
        ["physical:heatwave"] = 0.12,
        ["physical:drought"] = 0.13,
        ["physical:sea_level"] = 0.10,
        ["physical:storm"] = 0.12,
        ["physical:flood"] = 0.11,
        ["physical:water_stress"] = 0.10,
        ["transition:policy"] = 0.14,
        ["transition:technology"] = 0.10,
        ["transition:market"] = 0.09,
        ["transition:reputation"] = 0.06,
        ["economic:impact"] = 0.05
    };

    public Intent Infer(BehaviorSpace behaviorSpace, BehaviorVector? precomputedVector = null)
    {
        var vector = precomputedVector ?? behaviorSpace.ToVector();
        var totalWeight = 0.0;
        var signals = new List<IntentSignal>();

        foreach (var (dim, count) in vector.Dimensions)
        {
            var w = SignalWeights.GetValueOrDefault(dim, 0.05) * Math.Min(count, 5);
            totalWeight += w;
            signals.Add(new IntentSignal("climate-risk", dim, w));
        }

        var score = Math.Min(1.0, totalWeight / 2.8);
        var confidence = IntentConfidence.FromScore(score);
        var name = GetIntentNameFromScore(score);
        var reasoning = $"{behaviorSpace.Events.Count} sinyal; ağırlıklı skor {totalWeight:F2} → {name} (güven {score:F2})";

        return new Intent(Name: name, Signals: signals, Confidence: confidence, Reasoning: reasoning);
    }

    private static string GetIntentNameFromScore(double score) => score switch
    {
        >= 0.80 => "CriticalClimateRisk",
        >= 0.60 => "ElevatedClimateRisk",
        >= 0.40 => "ModerateClimateRisk",
        >= 0.22 => "LowClimateRisk",
        _ => "MinimalClimateRisk"
    };
}
