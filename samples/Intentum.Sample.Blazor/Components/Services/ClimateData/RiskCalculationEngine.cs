namespace Intentum.Sample.Blazor.Components.Services.ClimateData;

using Intentum.Core;
using Intentum.Core.Behavior;

public sealed class RiskCalculationEngine
{
    private readonly OpenMeteoService _openMeteo;
    private readonly WriAqueductService _wri;
    private readonly ClimateMonitorService _climateMonitor;

    public RiskCalculationEngine(
        OpenMeteoService openMeteo,
        WriAqueductService wri,
        ClimateMonitorService climateMonitor)
    {
        _openMeteo = openMeteo;
        _wri = wri;
        _climateMonitor = climateMonitor;
    }

    public async Task<RiskAssessment> AssessAsync(
        RiskInput input, CancellationToken ct = default)
    {
        var space = new BehaviorSpace();
        space.Observe("ClimateRisk", $"Assess: {input.LocationName} ({input.Latitude},{input.Longitude})");
        space.Observe("ClimateRisk", $"Scenario={input.Scenario}, Horizon={input.Horizon}, Sector={input.Sector}");
        space.Observe("ClimateRisk", $"Sliders: Temp={input.TempAnomaly}°C, Precip={input.PrecipChange}%, SeaLevel={input.SeaLevelRise}m, CarbonPrice=${input.CarbonPrice}/t");

        var projection = await _openMeteo.GetProjectionAsync(
            input.Latitude, input.Longitude,
            model: GetModelForScenario(input.Scenario),
            startDate: $"{input.Horizon}-01-01",
            endDate: $"{input.Horizon}-12-31",
            ct);

        var wriRisk = await _wri.GetCountryRiskAsync(input.CountryIso3, ct);
        var baseline = await _climateMonitor.GetBaselineTrendsAsync(ct);

        var physicalScore = CalculatePhysicalRisk(projection, wriRisk, input);
        var transitionScore = CalculateTransitionRisk(input, baseline);
        var economicImpact = CalculateEconomicImpact(physicalScore, transitionScore, input);

        var overall = (physicalScore * 0.6 + transitionScore * 0.4);
        var decision = overall switch
        {
            > 0.7 => "REJECT",
            > 0.4 => "REVIEW",
            _ => "ALLOW"
        };

        var reasons = BuildDecisionReasons(input, physicalScore, transitionScore, wriRisk, projection);
        var actions = BuildRecommendedActions(decision, input, physicalScore, transitionScore);
        var summary = BuildDecisionSummary(decision, overall, physicalScore, transitionScore, input);

        space.Observe("ClimateRisk", $"Result: Physical={physicalScore:F3}, Transition={transitionScore:F3}, Overall={overall:F3} → {decision}");
        space.Observe("ClimateRisk", $"WaterStress={wriRisk?.WaterStressLabel ?? "N/A"}, EcoImpact={economicImpact.Total:F1}M$");

        return new RiskAssessment
        {
            Input = input,
            PhysicalRisk = physicalScore,
            TransitionRisk = transitionScore,
            OverallRisk = overall,
            Decision = decision,
            DecisionSummary = summary,
            DecisionReasons = reasons,
            RecommendedActions = actions,
            EconomicImpact = economicImpact,
            WaterStress = wriRisk?.WaterStress ?? 0,
            WaterStressLabel = wriRisk?.WaterStressLabel ?? "Veri Yok",
            Projection = projection,
            Baseline = baseline,
            RiskFactors = BuildRiskFactors(projection, wriRisk, input)
        };
    }

