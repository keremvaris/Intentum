using Intentum.Sample.Blazor.Components.Services.ClimateData;

namespace Intentum.Sample.Blazor.Tests.ClimateData;

public class ClimateVarEngineTests
{
    [Fact]
    public void ScenarioLoss_Constructor_SetsProperties()
    {
        var loss = new ScenarioLoss
        {
            ScenarioName = "Net Zero 2050",
            ScenarioCategory = "Orderly",
            WarmingLevel = 1.5,
            Loss = -500000,
            PhysicalRisk = 0.3,
            TransitionRisk = 0.7,
            Weight = 0.4
        };

        Assert.Equal("Net Zero 2050", loss.ScenarioName);
        Assert.Equal("Orderly", loss.ScenarioCategory);
        Assert.Equal(1.5, loss.WarmingLevel);
        Assert.Equal(-500000, loss.Loss);
        Assert.Equal(0.4, loss.Weight);
    }

    [Fact]
    public void ClimateVarResult_DefaultCurrency_IsTL()
    {
        var result = new ClimateVarResult();
        Assert.Equal("TL", result.Currency);
    }
}
