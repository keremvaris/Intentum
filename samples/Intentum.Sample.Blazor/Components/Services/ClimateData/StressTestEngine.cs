namespace Intentum.Sample.Blazor.Components.Services.ClimateData;

public sealed class StressTestEngine
{
    private readonly RiskCalculationEngine _riskEngine;
    private readonly FinancialImpactEngine? _financialEngine;

    public StressTestEngine(RiskCalculationEngine riskEngine, FinancialImpactEngine? financialEngine)
    {
        _riskEngine = riskEngine;
        _financialEngine = financialEngine;
    }

    public async Task<StressTestResult> RunAsync(
        RiskInput baselineInput, StressFactors factors,
        CompanyProfile? profile = null, CancellationToken ct = default)
    {
        var baseline = await _riskEngine.AssessAsync(baselineInput, ct);
        var stressedInput = ApplyFactors(baselineInput, factors);
        var stressed = await _riskEngine.AssessAsync(stressedInput, ct);

        var delta = CalculateDelta(baseline, stressed);

        var contributions = await CalculateContributionsAsync(baselineInput, factors, profile, ct);
        var sensitivity = contributions
            .OrderByDescending(c => c.MarginalEffect)
            .Select(c => ClassifySensitivity(c.FactorName, c.MarginalEffect))
            .ToList();

        return new StressTestResult
        {
            BaselineAssessment = baseline,
            StressedAssessment = stressed,
            Delta = delta,
            FactorContributions = contributions,
            SensitivityRanking = sensitivity
        };
    }

    public static RiskInput ApplyFactors(RiskInput input, StressFactors factors)
    {
        return input with
        {
            TempAnomaly = input.TempAnomaly * factors.TemperatureMultiplier,
            PrecipChange = input.PrecipChange * factors.PrecipitationMultiplier,
            SeaLevelRise = input.SeaLevelRise * factors.SeaLevelMultiplier,
            CarbonPrice = (int)(input.CarbonPrice * factors.CarbonPriceMultiplier)
        };
    }

    public static StressDelta CalculateDelta(RiskAssessment baseline, RiskAssessment stressed)
    {
        var physicalDelta = stressed.PhysicalRisk - baseline.PhysicalRisk;
        var transitionDelta = stressed.TransitionRisk - baseline.TransitionRisk;
        var overallDelta = (physicalDelta * 0.6 + transitionDelta * 0.4);

        var decisionChange = baseline.Decision == stressed.Decision
            ? baseline.Decision
            : $"{baseline.Decision}→{stressed.Decision}";

        return new StressDelta
        {
            PhysicalRiskDelta = physicalDelta,
            TransitionRiskDelta = transitionDelta,
            OverallRiskDelta = overallDelta,
            DecisionChange = decisionChange
        };
    }

    public static SensitivityItem ClassifySensitivity(string factorName, double score)
    {
        var normalized = Math.Clamp(score, 0, 1);
        var risk = normalized switch
        {
            >= 0.7 => "Yüksek",
            >= 0.4 => "Orta",
            _ => "Düşük"
        };
        return new SensitivityItem { FactorName = factorName, SensitivityScore = normalized, Risk = risk };
    }

    private async Task<List<FactorContribution>> CalculateContributionsAsync(
        RiskInput baseline, StressFactors factors, CompanyProfile? profile, CancellationToken ct)
    {
        var contributions = new List<FactorContribution>();
        var allFactors = new (string Name, string NameTr, double Mult)[]
        {
            ("Temperature", "Sıcaklık", factors.TemperatureMultiplier),
            ("Precipitation", "Yağış", factors.PrecipitationMultiplier),
            ("SeaLevel", "Deniz Seviyesi", factors.SeaLevelMultiplier),
            ("WaterStress", "Su Stresi", factors.WaterStressMultiplier),
            ("CarbonPrice", "Karbon Fiyatı", factors.CarbonPriceMultiplier),
            ("WindSpeed", "Rüzgar Hızı", factors.WindSpeedMultiplier),
            ("PhysicalRisk", "Fiziksel Risk", factors.PhysicalRiskMultiplier),
            ("TransitionRisk", "Geçiş Riski", factors.TransitionRiskMultiplier)
        };

        var baselineAssessment = await _riskEngine.AssessAsync(baseline, ct);
        var baselineOverall = baselineAssessment.PhysicalRisk * 0.6 + baselineAssessment.TransitionRisk * 0.4;

        double totalDelta = 0;
        foreach (var (name, nameTr, mult) in allFactors.Where(f => Math.Abs(f.Mult - 1.0) > 0.01))
        {
            var stressedInput = ApplyFactors(baseline, new StressFactors
            {
                TemperatureMultiplier = name == "Temperature" ? mult : 1.0,
                PrecipitationMultiplier = name == "Precipitation" ? mult : 1.0,
                SeaLevelMultiplier = name == "SeaLevel" ? mult : 1.0,
                WaterStressMultiplier = name == "WaterStress" ? mult : 1.0,
                CarbonPriceMultiplier = name == "CarbonPrice" ? mult : 1.0,
                WindSpeedMultiplier = name == "WindSpeed" ? mult : 1.0,
                PhysicalRiskMultiplier = name == "PhysicalRisk" ? mult : 1.0,
                TransitionRiskMultiplier = name == "TransitionRisk" ? mult : 1.0
            });

            var stressedAssessment = await _riskEngine.AssessAsync(stressedInput, ct);
            var stressedOverall = stressedAssessment.PhysicalRisk * 0.6 + stressedAssessment.TransitionRisk * 0.4;
            var effect = Math.Abs(stressedOverall - baselineOverall);
            totalDelta += effect;

            contributions.Add(new FactorContribution
            {
                FactorName = name,
                FactorNameTr = nameTr,
                Multiplier = mult,
                MarginalEffect = effect
            });
        }

        foreach (var c in contributions)
            c.ContributionPct = totalDelta > 0 ? (c.MarginalEffect / totalDelta * 100) : 0;

        return contributions;
    }
}