    private double CalculatePhysicalRisk(ClimateProjection? projection, WriCountryRisk? wri, RiskInput input)
    {
        double score = 0;

        // Temperature risk: slider value is primary, API data blends in
        var tempScore = Math.Clamp(input.TempAnomaly / 6.0, 0, 1);
        if (projection != null && projection.AvgTempMax > 0)
        {
            var apiTemp = Math.Clamp((projection.AvgTempMax - 30) / 10.0, 0, 1);
            tempScore = tempScore * 0.6 + apiTemp * 0.4; // 60% slider, 40% API
        }
        score += tempScore * 0.25;

        // Precipitation risk: drought from slider
        var precipScore = Math.Clamp(Math.Abs(input.PrecipChange) / 50.0, 0, 1);
        if (projection != null)
        {
            var apiPrecip = Math.Clamp(Math.Abs(projection.AvgPrecipitation - 2.0) / 5.0, 0, 1);
            precipScore = precipScore * 0.6 + apiPrecip * 0.4;
        }
        score += precipScore * 0.2;

        // Wind/storm risk: from API or estimate from scenario
        if (projection != null && projection.WindMax.Length > 0)
        {
            var avgWind = projection.WindMax.Average();
            score += Math.Clamp(avgWind / 50.0, 0, 1) * 0.15;
        }
        else
        {
            // Estimate from scenario severity
            var scenarioWind = input.Scenario switch
            {
                "SSP5-8.5" => 0.6,
                "SSP3-7.0" => 0.45,
                "SSP2-4.5" => 0.3,
                _ => 0.2
            };
            score += scenarioWind * 0.15;
        }

        // Sea level rise risk
        var seaScore = Math.Clamp(input.SeaLevelRise / 2.0, 0, 1);
        score += seaScore * 0.15;

        // Water stress from WRI
        if (wri != null && wri.WaterStress > 0)
        {
            score += (wri.WaterStress / 5.0) * 0.15;
        }
        else
        {
            // Estimate from precipitation change
            score += Math.Clamp(Math.Abs(input.PrecipChange) / 100.0, 0, 1) * 0.15;
        }

        // Flood risk from WRI
        if (wri != null && wri.FloodRisk > 0)
        {
            score += (wri.FloodRisk / 5.0) * 0.1;
        }

        return Math.Clamp(score, 0, 1);
    }

    private double CalculateTransitionRisk(RiskInput input, ClimateBaselineTrends baseline)
    {
        double score = 0;

        // Carbon price directly affects transition risk
        score += Math.Clamp(input.CarbonPrice / 200.0, 0, 1) * 0.35;

        // Scenario base risk
        score += input.Scenario switch
        {
            "SSP1-2.6" => 0.2,
            "SSP2-4.5" => 0.35,
            "SSP3-7.0" => 0.5,
            "SSP5-8.5" => 0.65,
            _ => 0.35
        };

        // Sector-specific adjustment
        var sectorAdj = input.Sector switch
        {
            "Enerji" => 0.15,
            "Tarim" => 0.05,
            "Emlak" => 0.0,
            "Finans" => 0.1,
            "Turizm" => 0.05,
            "Sanayi" => 0.1,
            _ => 0.0
        };
        score += sectorAdj;

        if (baseline.co2?.current_value > 420)
            score += 0.05;

        if (baseline.temperature_anomaly?.current_value > 1.2)
            score += 0.05;

        return Math.Clamp(score, 0, 1);
    }

    private EconomicImpact CalculateEconomicImpact(double physical, double transition, RiskInput input)
    {
        var sectorMultiplier = input.Sector switch
        {
            "Enerji" => 1.2,
            "Tarim" => 1.1,
            "Emlak" => 1.0,
            "Finans" => 0.9,
            "Turizm" => 1.15,
            "Sanayi" => 1.05,
            _ => 1.0
        };

        var baseMdp = 2.5 * sectorMultiplier;
        return new EconomicImpact
        {
            MdpLoss = baseMdp * physical * 1.5,
            CapexIncrease = baseMdp * physical * 0.8,
            InsuranceCost = baseMdp * physical * 0.4 * sectorMultiplier,
            BorrowingCost = baseMdp * transition * 0.3,
            OperationalCost = baseMdp * physical * transition * 0.2
        };
    }

    private List<RiskFactor> BuildRiskFactors(ClimateProjection? proj, WriCountryRisk? wri, RiskInput input)
    {
        var factors = new List<RiskFactor>();

        if (proj != null)
        {
            factors.Add(new RiskFactor("Sıcaklık Artışı", Math.Clamp(proj.AvgTempMax / 50.0, 0, 1), "open-meteo"));
            factors.Add(new RiskFactor("Yağış Değişimi", Math.Clamp(Math.Abs(proj.AvgPrecipitation - 2) / 8.0, 0, 1), "open-meteo"));
            factors.Add(new RiskFactor("Maks. Rüzgar", Math.Clamp(proj.WindMax.DefaultIfEmpty(0).Average() / 50.0, 0, 1), "open-meteo"));
        }

        if (wri != null)
        {
            factors.Add(new RiskFactor("Su Stresi", wri.WaterStress / 5.0, "wri-aqueduct"));
            factors.Add(new RiskFactor("Sel Riski", wri.FloodRisk / 5.0, "wri-aqueduct"));
            factors.Add(new RiskFactor("Kuraklık Riski", wri.DroughtRisk / 5.0, "wri-aqueduct"));
        }

        return factors;
    }

