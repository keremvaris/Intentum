using Intentum.Sample.Blazor.Components.Services.ClimateData;

namespace Intentum.Sample.Blazor.Tests.ClimateData;

public class RiskMatrixEngineTests
{
    private static CompanyProfile Profile() => new()
    {
        Name = "Test",
        Categories =
        [
            new FinancialCategory
            {
                Type = FinancialCategoryType.Revenue,
                Name = "Gelir",
                LineItems =
                [
                    new FinancialLineItem { Name = "Satış", Value = 10_000_000, Sensitivity = 0.6, AdaptiveCapacity = 0.4 }
                ]
            },
            new FinancialCategory
            {
                Type = FinancialCategoryType.Opex,
                Name = "Operasyon",
                LineItems =
                [
                    new FinancialLineItem { Name = "Enerji", Value = 2_000_000, Sensitivity = 0.8, AdaptiveCapacity = 0.2 }
                ]
            }
        ]
    };

    [Fact]
    public void ComputeHazardExposureMatrix_ReturnsExpectedStructure()
    {
        var engine = new RiskMatrixEngine();
        var matrix = engine.ComputeHazardExposureMatrix(Profile(), hazardProvider: h => 0.5);

        Assert.NotNull(matrix);
        Assert.True(matrix.Hazards.Count > 0);
        Assert.Equal(matrix.Hazards.Count * matrix.Categories.Count, matrix.Cells.Count);
    }

    [Fact]
    public void Vulnerability_IsSensitivityOverAdaptiveCapacity()
    {
        var engine = new RiskMatrixEngine();
        // Sensitivity=0.8, AdaptiveCapacity=0.2 → 0.8/0.2 = 4 → clamped to 1
        var v = engine.ComputeVulnerability(0.8, 0.2);
        Assert.Equal(1.0, v);
    }

    [Fact]
    public void Vulnerability_ZeroAdaptiveCapacity_IsMax()
    {
        var engine = new RiskMatrixEngine();
        Assert.Equal(1.0, engine.ComputeVulnerability(0.5, 0.0));
    }

    [Fact]
    public void Vulnerability_LowSensitivity_HighCapacity_IsLow()
    {
        var engine = new RiskMatrixEngine();
        // 0.2/1.0 = 0.2
        Assert.Equal(0.2, engine.ComputeVulnerability(0.2, 1.0), 2);
    }

    [Fact]
    public void RiskScore_MultipliesHazardExposureVulnerability()
    {
        var engine = new RiskMatrixEngine();
        var score = engine.ComputeRiskScore(hazard: 0.5, exposure: 0.8, vulnerability: 0.4);
        Assert.Equal(0.5 * 0.8 * 0.4, score, 5);
    }

    [Fact]
    public void RiskScore_ClampsToZeroOne()
    {
        var engine = new RiskMatrixEngine();
        Assert.Equal(1.0, engine.ComputeRiskScore(hazard: 1.0, exposure: 1.0, vulnerability: 1.0), 5);
        Assert.Equal(0.0, engine.ComputeRiskScore(hazard: 0, exposure: 1, vulnerability: 1), 5);
    }

    [Fact]
    public void ScenarioMatrix_HasSSPScenarios()
    {
        var engine = new RiskMatrixEngine();
        var matrix = engine.ComputeScenarioMatrix(
            profile: Profile(),
            risksByScenario: new Dictionary<string, double>
            {
                ["SSP1-2.6"] = 0.3,
                ["SSP2-4.5"] = 0.5,
                ["SSP3-7.0"] = 0.7,
                ["SSP5-8.5"] = 0.9
            },
            hazardProvider: h => 0.6);

        Assert.NotNull(matrix);
        Assert.Contains("SSP5-8.5", matrix.Scenarios);
        Assert.Equal(4, matrix.Scenarios.Count);
    }
}
