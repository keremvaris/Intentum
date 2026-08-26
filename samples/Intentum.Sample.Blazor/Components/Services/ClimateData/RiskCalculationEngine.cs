namespace Intentum.Sample.Blazor.Components.Services.ClimateData;

using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Core.Intents;
using Intentum.Runtime.Engine;

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
        var projection = await _openMeteo.GetProjectionAsync(
            input.Latitude, input.Longitude,
            model: GetModelForScenario(input.Scenario),
            startDate: $"{input.Horizon}-01-01",
            endDate: $"{input.Horizon}-12-31",
            ct);

        var wriRisk = await _wri.GetCountryRiskAsync(input.CountryIso3, ct);
        var baseline = await _climateMonitor.GetBaselineTrendsAsync(ct);

        var coastal = GeoRiskHelper.GetCoastalInfo(input.Latitude, input.Longitude, input.LocationName);
        var effectiveSea = GeoRiskHelper.SeaLevelEffective(input.SeaLevelRise, coastal.isCoastal, coastal.distanceKm);

        var physicalScore = CalculatePhysicalRisk(projection, wriRisk, input, effectiveSea);
        var transitionScore = CalculateTransitionRisk(input, baseline);
        var economicImpact = CalculateEconomicImpact(physicalScore, transitionScore, input);

        // Gerçek Intentum: BehaviorSpace → IntentModel → Policy
        var space = BuildBehaviorSpace(input, physicalScore, transitionScore, wriRisk, effectiveSea, coastal);
        var model = new ClimateRiskIntentModel();
        var intent = model.Infer(space);
        var policy = ClimateRiskPolicy.Create();
        var policyDecision = IntentPolicyEngine.Evaluate(intent, policy);
        var decision = ClimateRiskPolicy.MapToDecision(policyDecision.ToString());

        // Skor-policy tutarlılığı: çok yüksek skor intent ALLOW verse bile REVIEW/REJECT'e çek
        var overall = (physicalScore * 0.6 + transitionScore * 0.4);
        if (overall > 0.68 && decision == "ALLOW") decision = "REVIEW";
        if (overall > 0.78 && decision == "REVIEW") decision = "REJECT";

        var reasons = BuildDecisionReasons(input, physicalScore, transitionScore, wriRisk, projection, coastal, effectiveSea, intent);
        var actions = BuildRecommendedActions(decision, input, physicalScore, transitionScore, coastal);
        var summary = BuildDecisionSummary(decision, overall, physicalScore, transitionScore, input, intent, coastal);

        space.Observe("ClimateRisk:result", $"{intent.Name} {policyDecision} → {decision} ({intent.Confidence.Score:F2})");

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
            IntentName = intent.Name,
            ConfidenceScore = intent.Confidence.Score,
            ConfidenceLevel = intent.Confidence.Level,
            IntentReasoning = intent.Reasoning ?? "",
            Signals = intent.Signals.ToList(),
            CoastalInfo = coastal.note,
            IsCoastal = coastal.isCoastal,
            EffectiveSeaLevel = effectiveSea,
            EconomicImpact = economicImpact,
            WaterStress = wriRisk?.WaterStress ?? 0,
            WaterStressLabel = wriRisk?.WaterStressLabel ?? "Veri Yok",
            Projection = projection,
            Baseline = baseline,
            RiskFactors = BuildRiskFactors(projection, wriRisk, input, effectiveSea, coastal)
        };
    }

    private static BehaviorSpace BuildBehaviorSpace(
        RiskInput input, double physical, double transition, WriCountryRisk? wri, double effectiveSea, (bool isCoastal, double distanceKm, string note) coastal)
    {
        var space = new BehaviorSpace();
        space.SetMetadata("location", input.LocationName);
        space.SetMetadata("lat", input.Latitude);
        space.SetMetadata("lng", input.Longitude);
        space.SetMetadata("scenario", input.Scenario);
        space.SetMetadata("sector", input.Sector);
        space.SetMetadata("coastal", coastal.isCoastal);
        space.SetMetadata("coastalNote", coastal.note);

        void Add(string dim, double score)
        {
            var n = (int)Math.Ceiling(Math.Clamp(score, 0, 1) * 5);
            for (var i = 0; i < n; i++)
            {
                var actor = dim.Split(':')[0];
                space.Observe(actor, dim);
            }
        }

        Add("physical:heatwave", Math.Clamp(input.TempAnomaly / 5.0, 0, 1));
        Add("physical:drought", Math.Clamp(Math.Abs(input.PrecipChange) / 40.0, 0, 1));
        Add("physical:sea_level", Math.Clamp(effectiveSea / 1.5, 0, 1));
        Add("physical:storm", physical > 0.6 ? 0.7 : physical > 0.35 ? 0.4 : 0.15);
        Add("physical:water_stress", wri != null ? Math.Clamp(wri.WaterStress / 5.0, 0, 1) : 0.25);
        Add("physical:flood", wri != null && wri.WaterStress > 3 ? 0.5 : 0.15);
        Add("transition:policy", Math.Clamp(input.CarbonPrice / 180.0, 0, 1));
        Add("transition:technology", input.Sector == "Enerji" ? 0.7 : input.Sector == "Sanayi" ? 0.55 : 0.3);
        Add("transition:market", input.Scenario == "SSP5-8.5" ? 0.85 : input.Scenario == "SSP3-7.0" ? 0.6 : 0.35);
        Add("transition:reputation", input.Scenario == "SSP1-2.6" ? 0.15 : 0.4);
        Add("economic:impact", Math.Clamp((physical * 0.6 + transition * 0.4), 0, 1));

        return space;
    }

    private double CalculatePhysicalRisk(ClimateProjection? projection, WriCountryRisk? wri, RiskInput input, double effectiveSea)
    {
        double score = 0;

        var tempScore = Math.Clamp(input.TempAnomaly / 6.0, 0, 1);
        if (projection != null && projection.AvgTempMax > 0)
        {
            var apiTemp = Math.Clamp((projection.AvgTempMax - 30) / 10.0, 0, 1);
            tempScore = tempScore * 0.6 + apiTemp * 0.4;
        }
        score += tempScore * 0.25;

        var precipScore = Math.Clamp(Math.Abs(input.PrecipChange) / 50.0, 0, 1);
        if (projection != null)
        {
            var apiPrecip = Math.Clamp(Math.Abs(projection.AvgPrecipitation - 2.0) / 5.0, 0, 1);
            precipScore = precipScore * 0.6 + apiPrecip * 0.4;
        }
        score += precipScore * 0.2;

        if (projection != null && projection.WindMax.Length > 0)
        {
            var avgWind = projection.WindMax.Average();
            score += Math.Clamp(avgWind / 50.0, 0, 1) * 0.15;
        }
        else
        {
            var scenarioWind = input.Scenario switch
            {
                "SSP5-8.5" => 0.6,
                "SSP3-7.0" => 0.45,
                "SSP2-4.5" => 0.3,
                _ => 0.2
            };
            score += scenarioWind * 0.15;
        }

        // Coğrafi-duyarlı deniz seviyesi: iç bölgede 0
        var seaScore = Math.Clamp(effectiveSea / 2.0, 0, 1);
        score += seaScore * 0.15;

        if (wri != null && wri.WaterStress > 0)
            score += (wri.WaterStress / 5.0) * 0.15;
        else
            score += Math.Clamp(Math.Abs(input.PrecipChange) / 100.0, 0, 1) * 0.15;

        if (wri != null && wri.FloodRisk > 0)
            score += (wri.FloodRisk / 5.0) * 0.1;

        return Math.Clamp(score, 0, 1);
    }

    private double CalculateTransitionRisk(RiskInput input, ClimateBaselineTrends baseline)
    {
        double score = 0;
        score += Math.Clamp(input.CarbonPrice / 200.0, 0, 1) * 0.35;
        score += input.Scenario switch
        {
            "SSP1-2.6" => 0.2,
            "SSP2-4.5" => 0.35,
            "SSP3-7.0" => 0.5,
            "SSP5-8.5" => 0.65,
            _ => 0.35
        };
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
        if (baseline.co2?.current_value > 420) score += 0.05;
        if (baseline.temperature_anomaly?.current_value > 1.2) score += 0.05;
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

    private List<RiskFactor> BuildRiskFactors(ClimateProjection? proj, WriCountryRisk? wri, RiskInput input, double effectiveSea, (bool isCoastal, double distanceKm, string note) coastal)
    {
        var factors = new List<RiskFactor>();
        if (proj != null)
        {
            factors.Add(new RiskFactor("Sıcaklık Artışı", Math.Clamp(proj.AvgTempMax / 50.0, 0, 1), "open-meteo"));
            factors.Add(new RiskFactor("Yağış Değişimi", Math.Clamp(Math.Abs(proj.AvgPrecipitation - 2) / 8.0, 0, 1), "open-meteo"));
            factors.Add(new RiskFactor("Maks. Rüzgar", Math.Clamp(proj.WindMax.DefaultIfEmpty(0).Average() / 50.0, 0, 1), "open-meteo"));
        }
        factors.Add(new RiskFactor("Sıcaklık (+2.4°C)", Math.Clamp(input.TempAnomaly / 5.0, 0, 1), "slider"));
        factors.Add(new RiskFactor("Yağış (-15%)", Math.Clamp(Math.Abs(input.PrecipChange)/50.0,0,1), "slider"));
        // Deniz faktörü coğrafi notla
        var seaLabel = coastal.isCoastal ? $"Deniz (+{effectiveSea:F1}m)" : $"Deniz (iç bölge)";
        factors.Add(new RiskFactor(seaLabel, Math.Clamp(effectiveSea/2.0,0,1), coastal.isCoastal ? "coastal" : "inland"));

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
        WriCountryRisk? wri, ClimateProjection? proj,
        (bool isCoastal, double distanceKm, string note) coastal, double effectiveSea, Intent intent)
    {
        var reasons = new List<string>();

        // Intent kaynaklı ilk satır — Intentum gerçek gücü
        reasons.Add($"Intentum niyeti: {intent.Name} (güven {intent.Confidence.Score:F2} {intent.Confidence.Level}) — {intent.Reasoning}");

        if (input.TempAnomaly >= 3.0)
            reasons.Add($"Yüksek sıcaklık artışı: +{input.TempAnomaly:F1}°C → fiziksel riski önemli ölçüde artırır");
        else if (input.TempAnomaly >= 2.0)
            reasons.Add($"Orta düzey sıcaklık artışı: +{input.TempAnomaly:F1}°C → fiziksel riskte ılımlı etki");

        if (input.PrecipChange <= -30)
            reasons.Add($"Ciddi yağış azalması: %{input.PrecipChange:F0} → kuraklık riski yüksek");
        else if (input.PrecipChange <= -15)
            reasons.Add($"Yağış azalması: %{input.PrecipChange:F0} → su kaynakları baskı altında");

        // Coğrafi-duyarlı deniz mantığı
        if (!coastal.isCoastal)
        {
            reasons.Add($"Deniz seviyesi (+{input.SeaLevelRise:F1}m) slider'da görünse de {input.LocationName} {coastal.note.ToLowerInvariant()} — fiziksel skora katkısı 0");
            reasons.Add($"Fabrika yarıçapı {input.RadiusKm}km, denize mesafe ~{coastal.distanceKm:F0}km → doğrudan kıyı taşkını yok, dolaylı tedarik zinciri riski izlenebilir");
        }
        else if (effectiveSea >= 1.0)
            reasons.Add($"Deniz seviyesi yükselişi: +{effectiveSea:F1}m (efektif) → kıyı tesisleri için yüksek risk ({coastal.note})");
        else if (effectiveSea >= 0.4)
            reasons.Add($"Deniz seviyesi yükselişi: +{effectiveSea:F1}m (efektif) → kıyı bölgelerinde orta düzey risk");

        if (wri != null && wri.WaterStress >= 4.0)
            reasons.Add($"Kritik su stresi: {wri.WaterStressLabel} ({wri.WaterStress:F1}/5) → operasyonel süreklilik tehdit altında");
        else if (wri != null && wri.WaterStress >= 2.5)
            reasons.Add($"Yüksek su stresi: {wri.WaterStressLabel} ({wri.WaterStress:F1}/5) → su kısıtı, WRI Aqueduct");

        if (input.CarbonPrice >= 150)
            reasons.Add($"Yüksek karbon fiyatı: €{input.CarbonPrice}/tCO₂ → geçiş maliyetleri belirgin");
        else if (input.CarbonPrice >= 80)
            reasons.Add($"Orta düzey karbon fiyatı: €{input.CarbonPrice}/tCO₂ → karbon yoğun sektörlerde maliyet artışı");

        if (input.Scenario == "SSP5-8.5")
            reasons.Add("Fosil yakıt senaryosu (SSP5-8.5) → en yüksek emisyon patikası");
        else if (input.Scenario == "SSP3-7.0")
            reasons.Add("Bölgesel çekişme (SSP3-7.0) → yüksek emisyon eğilimi");

        if (input.Sector == "Enerji")
            reasons.Add("Enerji sektörü: fiziksel (altyapı) ve geçiş (regülasyon) riski eşzamanlı yüksek");
        else if (input.Sector == "Tarim")
            reasons.Add("Tarım: iklim değişkenliğine yüksek duyarlılık");

        if (proj != null && proj.AvgTempMax > 35)
            reasons.Add($"Open-Meteo projeksiyonu: ort. max {proj.AvgTempMax:F1}°C → aşırı sıcak günler");

        // En güçlü sinyaller
        var topSignals = intent.Signals.OrderByDescending(s => s.Weight).Take(2).ToList();
        if (topSignals.Count > 0)
            reasons.Add($"En güçlü sinyaller: {string.Join(", ", topSignals.Select(s => $"{s.Description} ({s.Weight:F2})"))}");

        return reasons;
    }

    private static List<string> BuildRecommendedActions(
        string decision, RiskInput input, double physical, double transition, (bool isCoastal, double distanceKm, string note) coastal)
    {
        var actions = new List<string>();
        switch (decision)
        {
            case "REJECT":
                actions.Add("Yatırım komitesi öncesi TCFD/TNFD uyumlu iklim risk azaltım planı hazırlayın");
                actions.Add("Fiziksel dayanıklılık: yedek su kaynağı, ısı stresi için soğutma, altyapı güçlendirme");
                actions.Add("Geçiş: karbon ayak izi azaltım yol haritası + düşük karbon teknolojisi fizibilitesi");
                actions.Add("Sigorta kapsamını genişletin ve prim senaryolarını güncelleyin");
                if (physical > 0.6) actions.Add("Senaryo analizi (fiziksel felaket + piyasa şoku) raporu");
                if (!coastal.isCoastal) actions.Add("Deniz riski yok — odak: su verimliliği ve İç Anadolu kuraklık senaryoları");
                break;
            case "REVIEW":
                actions.Add("CDP/TNFD çerçevesinde detaylı durum tespiti (6 ayda bir güncelle)");
                actions.Add("Fiziksel göstergeleri 6 aylık periyotta izle (WRI + Open-Meteo)");
                actions.Add("Karbon fiyat duyarlılık matrisi oluştur (€50/100/180)");
                if (!coastal.isCoastal) actions.Add("Yerel su havzası ve tarımsal kuraklık — DSİ/konya havzası verileriyle çapraz kontrol");
                else actions.Add("Kıyı taşkını için 0.5m/1.0m senaryolarında fabrika kotu kontrolü");
                break;
            default:
                actions.Add("Mevcut izleme yeterli — yıllık rapor döngüsünü koruyun");
                actions.Add("Piyasa/regülasyon değişirse analizi yenileyin");
                break;
        }
        return actions;
    }

    private static string BuildDecisionSummary(
        string decision, double overall, double physical, double transition, RiskInput input, Intent intent, (bool isCoastal, double distanceKm, string note) coastal)
    {
        var physLabel = physical > 0.7 ? "yüksek" : physical > 0.4 ? "orta" : "düşük";
        var transLabel = transition > 0.7 ? "yüksek" : transition > 0.4 ? "orta" : "düşük";
        var coastalNote = coastal.isCoastal ? "" : $" {coastal.note}";
        var intentInfo = $"Intentum: {intent.Name} ({intent.Confidence.Level} {intent.Confidence.Score:F2}) — {intent.Reasoning}.";

        return decision switch
        {
            "REJECT" => $"Genel %{overall * 100:F0} ile RED (fiziksel %{physical * 100:F0} {physLabel}, geçiş %{transition * 100:F0} {transLabel}). {intentInfo}{coastalNote}",
            "REVIEW" => $"Genel %{overall * 100:F0} ile İNCELEME (fiziksel %{physical * 100:F0} {physLabel}, geçiş %{transition * 100:F0} {transLabel}). {intentInfo}{coastalNote}",
            _ => $"Genel %{overall * 100:F0} kabul edilebilir (fiziksel %{physical * 100:F0} {physLabel}, geçiş %{transition * 100:F0} {transLabel}). {intentInfo}{coastalNote}"
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
    public string IntentName { get; set; } = "";
    public double ConfidenceScore { get; set; }
    public string ConfidenceLevel { get; set; } = "";
    public string IntentReasoning { get; set; } = "";
    public List<IntentSignal> Signals { get; set; } = [];
    public string CoastalInfo { get; set; } = "";
    public bool IsCoastal { get; set; }
    public double EffectiveSeaLevel { get; set; }
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
