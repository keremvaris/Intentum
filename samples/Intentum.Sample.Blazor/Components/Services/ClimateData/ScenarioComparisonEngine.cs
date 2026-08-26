namespace Intentum.Sample.Blazor.Components.Services.ClimateData;

public sealed class ScenarioComparisonEngine
{
    private readonly RiskCalculationEngine _riskEngine;
    private static readonly string[] Scenarios = ["SSP1-2.6", "SSP2-4.5", "SSP3-7.0", "SSP5-8.5"];

    public ScenarioComparisonEngine(RiskCalculationEngine riskEngine)
    {
        _riskEngine = riskEngine;
    }

    public async Task<List<ScenarioComparisonResult>> CompareAllAsync(
        CompanyProfile profile,
        RiskInput baseInput,
        CancellationToken ct = default)
    {
        var tasks = Scenarios.Select(async scenario =>
        {
            var input = baseInput with { Scenario = scenario };
            var assessment = await _riskEngine.AssessAsync(input, ct);
            return new ScenarioComparisonResult
            {
                Scenario = scenario,
                Assessment = assessment,
                Impact = assessment.FinancialImpact ?? new FinancialImpact()
            };
        });

        var results = await Task.WhenAll(tasks);
        return results.ToList();
    }
}
