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
        // Open-Meteo climate modelleri yaklaşık 2050'ye kadar veri sağlar.
        // Horizon 2050'yi aşarsa en yakın kullanılabilir yıla (2050) sınırlayıp
        // projeksiyonu mevcut veriyle alır, böylece 400 Bad Request oluşmaz.
        var projectionYear = Math.Min(input.Horizon, 2050);
        var projection = await _openMeteo.GetProjectionAsync(
            input.Latitude, input.Longitude,
            model: GetModelForScenario(input.Scenario),
            startDate: $"{projectionYear}-01-01",
            endDate: $"{projectionYear}-12-31",
            ct);

        var wriRisk = await _wri.GetCountryRiskAsync(input.CountryIso3, input.Scenario, input.Horizon, ct);
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

        // Veri yeterlilik: kaynak mevcudiyetini hesapla (signal:missing için).
        var dataResult = DataSufficiency.Evaluate(
            hasProjection: projection != null && projection.TempMax.Length > 0,
            hasWri: wriRisk != null,
            hasCompanyProfile: companyProfile != null && companyProfile.Categories.Count > 0);

        var space = BuildBehaviorSpace(input, physicalScore, transitionScore, wriRisk, effectiveSea, coastal, companyProfile, dataResult);
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

        // Gerçek veri değerlerinden sinyal ağırlıklarını hesapla — statik sözlük yerine dinamik.
        var computedWeights = ComputeSignalWeights(physicalScore, transitionScore, wriRisk, effectiveSea, companyProfile, financialImpact, dataResult);
        intent = intent with
        {
            Signals = intent.Signals.Select(s =>
            {
                if (computedWeights.TryGetValue(s.Source, out var computed))
                    return s with { Weight = computed };
                return s;
            }).ToList()
        };

        // Ağırlıklar gerçek veriden hesaplandı — confidence ve niyeti yeniden hesapla.
        var activeSignals = intent.Signals.Where(s => s.Weight > 0).ToList();
        var recomputedTotal = activeSignals.Sum(s => s.Weight);
        var activeCount = Math.Max(activeSignals.Count, 1);
        var recomputedAvg = recomputedTotal / activeCount;
        var recomputedScore = Math.Min(1.0, recomputedAvg);
        var recomputedConfidence = IntentConfidence.FromScore(recomputedScore);
        var recomputedName = recomputedScore switch
        {
            >= 0.80 => "Kritik İklim Riski",
            >= 0.60 => "Yüksek İklim Riski",
            >= 0.40 => "Orta İklim Riski",
            >= 0.20 => "Düşük İklim Riski",
            _ => "Minimal İklim Riski"
        };
        var recomputedReasoning = $"{activeSignals.Count} aktif sinyal; sinyal başına güç {recomputedAvg:F2} → {recomputedName} (güven {recomputedScore:F2})";
        intent = intent with
        {
            Name = recomputedName,
            Confidence = recomputedConfidence,
            Reasoning = recomputedReasoning
        };

        // Tek tutarlı karar: Intentum niyeti ağırlıklı, risk skoru güvenlik ağı.
        // Veri yetersizse REVIEW'e çekilir; aşırı yüksek risk skoru REJECT'i zorlar.
        var overall = (physicalScore * 0.6 + transitionScore * 0.4);
        decision = DetermineDecision(overall, intent.Name, financialImpact, dataResult.Score, dataResult.IsRegionalEstimate);

        var reasons = BuildDecisionReasons(input, physicalScore, transitionScore, wriRisk, projection, coastal, effectiveSea, intent, financialImpact, companyProfile, dataResult);
        var region = ClimateRegionCatalog.Get(input.CountryIso3);
        var actions = BuildRecommendedActions(decision, input, physicalScore, transitionScore, coastal, wriRisk, financialImpact, companyProfile, region);
        var summary = BuildDecisionSummary(decision, overall, physicalScore, transitionScore, input, intent, coastal, financialImpact, companyProfile);

        space.Observe("ClimateRisk:result", $"{intent.Name} {policyDecision} → {decision} ({intent.Confidence.Score:F2})");

        var assessment = new RiskAssessment
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

        // Bölgesel bağlam + veri yeterlilik katmanları.
        assessment.ClimateRegion = ClimateRegionCatalog.Get(input.CountryIso3);
        assessment.DataConfidence = dataResult.Score;
        assessment.MissingDataSources = dataResult.Missing;
        assessment.IsRegionalEstimate = dataResult.IsRegionalEstimate;

        // Sistem geneli etki analizi.
        assessment.SystemImpact = SystemImpactModel.Calculate(
            decision, physicalScore, transitionScore,
            wriRisk?.WaterStress ?? 0, coastal, wriRisk,
            financialImpact, companyProfile, input.Sector,
            region, region?.DominantHazards ?? []);

        return assessment;
    }

    private static BehaviorSpace BuildBehaviorSpace(
        RiskInput input, double physical, double transition, WriCountryRisk? wri, double effectiveSea, (bool isCoastal, double distanceKm, string note) coastal, CompanyProfile? companyProfile = null, DataConfidenceResult? dataResult = null)
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

        // Financial signals from company profile — maruziyet, risk skorlarıyla birleşir.
        // Şirket büyüklüğü tek başına değil; büyüklük × (fiziksel/geçiş riski) sinyali üretir.
        if (companyProfile != null)
        {
            var revenue = companyProfile.Categories
                .Where(c => c.Type == FinancialCategoryType.Revenue)
                .SelectMany(c => c.LineItems)
                .Sum(li => li.Value);
            var opex = companyProfile.Categories
                .Where(c => c.Type == FinancialCategoryType.Opex)
                .SelectMany(c => c.LineItems)
                .Sum(li => li.Value);
            var capex = companyProfile.Categories
                .Where(c => c.Type == FinancialCategoryType.Capex)
                .SelectMany(c => c.LineItems)
                .Sum(li => li.Value);

            // Maruz kalan finansal değer: büyüklük × risk. Ne kadar risk, o kadar maruziyet.
            var revenueExposure = Math.Clamp((revenue / 200_000_000.0) * transition, 0, 1);   // gelir geçiş riskine maruz
            var opexExposure = Math.Clamp((opex / 100_000_000.0) * physical, 0, 1);            // operasyon fiziksel riske maruz
            var capexExposure = Math.Clamp((capex / 80_000_000.0) * physical, 0, 1);           // yatırım fiziksel riske maruz

            var revenueObs = (int)Math.Ceiling(revenueExposure * 5);
            var opexObs = (int)Math.Ceiling(opexExposure * 5);
            var capexObs = (int)Math.Ceiling(capexExposure * 5);

            for (var i = 0; i < revenueObs; i++) space.Observe("economic", "revenue_at_risk");
            for (var i = 0; i < opexObs; i++) space.Observe("economic", "operational_expenses");
            for (var i = 0; i < capexObs; i++) space.Observe("economic", "capital_expenditure");
            for (var i = 0; i < capexObs; i++) space.Observe("economic", "cost_of_goods");
        }

        // Veri yeterlilik sinyalleri: eksik veri ve bölgesel tahmin, niyet kararını etkiler.
        if (dataResult != null)
        {
            if (dataResult.Missing.Count > 0)
            {
                var n = Math.Min(dataResult.Missing.Count, 4);
                for (var i = 0; i < n; i++) space.Observe("signal", "missing_data");
            }
            if (dataResult.IsRegionalEstimate)
                space.Observe("signal", "regional_estimate");
        }

        return space;
    }

    private double CalculatePhysicalRisk(ClimateProjection? projection, WriCountryRisk? wri, RiskInput input, double effectiveSea)
    {
        double score = 0;

        // Sıcaklık anomalisi: kullanıcı slider'ı birincil, API projeksiyonu ikincil.
        var tempScore = Math.Clamp(input.TempAnomaly / 6.0, 0, 1);
        if (projection != null && projection.AvgTempMax > 0)
        {
            var apiTemp = Math.Clamp((projection.AvgTempMax - 30) / 10.0, 0, 1);
            tempScore = Math.Max(tempScore, apiTemp);
        }
        score += tempScore * 0.30;

        // Yağış değişimi (kuraklık/sel): mutlak değişim büyüdükçe risk artar.
        var precipScore = Math.Clamp(Math.Abs(input.PrecipChange) / 50.0, 0, 1);
        if (projection != null)
        {
            var apiPrecip = Math.Clamp(Math.Abs(projection.AvgPrecipitation - 2.0) / 5.0, 0, 1);
            precipScore = Math.Max(precipScore, apiPrecip);
        }
        score += precipScore * 0.20;

        // Fırtına: senaryo ve/veya API rüzgar verisine bağlı.
        if (projection != null && projection.WindMax.Length > 0)
        {
            var avgWind = projection.WindMax.Average();
            score += Math.Clamp(avgWind / 50.0, 0, 1) * 0.15;
        }
        else
        {
            var scenarioWind = input.Scenario switch
            {
                "SSP5-8.5" => 0.85,
                "SSP3-7.0" => 0.65,
                "SSP2-4.5" => 0.45,
                _ => 0.25
            };
            score += scenarioWind * 0.15;
        }

        // Coğrafi-duyarlı deniz seviyesi: iç bölgede 0, kıyıda yükselir.
        var seaScore = Math.Clamp(effectiveSea / 2.0, 0, 1);
        score += seaScore * 0.15;

        // Su stresi ve sel: WRI verisine dayanır.
        if (wri != null && wri.WaterStress > 0)
            score += (wri.WaterStress / 5.0) * 0.12;
        else
            score += Math.Clamp(Math.Abs(input.PrecipChange) / 100.0, 0, 1) * 0.12;

        if (wri != null && wri.FloodRisk > 0)
            score += (wri.FloodRisk / 5.0) * 0.08;

        // Deniz seviyesi + kıyı yoksa bu kısım fiziksel riski düşürür; ama sliderlar
        // maksimumda genel risk yine REJECT eşiğini geçmeli. Toplam ağırlık 1.0'dır.
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

    // Monoton karar türetme: yüksek risk her zaman daha katı karar üretir.
    // Veri yetersizse REVIEW'e çekilir (düşük veriyle REJECT/ALLOW riskli).
    // Finansal kayıp büyükse bir kademe sıkılaştırılır. Aşırı yüksek risk skoru REJECT'i zorlar.
    internal static string DetermineDecision(double overall, string intentName, FinancialImpact? financialImpact = null, double dataConfidence = 1.0, bool isRegionalEstimate = false)
    {
        string decision;
        if (overall >= 0.80) decision = "REJECT";                                           // güvenlik ağı: aşırı risk her zaman REJECT
        else if (overall > 0.60) decision = "REVIEW";
        else decision = "ALLOW";

        // Veri yeterlilik: eksik veri kararı REVIEW'e çeker (ALLOW/REJECT'i baskılar).
        if (dataConfidence < 0.75 && decision == "ALLOW") decision = "REVIEW";
        if (isRegionalEstimate && decision == "ALLOW") decision = "REVIEW";

        // Destekleyici finansal etki: net nakit akışı kaybı büyükse kararı bir kademe sıkılaştır.
        if (financialImpact != null)
        {
            var net = financialImpact.NetCashFlowImpact;
            if (net <= -10_000_000 && decision == "ALLOW") decision = "REVIEW";          // 10M+ kayıp: ALLOW → REVIEW
            if (net <= -25_000_000 && decision == "REVIEW") decision = "REJECT";         // 25M+ kayıp: REVIEW → REJECT
        }

        return decision;
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
        factors.Add(new RiskFactor("Sıcaklık Artışı", Math.Clamp(input.TempAnomaly / 5.0, 0, 1), "slider"));
        factors.Add(new RiskFactor("Yağış Değişimi", Math.Clamp(Math.Abs(input.PrecipChange)/50.0,0,1), "slider"));
        // Deniz faktörü coğrafi notla — RiskMatrixEngine tehlikesiyle aynı ad.
        factors.Add(new RiskFactor("Deniz Seviyesi", Math.Clamp(effectiveSea/2.0,0,1), coastal.isCoastal ? "coastal" : "inland"));

        factors.Add(new RiskFactor("Su Stresi", wri != null ? wri.WaterStress / 5.0 : Math.Clamp(Math.Abs(input.PrecipChange)/100.0,0,1), "wri-aqueduct"));
        factors.Add(new RiskFactor("Sel Riski", wri != null ? wri.FloodRisk / 5.0 : 0, "wri-aqueduct"));
        factors.Add(new RiskFactor("Kuraklık Riski", wri != null ? wri.DroughtRisk / 5.0 : 0, "wri-aqueduct"));
        return factors;
    }

    /// <summary>Gerçek veri değerlerinden sinyal ağırlıklarını hesaplar — statik sözlük yerine dinamik.</summary>
    private static Dictionary<string, double> ComputeSignalWeights(
        double physical, double transition, WriCountryRisk? wri, double effectiveSea,
        CompanyProfile? companyProfile, FinancialImpact? financialImpact, DataConfidenceResult? dataResult)
    {
        var weights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        var waterStress = wri?.WaterStress ?? 0;
        var floodRisk = wri?.FloodRisk ?? 0;
        var droughtRisk = wri?.DroughtRisk ?? 0;

        // Fiziksel: her sinyal kendi verisinden türetilir
        weights["physical:water_stress"] = waterStress / 5.0;
        weights["physical:flood"] = floodRisk / 5.0;
        weights["physical:drought"] = droughtRisk / 5.0;
        weights["physical:heatwave"] = physical * 0.9;  // physical skorundan türet
        weights["physical:storm"] = physical * 0.7;     // physical skorundan türet
        weights["physical:sea_level"] = Math.Clamp(effectiveSea / 2.0, 0, 1);

        // Geçiş: physical/transition skorlarından + karbon fiyatından
        weights["transition:market"] = transition * 0.9;
        weights["transition:technology"] = transition * 0.75;
        weights["transition:policy"] = transition * 0.85;
        weights["transition:reputation"] = transition * 0.6;

        // Finansal: şirket profili + finansal etki
        if (companyProfile != null && financialImpact != null)
        {
            var revenue = companyProfile.TotalRevenue;
            var net = financialImpact.NetCashFlowImpact;
            weights["economic:revenue_at_risk"] = revenue > 0 ? Math.Clamp(Math.Abs(net) / revenue, 0, 1) : 0;
            weights["economic:operational_expenses"] = financialImpact.CategoryImpacts
                .Where(c => c.Type == FinancialCategoryType.Opex)
                .Select(c => c.TotalImpact)
                .DefaultIfEmpty(0).First() / (revenue > 0 ? revenue : 1);
            weights["economic:cost_of_goods"] = financialImpact.CategoryImpacts
                .Where(c => c.Type == FinancialCategoryType.Capex)
                .Select(c => c.TotalImpact)
                .DefaultIfEmpty(0).First() / (revenue > 0 ? revenue : 1);
            weights["economic:capital_expenditure"] = financialImpact.CategoryImpacts
                .Where(c => c.Type == FinancialCategoryType.Capex)
                .Select(c => c.TotalImpact)
                .DefaultIfEmpty(0).First() / (revenue > 0 ? revenue : 1);
            weights["economic:impact"] = physical * 0.5 + transition * 0.5;
        }
        else
        {
            weights["economic:revenue_at_risk"] = 0;
            weights["economic:operational_expenses"] = 0;
            weights["economic:cost_of_goods"] = 0;
            weights["economic:capital_expenditure"] = 0;
            weights["economic:impact"] = physical * 0.5 + transition * 0.5;
        }

        // Veri yeterlilik
        weights["signal:missing_data"] = dataResult != null ? 1.0 - dataResult.Score : 0;
        weights["signal:regional_estimate"] = dataResult?.IsRegionalEstimate == true ? 0.8 : 0;

        // 0-1 aralığında sıkıştır
        foreach (var key in weights.Keys.ToList())
            weights[key] = Math.Clamp(weights[key], 0, 1);

        return weights;
    }

    private static List<string> BuildDecisionReasons(
        RiskInput input, double physical, double transition,
        WriCountryRisk? wri, ClimateProjection? proj,
        (bool isCoastal, double distanceKm, string note) coastal, double effectiveSea, Intent intent, FinancialImpact? financialImpact = null, CompanyProfile? companyProfile = null, DataConfidenceResult? dataResult = null)
    {
        var reasons = new List<string>();

        // Intentum niyet analizi
        reasons.Add($"Intentum niyeti: {intent.Name} — {intent.Reasoning}");

        // Şirket ve sektör bağlamı
        if (companyProfile != null)
            reasons.Add($"Şirket: {companyProfile.Name} ({companyProfile.Sector} sektörü, {companyProfile.LocationName}) — {companyProfile.TotalRevenue:N0} TL/yıl ciro");
        else
            reasons.Add($"Bağlam: {input.Sector} sektörü, {input.LocationName}");

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
                if (net <= -25_000_000)
                    reasons.Add($"Finansal maruziyet kritik: net nakit akışı kaybı {net:N0} TL/yıl — karar REJECT'e sıkılaştırıldı");
                else if (net <= -10_000_000)
                    reasons.Add($"Finansal maruziyet yüksek: net nakit akışı kaybı {net:N0} TL/yıl — karar REVIEW'e sıkılaştırıldı");
                else if (net < 0)
                    reasons.Add($"Finansal maruziyet: net nakit akışı kaybı {net:N0} TL/yıl");
            }

            // Veri yeterlilik gerekçesi.
            if (dataResult != null && dataResult.Missing.Count > 0)
                reasons.Add($"Eksik veri kaynakları: {string.Join(", ", dataResult.Missing)} — veri güvenliği {dataResult.Score:P0}");
            if (dataResult != null && dataResult.IsRegionalEstimate)
                reasons.Add("Yerel veri olmadığı için bölgesel profilden genel tahmin kullanıldı");

            return reasons;
    }

    private static List<string> BuildRecommendedActions(
        string decision, RiskInput input, double physical, double transition, (bool isCoastal, double distanceKm, string note) coastal, WriCountryRisk? wri, FinancialImpact? financialImpact = null, CompanyProfile? companyProfile = null, ClimateRegionProfile? region = null)
    {
        var actions = new List<string>();
        var company = companyProfile != null ? $" ({companyProfile.Name})" : "";
        var location = string.IsNullOrWhiteSpace(input.LocationName) ? "bu bölge" : input.LocationName;
        var waterStress = wri?.WaterStress ?? 0;
        var dominantHazards = region?.DominantHazards ?? [];

        switch (decision)
        {
            case "REJECT":
                actions.Add($"⚠️ {input.Sector} sektörü{company} için {location} bölgesinde derin iklim risk azaltım planı hazırlayın — TCFD/TNFD çerçevesinde somut hedefler belirleyin");

                // Fiziksel risk high → somut yatırım/tedbir
                if (physical > 0.7)
                {
                    actions.Add("🛡️ Kritik fiziksel risk: yedek su kaynağı + soğutma kapasitesi artırımı için derhal bütçe ayırın");
                    if (dominantHazards.Contains("Kuraklık Riski") || dominantHazards.Contains("Su Stresi"))
                        actions.Add("💧 Kuraklık/su stresi baskın: acil su verimliliği programı — geri dönüşüm sistemi + alternatif su kaynağı (geri kazanılmış su/yağmur suyu)");
                    if (dominantHazards.Contains("Taşkın Riski"))
                        actions.Add("🌊 Taşkın baskın: drenaj altyapısı güçlendirme + tesis kotu revizyonu + acil durum tahliye planı");
                    if (dominantHazards.Contains("Sıcaklık Artışı") || dominantHazards.Contains("Isı Stresi"))
                        actions.Add("🌡️ Isı stresi baskın: soğutma altyapısı yatırımı + işçi sağlığı protokolü + yaz operasyon planlaması");
                    actions.Add("🛡️ Sigorta kapsamını genişletin: sel/sıcaklık/kuraklık teminatını mevcut poliçeye ekletin");
                }
                else if (physical > 0.5)
                {
                    actions.Add("🛡️ Orta fiziksel risk: mevcut altyapı dayanıklılık testi yaptırın, kırılgan noktaları belirleyin");
                }

                // Su stresi eşiğine göre su önerileri
                if (waterStress >= 4)
                    actions.Add($"💧 {location}: çok yüksek su stresi ({waterStress:F1}/5) — su ayak izi azaltım zorunlu, geri dönüşüm + alternatif su kaynağı fizibilitesi çıkarın");
                else if (waterStress >= 3)
                    actions.Add($"💧 {location}: yüksek su stresi ({waterStress:F1}/5) — su verimliliği izleme sistemi kurun, DSİ/bölge verileriyle çapraz kontrol edin");

                // Kıyı/iç bölge
                if (coastal.isCoastal)
                    actions.Add("🌊 Kıyı tesisleri: deniz seviyesi yükselmesi + fırtına senaryolarında tesis kotu ve drenaj kontrolü yapın");
                else
                    actions.Add("🏜️ İç bölge: odak noktası kuraklık, su kıstı ve ısı stresi senaryoları");

                // Geçiş riski high → regülasyon/sürdürülebilirlik
                if (transition > 0.6)
                    actions.Add("🌱 Yüksek geçiş riski: karbon ayak izi azaltım yol haritası + düşük karbon teknolojisi fizibilitesi ve geçiş maliyet analizi");

                // Sektörel öneriler
                if (input.Sector == "Enerji")
                    actions.Add("⚡ Enerji sektörü: yenilenebilir kaynak geçiş planı + emisyon yoğunluğu azaltım hedefleri");
                else if (input.Sector == "Tarım")
                    actions.Add("🌾 Tarım: alternatif ürün çeşitlendirme + sulama optimizasyonu + kuraklık dayanıklı tohum seçimi");

                // Finansal tedbir (REJECT'te tek blok)
                if (financialImpact != null && financialImpact.NetCashFlowImpact < 0)
                {
                    var net = financialImpact.NetCashFlowImpact;
                    if (net <= -25_000_000)
                        actions.Add($"💸 Kritik finansal maruziyet ({net:N0} TL/yıl): kapsamlı finansal dayanıklılık planı — sermaye planlaması, sigorta limitlerini yeniden yapılandırın ve likidite riskini yönetin");
                    else
                        actions.Add($"💸 Yüksek finansal maruziyet ({net:N0} TL/yıl): hedefli maliyet düşürme + gelir koruma önlemleri uygulayın, nakit akışı senaryoları oluşturun");
                }
                break;

            case "REVIEW":
                // Tek izleme sıklığı: su stresi ve fiziksel riski birlikte değerlendir, tek öneri üret
                var monitoringFrequency = (waterStress >= 3 || physical > 0.5) ? "3 ayda bir" : "6 ayda bir";
                actions.Add($"📡 {location} için {monitoringFrequency} izleme: WRI su stresi + Open-Meteo sıcaklık verilerini takip edin{(waterStress >= 3 ? $" (su stresi: {waterStress:F1}/5)" : "")}");

                // Kıyı/iç bölge riski
                if (coastal.isCoastal)
                    actions.Add("🌊 Kıyı taşkını: +0.5m/+1.0m senaryolarında tesis kotu ve drenaj kontrolü yapın");
                else if (waterStress > 2)
                    actions.Add($"🏜️ {location} iç bölge: kuraklık ve su kıstı senaryolarını izleyin, ısı stresi için yaz operasyon planlaması yapın");

                // Karbon fiyat duyarlılığı
                if (input.CarbonPrice >= 80)
                    actions.Add("💶 Karbon fiyat duyarlılık matrisi: €50/100/180 senaryolarında maliyet analizi yapın");

                // Bölgesel baskın tehlike bazlı
                if (dominantHazards.Count > 0)
                {
                    var topHazard = dominantHazards[0];
                    actions.Add($"🎯 {location} için öncelikli tehlike: {topHazard} — bu tehlikeye odaklanarak izleme planı oluşturun");
                }

                // Finansal tampon (REVIEW'te tek blok)
                if (financialImpact != null && financialImpact.NetCashFlowImpact < 0)
                {
                    var net = financialImpact.NetCashFlowImpact;
                    if (net <= -25_000_000)
                        actions.Add($"💸 Kritik finansal maruziyet ({net:N0} TL/yıl): yedek fon + sigorta + türev enstrümanlarını değerlendirin");
                    else
                        actions.Add($"💰 Finansal tampon ({net:N0} TL/yıl): beklenen kayıp için yedek fon ayırın, nakit akışı senaryoları oluşturun");
                }
                break;

            default: // ALLOW
                actions.Add($"✅ {company} için mevcut izleme yeterli — yıllık rapor döngüsünü koruyun");
                if (physical > 0.3 || transition > 0.3)
                    actions.Add("📋 Düşük risk bölgesinde bile: iklim etkenlerini yatırım kararlarına dahil etmeye devam edin");
                if (waterStress >= 3)
                    actions.Add($"💧 {location}: su stresi yüksek ({waterStress:F1}/5) — ALLOW kararına rağmen su verimliliği izlemesini sürdürün");
                break;
        }

        // Bölgesel adaptif öneriler: SADECE baskın tehlikelerle eşleşen öneriler
        if (region != null)
        {
            foreach (var rec in region.AdaptiveRecommendations)
            {
                var recLower = rec.ToLowerInvariant();
                var anyHazardMatch = dominantHazards.Any(h => recLower.Contains(h.ToLowerInvariant()));
                if (anyHazardMatch || dominantHazards.Count == 0)
                    actions.Add($"🗺️ {region.Name} ({region.ClimateType}): {rec}");
            }

            var seasonalNote = BuildSeasonalNote(region, coastal, waterStress, physical, dominantHazards);
            if (!string.IsNullOrEmpty(seasonalNote))
                actions.Add($"📅 {seasonalNote}");
        }

        return actions;
    }

    /// <summary>Lokasyona özel mevsimsellik notu üretir — ülke profili + kıyı durumu + su stresi + baskın tehlikeler.</summary>
    private static string BuildSeasonalNote(ClimateRegionProfile region, (bool isCoastal, double distanceKm, string note) coastal, double waterStress, double physical, List<string> dominantHazards)
    {
        var parts = new List<string>();

        // Bölge tipine göre mevsimsel riskler
        if (coastal.isCoastal)
        {
            parts.Add("Kıyı şeridinde kış fırtınası ve ani taşkın riski yüksek");
            if (physical > 0.5)
                parts.Add("yaz sıcağı ve kuraklık operasyonları doğrudan etkiler");
        }
        else
        {
            if (waterStress >= 3)
                parts.Add("yaz kuraklığı ve su kıstı baskın");
            else
                parts.Add("iç bölgelerde yaz kuraklığı ve gece-gündüz sıcaklık farkı yüksek");
            if (dominantHazards.Contains("Sıcaklık Artışı") || dominantHazards.Contains("Isı Stresi"))
                parts.Add("artan sıcak dalgaları işçi sağlığını ve soğutma yükünü etkiler");
        }

        // Ülke/genel not sadece bölgeye özgü bilgi yoksa ekle
        if (parts.Count == 0 && !string.IsNullOrEmpty(region.Seasonality))
            return $"{region.Seasonality} — mevsimsellik dikkate alınmalı";

        return parts.Count > 0 ? string.Join("; ", parts) + " — mevsimsellik dikkate alınmalı" : "";
    }

    private static string BuildDecisionSummary(
        string decision, double overall, double physical, double transition, RiskInput input, Intent intent, (bool isCoastal, double distanceKm, string note) coastal, FinancialImpact? financialImpact = null, CompanyProfile? companyProfile = null)
    {
        var physLabel = physical > 0.7 ? "yüksek" : physical > 0.4 ? "orta" : "düşük";
        var transLabel = transition > 0.7 ? "yüksek" : transition > 0.4 ? "orta" : "düşük";
        var coastalNote = coastal.isCoastal ? "" : $" {coastal.note}";
        var intentInfo = $"Intentum: {intent.Name} ({intent.Confidence.Level} {intent.Confidence.Score:F2}) — {intent.Reasoning}.";
        var entity = companyProfile != null ? companyProfile.Name : input.LocationName;

        // Finansal maruziyet kararın gerekçesini destekler.
        var financialNote = "";
        if (financialImpact != null && financialImpact.NetCashFlowImpact < 0)
            financialNote = $" Finansal maruziyet: net {financialImpact.NetCashFlowImpact:N0} TL/yıl.";

        return decision switch
        {
            "REJECT" => $"{entity} için {input.Scenario} senaryosunda genel %{overall * 100:F0} RED — fiziksel %{physical * 100:F0} ({physLabel}), geçiş %{transition * 100:F0} ({transLabel}). {intentInfo}{financialNote}{coastalNote}",
            "REVIEW" => $"{entity} için {input.Scenario} senaryosunda genel %{overall * 100:F0} İNCELEME — fiziksel %{physical * 100:F0} ({physLabel}), geçiş %{transition * 100:F0} ({transLabel}). {intentInfo}{financialNote}{coastalNote}",
            _ => $"{entity} için {input.Scenario} senaryosunda genel %{overall * 100:F0} kabul edilebilir — fiziksel %{physical * 100:F0} ({physLabel}), geçiş %{transition * 100:F0} ({transLabel}). {intentInfo}{financialNote}{coastalNote}"
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
    // IPCC risk çerçevesi: Tehlike × Maruziyet × Kırılganlık matrix'leri.
    public HazardExposureMatrix? HazardExposureMatrix { get; set; }
    public ScenarioMatrix? ScenarioMatrix { get; set; }
    // Bölgesel bağlam katmanı: ülke bazlı iklim profili.
    public ClimateRegionProfile? ClimateRegion { get; set; }
    // Veri yeterlilik katmanı: 0-1 arası güven + eksik kaynak listesi.
    public double DataConfidence { get; set; } = 1.0;
    public List<string> MissingDataSources { get; set; } = [];
    public bool IsRegionalEstimate { get; set; }
    // Sistem geneli etki analizi.
    public SystemImpactResult? SystemImpact { get; set; }
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
