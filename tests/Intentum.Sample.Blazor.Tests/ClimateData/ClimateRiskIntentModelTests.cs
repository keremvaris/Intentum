using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Sample.Blazor.Components.Services.ClimateData;

namespace Intentum.Sample.Blazor.Tests.ClimateData;

public class ClimateRiskIntentModelTests
{
    private static BehaviorSpace Space(params (string actor, string action, int count)[] dims)
    {
        var space = new BehaviorSpace();
        foreach (var (actor, action, count) in dims)
            for (var i = 0; i < count; i++)
                space.Observe(actor, action);
        return space;
    }

    [Fact]
    public void Infer_ManySignals_DoesNotAlwaysHitMaxConfidence()
    {
        // Tipik bir analiz: 10 farklı sinyal, her biri orta yoğunlukta.
        var space = Space(
            ("physical", "heatwave", 3),
            ("physical", "drought", 2),
            ("physical", "storm", 2),
            ("physical", "water_stress", 4),
            ("physical", "sea_level", 1),
            ("transition", "policy", 3),
            ("transition", "technology", 3),
            ("transition", "market", 4),
            ("economic", "impact", 4),
            ("economic", "revenue_at_risk", 2));

        var model = new ClimateRiskIntentModel();
        var intent = model.Infer(space);

        // Güven 1.00'e takılmamalı ve anlamlı bir aralıkta olmalı (0.2 - 0.99).
        Assert.InRange(intent.Confidence.Score, 0.2, 0.99);
    }

    [Fact]
    public void Infer_LowSignals_LowerConfidenceThanHighSignals()
    {
        var low = Space(("physical", "heatwave", 1), ("transition", "policy", 1));
        var high = Space(("physical", "heatwave", 5), ("transition", "policy", 5), ("physical", "storm", 5));

        var model = new ClimateRiskIntentModel();
        var lowConf = model.Infer(low).Confidence.Score;
        var highConf = model.Infer(high).Confidence.Score;

        Assert.True(highConf > lowConf, $"Yüksek yoğunluk ({highConf}) düşük yoğunluktan ({lowConf}) büyük olmalı.");
    }
}
