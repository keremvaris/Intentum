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
}