    private static List<string> BuildDecisionReasons(
        RiskInput input, double physical, double transition,
        WriCountryRisk? wri, ClimateProjection? proj)
    {
        var reasons = new List<string>();

        // Physical risk drivers
        if (input.TempAnomaly >= 3.0)
            reasons.Add($"Yüksek sıcaklık artışı: +{input.TempAnomaly:F1}°C (eşik: 3.0°C) → fiziksel riski önemli ölçüde artırır");
        else if (input.TempAnomaly >= 2.0)
            reasons.Add($"Orta düzey sıcaklık artışı: +{input.TempAnomaly:F1}°C → fiziksel risk üzerinde moderat etki");

        if (input.PrecipChange <= -30)
            reasons.Add($"Ciddi yağış azalması: %{input.PrecipChange:F0} → kuraklık ve su kıtlığı riski yüksek");
        else if (input.PrecipChange <= -15)
            reasons.Add($"Yağış azalması: %{input.PrecipChange:F0} → su kaynakları üzerinde baskı");

        if (input.SeaLevelRise >= 1.0)
            reasons.Add($"Deniz seviyesi yükselişi: +{input.SeaLevelRise:F1}m → kıyı tesisleri için yüksek risk");
        else if (input.SeaLevelRise >= 0.5)
            reasons.Add($"Deniz seviyesi yükselişi: +{input.SeaLevelRise:F1}m → kıyı bölgelerinde orta düzey risk");

        // Water stress
        if (wri != null && wri.WaterStress >= 4.0)
            reasons.Add($"Kritik su stresi: {wri.WaterStressLabel} ({wri.WaterStress:F1}/5) → operasyonel süreklilik tehdit altında");
        else if (wri != null && wri.WaterStress >= 2.5)
            reasons.Add($"Yüksek su stresi: {wri.WaterStressLabel} ({wri.WaterStress:F1}/5) → su kaynakları kısıtlı");

        // Transition risk drivers
        if (input.CarbonPrice >= 150)
            reasons.Add($"Yüksek karbon fiyatı: €{input.CarbonPrice}/tCO₂ → geçiş maliyetleri önemli ölçüde artar");
        else if (input.CarbonPrice >= 80)
            reasons.Add($"Orta düzey karbon fiyatı: €{input.CarbonPrice}/tCO₂ → karbon_intensity bağlı maliyet artışı");

        // Scenario impact
        if (input.Scenario == "SSP5-8.5")
            reasons.Add("Fosil yakıt senaryosu (SSP5-8.5) seçildi → en yüksek emisyon ve risk yolculuğu");
        else if (input.Scenario == "SSP3-7.0")
            reasons.Add("Bölgesel çekişme senaryosu (SSP3-7.0) → yüksek emisyon eğilimi");

        // Sector specific
        if (input.Sector == "Enerji")
            reasons.Add("Enerji sektörü: hem fiziksel risk (altyapı) hem geçiş riski (regülasyon) yüksek");
        else if (input.Sector == "Tarim")
            reasons.Add("Tarım sektörü: iklim değişkenliğine yüksek duyarlılık");

        // Projection data
        if (proj != null && proj.AvgTempMax > 35)
            reasons.Add($"Open-Meteo projeksiyonu: ortalama max sıcaklık {proj.AvgTempMax:F1}°C → aşırı sıcaklık olayları");

        if (reasons.Count == 0)
            reasons.Add("Belirgin risk faktörü tespit edilmedi → tüm göstergeler kabul edilebilir aralıkta");

        return reasons;
    }

    private static List<string> BuildRecommendedActions(
        string decision, RiskInput input, double physical, double transition)
    {
        var actions = new List<string>();

        switch (decision)
        {
            case "REJECT":
                actions.Add("Proje investment komitesine sunulmadan önce iklim risk azaltma planı hazırlanmalı");
                actions.Add("Fiziksel risk azaltma: altyapı dayanıklılık artırımı, yedekleme sistemleri");
                actions.Add("Geçiş riski: karbon ayak izi azaltma stratejisi, düşük karbon teknolojilerine geçiş planı");
                actions.Add("Sigorta kapsamı genişletilmeli ve maliyet analizi güncellenmeli");
                if (physical > 0.6)
                    actions.Add("Kapsamlı iklim felaket senaryosu (TCFD) raporu hazırlanmalı");
                break;
            case "REVIEW":
                actions.Add("Detaylı iklim risk değerlendirmesi (CDP/TNFD çerçevesinde) yapılmalı");
                actions.Add("Fiziksel risk göstergeleri 6 aylık periyotlarla izlenmeli");
                actions.Add("Karbon fiyat senaryolarına karşı hassasiyet analizi güncellenmeli");
                actions.Add("Yerel su kaynakları durumu detaylı incelenmeli");
                break;
            default: // ALLOW
                actions.Add("Mevcut risk izleme prosedürleri yeterli");
                actions.Add("Yıllık iklim risk raporlama döngüsü devam etmeli");
                actions.Add("Piyasa koşulları değiştiğinde analiz yenilenmeli");
                break;
        }

        return actions;
    }

