using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;
using Intentum.Example.ClimateRisk.Models;
using Intentum.Example.ClimateRisk.Risks;

namespace Intentum.Example.ClimateRisk.Intents;

public sealed class ClimateRiskIntentModel : IIntentModel
{
    private static readonly Dictionary<string, double> SignalWeights = new(StringComparer.OrdinalIgnoreCase)
    {
        ["physical:flood"] = 0.15,
        ["physical:drought"] = 0.12,
        ["physical:storm"] = 0.13,
        ["physical:sea_level"] = 0.10,
        ["physical:heatwave"] = 0.10,
        ["transition:policy"] = 0.15,
        ["transition:technology"] = 0.12,
        ["transition:market"] = 0.10,
        ["transition:reputation"] = 0.08,
        ["economic:impact"] = 0.05
    };

    public Intent Infer(BehaviorSpace behaviorSpace, BehaviorVector? precomputedVector = null)
    {
        var vector = precomputedVector ?? behaviorSpace.ToVector();
        var totalWeight = 0.0;
        var signalList = new List<IntentSignal>();

        foreach (var (dim, count) in vector.Dimensions)
        {
            var weight = SignalWeights.GetValueOrDefault(dim, 0.05) * Math.Min(count, 5);
            totalWeight += weight;
            signalList.Add(new IntentSignal("climate-risk", dim, weight));
        }

        var score = Math.Min(1.0, totalWeight / 2.5);
        var confidence = IntentConfidence.FromScore(score);
        var name = GetIntentNameFromScore(score);
        var reasoning = $"{behaviorSpace.Events.Count} signals; weighted score {totalWeight:F2} -> {name}";

        return new Intent(Name: name, Signals: signalList, Confidence: confidence, Reasoning: reasoning);
    }

    private static string GetIntentNameFromScore(double score) => score switch
    {
        >= 0.8 => "CriticalClimateRisk",
        >= 0.6 => "ElevatedClimateRisk",
        >= 0.4 => "ModerateClimateRisk",
        >= 0.2 => "LowClimateRisk",
        _ => "MinimalClimateRisk"
    };
}
