using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;

namespace Intentum.Sample.Blazor.Features.GreenwashingDetection;

/// <summary>
/// Sürdürülebilirlik raporu sinyallerinden niyet çıkarır (GenuineSustainability → ActiveGreenwashing).
/// </summary>
public sealed class GreenwashingIntentModel : IIntentModel
{
    private static readonly Dictionary<string, double> SignalWeights = new(StringComparer.OrdinalIgnoreCase)
    {
        ["language:claim.vague"] = 0.15,
        ["language:comparison.unsubstantiated"] = 0.25,
        ["data:metrics.without.proof"] = 0.35,
        ["data:baseline.manipulation"] = 0.45,
        ["imagery:nature.without.data"] = 0.2
    };

    private static string GetIntentNameFromScore(double score)
    {
        if (score >= 0.8) return "ActiveGreenwashing";
        if (score >= 0.6) return "StrategicObfuscation";
        if (score >= 0.4) return "SelectiveDisclosure";
        if (score >= 0.2) return "UnintentionalMisrepresentation";
        return "GenuineSustainability";
    }

    public Intent Infer(BehaviorSpace behaviorSpace, BehaviorVector? precomputedVector = null)
    {
        var vector = precomputedVector ?? behaviorSpace.ToVector();
        var totalWeight = 0.0;
        var signalList = new List<IntentSignal>();

        foreach (var (dim, count) in vector.Dimensions)
        {
            var weight = SignalWeights.GetValueOrDefault(dim, 0.1) * Math.Min(count, 5);
            totalWeight += weight;
            signalList.Add(new IntentSignal("greenwashing", dim, weight));
        }

        var score = Math.Min(1.0, totalWeight / 3.0);
        var confidence = IntentConfidence.FromScore(score);
        var name = GetIntentNameFromScore(score);
        var reasoning = $"{behaviorSpace.Events.Count} sinyal; ağırlıklı skor {totalWeight:F2} → {name}";

        return new Intent(Name: name, Signals: signalList, Confidence: confidence, Reasoning: reasoning);
    }
}
