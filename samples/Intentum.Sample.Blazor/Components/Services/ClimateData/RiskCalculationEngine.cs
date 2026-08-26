namespace Intentum.Sample.Blazor.Components.Services.ClimateData;

using System.Linq;
using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Core.Intents;
using Intentum.Runtime.Engine;

public class RiskCalculationEngine
{
    private readonly OpenMeteoService _openMeteo;
    private readonly WriAqueductService _wri;
    private readonly ClimateMonitorService _climateMonitor;
    private readonly FinancialImpactEngine? _financialImpactEngine;
    private readonly CompanyProfileService? _companyProfileService;

    public RiskCalculationEngine(
        OpenMeteoService openMeteo,
        WriAqueductService wri,
        ClimateMonitorService climateMonitor)
        : this(openMeteo, wri, climateMonitor, null, null)
    {
    }

    public RiskCalculationEngine(
        OpenMeteoService openMeteo,
        WriAqueductService wri,
        ClimateMonitorService climateMonitor,
        FinancialImpactEngine? financialImpactEngine,
        CompanyProfileService? companyProfileService)
    {
        _openMeteo = openMeteo;
        _wri = wri;
        _climateMonitor = climateMonitor;
        _financialImpactEngine = financialImpactEngine;
        _companyProfileService = companyProfileService;
    }

