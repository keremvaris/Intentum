using System.Globalization;

namespace Intentum.Sample.Blazor.Components.Services.ClimateData;

public sealed class PortfolioRiskEngine
{
    private readonly RiskCalculationEngine _riskEngine;
    private readonly FinancialImpactEngine? _financialEngine;
    private readonly ClimateVarEngine? _varEngine;

    public PortfolioRiskEngine(
        RiskCalculationEngine riskEngine,
        FinancialImpactEngine? financialEngine,
        ClimateVarEngine? varEngine)
    {
        _riskEngine = riskEngine;
        _financialEngine = financialEngine;
        _varEngine = varEngine;
    }

    public async Task<PortfolioResult> CalculateAsync(
        IReadOnlyList<CompanyProfile> profiles, RiskInput template,
        IReadOnlyList<string> ngfsScenarios, CancellationToken ct = default)
    {
        var results = new List<PortfolioCompanyResult>();

        foreach (var profile in profiles)
        {
            var input = template with { CompanyProfileId = profile.Id };
            var assessment = await _riskEngine.AssessAsync(input, ct);

            FinancialImpact? financial = null;
            if (_financialEngine != null)
            {
                financial = _financialEngine.Calculate(
                    profile, assessment.PhysicalRisk, assessment.TransitionRisk,
                    assessment.Signals.Select(s => s.Source).ToList());
            }

            var riskScore = assessment.PhysicalRisk * 0.6 + assessment.TransitionRisk * 0.4;
            var revenueAtRisk = CalculateRevenueAtRisk(profile.TotalRevenue, riskScore, assessment.Decision);

            results.Add(new PortfolioCompanyResult
            {
                Company = profile,
                Assessment = assessment,
                FinancialImpact = financial,
                RiskScore = riskScore,
                RevenueAtRisk = revenueAtRisk,
                Decision = assessment.Decision,
                RiskLevel = ClassifyRisk(riskScore)
            });
        }

        var totalRevenue = profiles.Sum(p => p.TotalRevenue);
        var totalAtRisk = results.Sum(r => r.RevenueAtRisk);
        var concentration = CalculateConcentrationRisk(results, totalRevenue);

        var ranking = results.OrderByDescending(r => r.RiskScore).ToList();

        var portfolioDecision = results.Count(r => r.Decision == "REJECT") > 0 ? "REVIEW"
            : results.Count(r => r.Decision == "ALLOW") > results.Count * 0.6 ? "ALLOW"
            : "REVIEW";

        return new PortfolioResult
        {
            Companies = results,
            TotalRevenue = totalRevenue,
            TotalAtRisk = totalAtRisk,
            ConcentrationRisk = concentration,
            RiskRanking = ranking,
            PortfolioDecision = portfolioDecision
        };
    }

    public static double CalculateRevenueAtRisk(double totalRevenue, double riskScore, string decision)
    {
        var multiplier = decision switch
        {
            "REJECT" => 1.0,
            "REVIEW" => 0.5,
            _ => 0.1
        };
        return totalRevenue * riskScore * multiplier;
    }

    public static double CalculateConcentrationRisk(List<PortfolioCompanyResult> companies, double totalRevenue)
    {
        if (totalRevenue <= 0 || companies.Count == 0) return 0;
        var maxRisk = companies.Max(c => c.RevenueAtRisk);
        return maxRisk / totalRevenue * 100;
    }

    public static string ClassifyRisk(double score) => score switch
    {
        >= 0.8 => "Kritik",
        >= 0.6 => "Yüksek",
        >= 0.4 => "Orta",
        >= 0.2 => "Düşük",
        _ => "Minimal"
    };
}