    private static string BuildDecisionSummary(
        string decision, double overall, double physical, double transition, RiskInput input)
    {
        var physLabel = physical > 0.7 ? "yüksek" : physical > 0.4 ? "orta" : "düşük";
        var transLabel = transition > 0.7 ? "yüksek" : transition > 0.4 ? "orta" : "düşük";

        return decision switch
        {
            "REJECT" => $"Genel risk skoru %{overall * 100:F0} ile_RED_eşiklerini aşıyor (fiziksel: %{physical * 100:F0} {physLabel}, geçiş: %{transition * 100:F0} {transLabel}). " +
                        $"{input.Sector} sektöründe {input.Scenario} senaryosu ile {input.Horizon} horizonu için " +
                        $"iklim riskleri kabul edilebilir düzeyin üzerinde.",
            "REVIEW" => $"Genel risk skoru %{overall * 100:F0} ile_INCELEME_aralığında (fiziksel: %{physical * 100:F0} {physLabel}, geçiş: %{transition * 100:F0} {transLabel}). " +
                        $"Detaylı değerlendirme ve ek veri toplama gerekiyor.",
            _ => $"Genel risk skoru %{overall * 100:F0} ile Kabul edilebilir aralıkta (fiziksel: %{physical * 100:F0} {physLabel}, geçiş: %{transition * 100:F0} {transLabel}). " +
                 $"Mevcut izleme prosedürleri yeterli."
        };
    }

    private static string GetModelForScenario(string scenario) => scenario switch
    {
        "SSP1-2.6" => "EC_Earth3P_HR",
        "SSP2-4.5" => "EC_Earth3P_HR",
        "SSP3-7.0" => "MPI_ESM1_2_XR",
        "SSP5-8.5" => "MPI_ESM1_2_XR",
        _ => "EC_Earth3P_HR"
    };
}

public sealed class RiskInput
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string LocationName { get; set; } = "";
    public string Scenario { get; set; } = "SSP2-4.5";
    public string Sector { get; set; } = "Enerji";
    public int Horizon { get; set; } = 2050;
    public double RadiusKm { get; set; } = 10;
    public string CountryIso3 { get; set; } = "TUR";
    public double TempAnomaly { get; set; } = 2.4;
    public double PrecipChange { get; set; } = -15;
    public double SeaLevelRise { get; set; } = 0.5;
    public int CarbonPrice { get; set; } = 85;
}

public sealed class RiskAssessment
{
    public RiskInput Input { get; set; } = new();
    public double PhysicalRisk { get; set; }
    public double TransitionRisk { get; set; }
    public double OverallRisk { get; set; }
    public string Decision { get; set; } = "ALLOW";
    public string DecisionSummary { get; set; } = "";
    public List<string> DecisionReasons { get; set; } = [];
    public List<string> RecommendedActions { get; set; } = [];
    public EconomicImpact EconomicImpact { get; set; } = new();
    public double WaterStress { get; set; }
    public string WaterStressLabel { get; set; } = "";
    public ClimateProjection? Projection { get; set; }
    public ClimateBaselineTrends? Baseline { get; set; }
    public List<RiskFactor> RiskFactors { get; set; } = [];
}

public sealed class EconomicImpact
{
    public double MdpLoss { get; set; }
    public double CapexIncrease { get; set; }
    public double InsuranceCost { get; set; }
    public double BorrowingCost { get; set; }
    public double OperationalCost { get; set; }
    public double Total => MdpLoss + CapexIncrease + InsuranceCost + BorrowingCost + OperationalCost;
}

public sealed class RiskFactor(string name, double score, string source)
{
    public string Name { get; } = name;
    public double Score { get; } = score;
    public string Source { get; } = source;
    public string Label => Score switch
    {
        > 0.7 => "Yüksek",
        > 0.4 => "Orta",
        _ => "Düşük"
    };
    public string Color => Score switch
    {
        > 0.7 => "#ef4444",
        > 0.4 => "#f59e0b",
        _ => "#22c55e"
    };
}