    public virtual async Task<RiskAssessment> AssessAsync(
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
        CompanyProfile? companyProfile = null;
        if (_companyProfileService != null && !string.IsNullOrEmpty(input.CompanyProfileId))
        {
            companyProfile = _companyProfileService.GetById(input.CompanyProfileId);
        }

        var space = BuildBehaviorSpace(input, physicalScore, transitionScore, wriRisk, effectiveSea, coastal, companyProfile);
        var model = new ClimateRiskIntentModel();
        var intent = model.Infer(space);
        var policy = ClimateRiskPolicy.Create();
        var policyDecision = IntentPolicyEngine.Evaluate(intent, policy);
        var decision = ClimateRiskPolicy.MapToDecision(policyDecision.ToString());

        // Calculate financial impact
        FinancialImpact? financialImpact = null;
        if (_financialImpactEngine != null && companyProfile != null)
        {
            financialImpact = _financialImpactEngine.Calculate(companyProfile, physicalScore, transitionScore, intent.Signals.Select(s => s.Source).ToList());
        }

        // Skor-policy tutarlılığı: çok yüksek skor intent ALLOW verse bile REVIEW/REJECT'e çek
        var overall = (physicalScore * 0.6 + transitionScore * 0.4);
        if (overall > 0.68 && decision == "ALLOW") decision = "REVIEW";
        if (overall > 0.78 && decision == "REVIEW") decision = "REJECT";

        var reasons = BuildDecisionReasons(input, physicalScore, transitionScore, wriRisk, projection, coastal, effectiveSea, intent, financialImpact);
        var actions = BuildRecommendedActions(decision, input, physicalScore, transitionScore, coastal, wriRisk, financialImpact);
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
            FinancialImpact = financialImpact,
            WaterStress = wriRisk?.WaterStress ?? 0,
            WaterStressLabel = wriRisk?.WaterStressLabel ?? "Veri Yok",
            Projection = projection,
            Baseline = baseline,
            RiskFactors = BuildRiskFactors(projection, wriRisk, input, effectiveSea, coastal)
        };
    }

    private static BehaviorSpace BuildBehaviorSpace(
        RiskInput input, double physical, double transition, WriCountryRisk? wri, double effectiveSea, (bool isCoastal, double distanceKm, string note) coastal, CompanyProfile? companyProfile = null)
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
                var parts = dim.Split(':');
                var actor = parts[0];
                var sub = parts.Length > 1 ? parts[1] : dim;
                space.Observe(actor, sub);
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

        // Financial signals from company profile
        if (companyProfile != null)
        {
            var costOfGoods = companyProfile.Categories
                .Where(c => c.Type == FinancialCategoryType.Capex)
                .SelectMany(c => c.LineItems)
                .Sum(li => li.Value);
            space.Observe("economic", $"cost_of_goods:{costOfGoods}");

            var opex = companyProfile.Categories
                .Where(c => c.Type == FinancialCategoryType.Opex)
                .SelectMany(c => c.LineItems)
                .Sum(li => li.Value);
            space.Observe("economic", $"operational_expenses:{opex}");

            var revenue = companyProfile.Categories
                .Where(c => c.Type == FinancialCategoryType.Revenue)
                .SelectMany(c => c.LineItems)
                .Sum(li => li.Value);
            space.Observe("economic", $"revenue_at_risk:{revenue}");

            var capex = companyProfile.Categories
                .Where(c => c.Type == FinancialCategoryType.Capex)
                .SelectMany(c => c.LineItems)
                .Sum(li => li.Value);
            space.Observe("economic", $"capital_expenditure:{capex}");
        }

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
        (bool isCoastal, double distanceKm, string note) coastal, double effectiveSea, Intent intent, FinancialImpact? financialImpact = null)
    {
        var reasons = new List<string>();

        // Intentum niyet analizi
        reasons.Add($"Intentum niyeti: {intent.Name} — {intent.Reasoning}");

        // Sıcaklık
        if (input.TempAnomaly >= 3.0)
            reasons.Add($"+{input.TempAnomaly:F1}°C sıcaklık artışı: aşırı sıcak günlerde üretim sürekliliği tehdit altında, soğutma maliyeti yükselir");
        else if (input.TempAnomaly >= 2.0)
            reasons.Add($"+{input.TempAnomaly:F1}°C sıcaklık artışı: yaz aylarında verim kaybı olasılığı, izleme önerilir");

        // Yağış
        if (input.PrecipChange <= -30)
            reasons.Add($"Yağış %'{input.PrecipChange:F0} azaldı: kuraklık riski yüksek, alternatif su kaynağı planlaması gerekir");
        else if (input.PrecipChange <= -15)
            reasons.Add($"Yağış %'{input.PrecipChange:F0} azaldı: su kaynakları baskı altında, su verimliliği artırılmalı");

        // Coğrafi-duyarlı deniz mantığı
        if (!coastal.isCoastal)
        {
            reasons.Add($"Deniz seviyesi riski yok: {input.LocationName} {coastal.note}");
            reasons.Add($"Konum yarıçapı {input.RadiusKm}km: denize ~{coastal.distanceKm:F0}km, doğrudan kıyı etkisi hariç");
        }
        else if (effectiveSea >= 1.0)
            reasons.Add($"Kritik deniz seviyesi: +{effectiveSea:F1}m efektif → kıyı tesisleri su altında kalma riski");
        else if (effectiveSea >= 0.4)
            reasons.Add($"Orta deniz seviyesi: +{effectiveSea:F1}m efektif → kıyı bölgelerinde taşkın riski");

        // Su stresi
        if (wri != null && wri.WaterStress >= 4.0)
            reasons.Add($"Kritik su stresi ({wri.WaterStress:F1}/5): {wri.WaterStressLabel} → operasyonel süreklilik ciddi risk altında");
        else if (wri != null && wri.WaterStress >= 2.5)
            reasons.Add($"Yüksek su stresi ({wri.WaterStress:F1}/5): {wri.WaterStressLabel} → su kısıtı beklentisi");

        // Karbon fiyatı
        if (input.CarbonPrice >= 150)
            reasons.Add($"Karbon fiyatı €{input.CarbonPrice}/tCO₂: yüksek maliyet baskısı, karbon yoğun süreçlerde acil optimizasyon gerekir");
        else if (input.CarbonPrice >= 80)
            reasons.Add($"Karbon fiyatı €{input.CarbonPrice}/tCO₂: orta düzey maliyet, karbon azaltım yol haritası planlanmalı");

        // Senaryo
        if (input.Scenario == "SSP5-8.5")
            reasons.Add("SSP5-8.5: en yüksek emisyon patikası — uzun vadeli fiziksel riskler belirgin");
        else if (input.Scenario == "SSP3-7.0")
            reasons.Add("SSP3-7.0: bölgesel çekişme — emisyon kontrolü zayıf, yüksek risk eğilimi");

        // Sektör
        if (input.Sector == "Enerji")
            reasons.Add("Enerji sektörü: hem fiziksel altyapı hem geçiş regülasyonu riski eşzamanlı");
        else if (input.Sector == "Tarim")
            reasons.Add("Tarım: iklim değişkenliğine doğrudan bağımlılık, verim kaybı riski yüksek");

        // Open-Meteo
        if (proj != null && proj.AvgTempMax > 35)
            reasons.Add($"Open-Meteo projeksiyonu: ort. max {proj.AvgTempMax:F1}°C → aşırı sıcak günlerde duruş riski");

        // En güçlü sinyaller
        var topSignals = intent.Signals.OrderByDescending(s => s.Weight).Take(2).ToList();
        if (topSignals.Count > 0)
            reasons.Add($"En etkili sinyaller: {string.Join(", ", topSignals.Select(s => $"{s.Description} ({s.Weight:F2})"))}");

        if (financialImpact != null)
        {
            var net = financialImpact.NetCashFlowImpact;
            if (net < 0)
            {
                reasons.Add($"Net financial exposure: {net:N0} TL/year ({(net < -10_000_000 ? "critical" : "significant")})");
            }
        }

        return reasons;
    }

    private static List<string> BuildRecommendedActions(
        string decision, RiskInput input, double physical, double transition, (bool isCoastal, double distanceKm, string note) coastal, WriCountryRisk? wri, FinancialImpact? financialImpact = null)
    {
        var actions = new List<string>();
        switch (decision)
        {
            case "REJECT":
                actions.Add("İklim risk azaltım planı hazırlayın: TCFD/TNFD çerçevenizde somut hedefler belirleyin");
                if (physical > 0.6)
                {
                    actions.Add("Fiziksel dayanıklılık: yedek su kaynağı ayırın, soğutma kapasitesini %20 artırın");
                    actions.Add("Sigorta kapsamını genişletin: sel/sıcaklık teminatı ekleyin");
                }
                if (wri != null && wri.WaterStress > 3)
                    actions.Add("Su verimliliği: geri dönüşüm sistemi kurun, alternatif su kaynağı (yağmur suyu) planlayın");
                if (!coastal.isCoastal)
                    actions.Add("Kıyı riski yok: odak noktanız su kıstı ve kuraklık senaryoları olmalı");
                actions.Add("Karbon ayak izi azaltım yol haritası: düşük karbon teknolojisi fizibilitesi çıkarın");
                break;
            case "REVIEW":
                actions.Add("6 ayda bir güncel izleme: WRI su stresi + Open-Meteo sıcaklık verilerini takip edin");
                if (input.CarbonPrice >= 80)
                    actions.Add("Karbon fiyat duyarlılık matrisi: €50/100/180 senaryolarında maliyet analizi");
                if (!coastal.isCoastal && wri != null && wri.WaterStress > 2)
                    actions.Add("Yerel su havzası verilerini kontrol edin: DSİ/konya havzası çapraz kontrol");
                if (coastal.isCoastal)
                    actions.Add("Kıyı taşkını: 0.5m/1.0m senaryolarında tesis kotu kontrolü yapın");
                actions.Add("Sektörel gelişmeleri izleyin: regülasyon değişiklikleri veya piyasa sinyalleri");
                break;
            default:
                actions.Add("Mevcut izleme yeterli: yıllık rapor döngüsünü koruyun");
                actions.Add("Piyasa veya regülasyon değişikliği olursa analizi yenileyin");
                break;
        }

        if (financialImpact != null)
        {
            var net = financialImpact.NetCashFlowImpact;
            if (net < -10_000_000)
            {
                actions.Add("Develop comprehensive financial resilience plan for major climate-related cost exposure");
            }
            else if (net < -5_000_000)
            {
                actions.Add("Implement targeted cost-reduction and revenue-protection measures");
            }
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

public sealed record RiskInput
{
    public string Scenario { get; set; } = "SSP2-4.5";
    public string Sector { get; set; } = "Sanayi";
    public int Horizon { get; set; } = 2050;
    public int RadiusKm { get; set; } = 25;
    public double Latitude { get; set; } = 39.93;
    public double Longitude { get; set; } = 32.86;
    public string LocationName { get; set; } = "";
    public string CountryIso3 { get; set; } = "TUR";
    public double TempAnomaly { get; set; } = 2.4;
    public double PrecipChange { get; set; } = -15;
    public double SeaLevelRise { get; set; } = 0.5;
    public int CarbonPrice { get; set; } = 85;
    public string? CompanyProfileId { get; set; }
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
    public FinancialImpact? FinancialImpact { get; set; } = new();
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
