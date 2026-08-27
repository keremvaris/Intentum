using Intentum.Sample.Blazor.Components.Services.ClimateData;

namespace Intentum.Sample.Blazor.Tests.ClimateData;

public class StressTestEngineTests
{
    [Fact]
    public void StressFactors_Default_AllMultipliersAreOne()
    {
        var factors = new StressFactors();
        Assert.Equal(1.0, factors.TemperatureMultiplier);
        Assert.Equal(1.0, factors.CarbonPriceMultiplier);
        Assert.Equal(1.0, factors.PhysicalRiskMultiplier);
    }

    [Fact]
    public void StressDelta_DecisionChange_FormatsCorrectly()
    {
        var delta = new StressDelta { DecisionChange = "ALLOW→REVIEW" };
        Assert.Contains("→", delta.DecisionChange);
    }

    [Fact]
    public void ApplyFactors_TemperatureMultiplier_ModifiesInput()
    {
        var input = CreateInput();
        var factors = new StressFactors { TemperatureMultiplier = 2.0 };

        var stressed = StressTestEngine.ApplyFactors(input, factors);

        Assert.Equal(input.TempAnomaly * 2.0, stressed.TempAnomaly);
    }

    [Fact]
    public void ApplyFactors_CarbonPriceMultiplier_ModifiesInput()
    {
        var input = CreateInput();
        var factors = new StressFactors { CarbonPriceMultiplier = 3.0 };

        var stressed = StressTestEngine.ApplyFactors(input, factors);

        Assert.Equal((int)(input.CarbonPrice * 3.0), stressed.CarbonPrice);
    }

    [Fact]
    public void CalculateDelta_WithDifferentDecisions_ShowsChange()
    {
        var baseline = new RiskAssessment { Decision = "ALLOW", PhysicalRisk = 0.3, TransitionRisk = 0.2 };
        var stressed = new RiskAssessment { Decision = "REVIEW", PhysicalRisk = 0.6, TransitionRisk = 0.5 };

        var delta = StressTestEngine.CalculateDelta(baseline, stressed);

        Assert.Equal(0.3, delta.PhysicalRiskDelta, 2);
        Assert.Equal("ALLOW→REVIEW", delta.DecisionChange);
    }

    [Fact]
    public void ClassifySensitivity_HighScore_ReturnsYuksek()
    {
        var item = StressTestEngine.ClassifySensitivity("Sıcaklık", 0.9);
        Assert.Equal("Yüksek", item.Risk);
    }

    private static RiskInput CreateInput() => new()
    {
        Scenario = "SSP2-4.5", Sector = "Sanayi", Horizon = 2050,
        Latitude = 39.93, Longitude = 32.86, CountryIso3 = "TUR",
        TempAnomaly = 2.4, PrecipChange = -15, SeaLevelRise = 0.5,
        CarbonPrice = 85
    };
}
