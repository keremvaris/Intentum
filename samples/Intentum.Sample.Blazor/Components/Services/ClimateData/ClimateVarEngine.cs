using System.Globalization;

namespace Intentum.Sample.Blazor.Components.Services.ClimateData;

public sealed class ClimateVarEngine
{
    private readonly NgfsScenarioService _ngfs;
    private readonly RiskCalculationEngine _riskEngine;

    public ClimateVarEngine(NgfsScenarioService ngfs, RiskCalculationEngine riskEngine)
    {
        _ngfs = ngfs;
        _riskEngine = riskEngine;
    }

    public async Task<ClimateVarResult> CalculateAsync(
        CompanyProfile profile, RiskInput input, IReadOnlyList<string> scenarios,
        CancellationToken ct = default)
    {
        var losses = new List<ScenarioLoss>();

        foreach (var scenarioId in scenarios)
        {
            var ngfsSnapshot = await _ngfs.GetSnapshotAsync(input.CountryIso3, scenarioId, input.Horizon, ct);
            var scenarioInput = input with
            {
                NgfsScenarioId = scenarioId,
                CarbonPrice = ngfsSnapshot?.CarbonPrice is > 0 ? (int)ngfsSnapshot.CarbonPrice : input.CarbonPrice
            };

            var assessment = await _riskEngine.AssessAsync(scenarioInput, ct);
            var scenarioInfo = NgfsScenarios.GetById(scenarioId);

            var totalLoss = 0.0;
            if (assessment.FinancialImpact != null)
            {
                totalLoss = assessment.FinancialImpact.CategoryImpacts
                    .Where(c => c.Type is FinancialCategoryType.Opex or FinancialCategoryType.Capex)
                    .Sum(c => c.TotalImpact);
                totalLoss += assessment.FinancialImpact.TotalRevenueImpact;
            }

            var weight = CalculateWeight(scenarioInfo?.WarmingLevel ?? 2.0, scenarioInfo?.Category ?? "Unknown");

            losses.Add(new ScenarioLoss
            {
                ScenarioName = scenarioInfo?.Name ?? scenarioId,
                ScenarioCategory = scenarioInfo?.Category ?? "Unknown",
                WarmingLevel = scenarioInfo?.WarmingLevel ?? 0,
                Loss = totalLoss,
                PhysicalRisk = assessment.PhysicalRisk,
                TransitionRisk = assessment.TransitionRisk,
                Weight = weight
            });
        }

        if (losses.Count == 0)
            return new ClimateVarResult { CompanyId = profile.Id, CompanyName = profile.Name };

        var sorted = losses.OrderBy(l => l.Loss).ToList();
        int var95Idx = (int)Math.Floor(0.95 * sorted.Count);
        int var99Idx = (int)Math.Floor(0.99 * sorted.Count);
        var95Idx = Math.Clamp(var95Idx, 0, sorted.Count - 1);
        var99Idx = Math.Clamp(var99Idx, 0, sorted.Count - 1);

        var var95 = sorted[var95Idx].Loss;
        var var99 = sorted[var99Idx].Loss;
        var tailLosses = sorted.Where(l => l.Loss >= var95).ToList();
        var cvar95 = tailLosses.Count > 0 ? tailLosses.Average(l => l.Loss) : var95;

        var totalWeight = losses.Sum(l => l.Weight);
        var expectedLoss = totalWeight > 0
            ? losses.Sum(l => l.Loss * l.Weight) / totalWeight
            : losses.Average(l => l.Loss);

        var worst = losses.OrderByDescending(l => l.Loss).First();
        var best = losses.OrderBy(l => l.Loss).First();

        return new ClimateVarResult
        {
            CompanyId = profile.Id,
            CompanyName = profile.Name,
            ExpectedLoss = expectedLoss,
            VaR95 = var95,
            VaR99 = var99,
            CVaR95 = cvar95,
            WorstScenario = worst.ScenarioName,
            WorstLoss = worst.Loss,
            BestScenario = best.ScenarioName,
            BestGain = best.Loss,
            LossDistribution = losses
        };
    }

    public static double CalculateWeight(double warmingLevel, string category)
    {
        var baseWeight = 1.0 / (1.0 + warmingLevel);
        return category switch
        {
            "Orderly" => baseWeight * 1.2,
            "HotHouse" => baseWeight * 0.8,
            _ => baseWeight
        };
    }
}