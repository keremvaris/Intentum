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
        ["economic:cost_of_goods"] = 0.15,
        ["economic:operational_expenses"] = 0.14,
        ["economic:revenue_at_risk"] = 0.18,
        ["economic:capital_expenditure"] = 0.13,
        ["economic:impact"] = 0.05
    };

    public static readonly Dictionary<string, string> SignalLabels = new(StringComparer.OrdinalIgnoreCase)
    {
        ["physical:heatwave"] = "Sıcak Dalgası",
        ["physical:drought"] = "Kuraklık",
        ["physical:sea_level"] = "Deniz Seviyesi",
        ["physical:storm"] = "Fırtına",
        ["physical:flood"] = "Sel",
        ["physical:water_stress"] = "Su Stresi",
        ["transition:policy"] = "Politika/Regülasyon",
        ["transition:technology"] = "Teknoloji Dönüşümü",
        ["transition:market"] = "Piyasa Riski",
        ["transition:reputation"] = "İtibar Riski",
        ["economic:cost_of_goods"] = "Maliyet Riski",
        ["economic:operational_expenses"] = "Operasyonel Giderler",
        ["economic:revenue_at_risk"] = "Gelir Kaybı",
        ["economic:capital_expenditure"] = "Yatırım Riski",
        ["economic:impact"] = "Ekonomik Etki"
    };

    public static readonly Dictionary<string, string> SignalCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        ["physical:heatwave"] = "Fiziksel",
        ["physical:drought"] = "Fiziksel",
        ["physical:sea_level"] = "Fiziksel",
        ["physical:storm"] = "Fiziksel",
        ["physical:flood"] = "Fiziksel",
        ["physical:water_stress"] = "Fiziksel",
        ["transition:policy"] = "Geçiş",
        ["transition:technology"] = "Geçiş",
        ["transition:market"] = "Geçiş",
        ["transition:reputation"] = "Geçiş",
        ["economic:cost_of_goods"] = "Finansal",
        ["economic:operational_expenses"] = "Finansal",
        ["economic:revenue_at_risk"] = "Finansal",
        ["economic:capital_expenditure"] = "Finansal",
        ["economic:impact"] = "Finansal"
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
            var label = SignalLabels.GetValueOrDefault(dim, dim);
            signals.Add(new IntentSignal(dim, label, w));
        }

        var score = Math.Min(1.0, totalWeight / 2.8);
        var confidence = IntentConfidence.FromScore(score);
        var name = GetIntentNameFromScore(score);
        var reasoning = $"{behaviorSpace.Events.Count} sinyal; ağırlıklı skor {totalWeight:F2} → {name} (güven {score:F2})";

        return new Intent(Name: name, Signals: signals, Confidence: confidence, Reasoning: reasoning);
    }

    private static string GetIntentNameFromScore(double score) => score switch
    {
        >= 0.80 => "Kritik İklim Riski",
        >= 0.60 => "Yüksek İklim Riski",
        >= 0.40 => "Orta İklim Riski",
        >= 0.22 => "Düşük İklim Riski",
        _ => "Minimal İklim Riski"
    };
}
