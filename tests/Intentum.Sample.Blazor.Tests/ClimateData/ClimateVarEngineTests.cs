using Intentum.Sample.Blazor.Components.Services.ClimateData;
using Moq;

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

    [Fact]
    public async Task CalculateAsync_EmptyScenarios_ReturnsZeroes()
    {
        var engine = new ClimateVarEngine(null!, null!);
        var profile = CreateSimpleProfile();
        var input = CreateInput();

        var result = await engine.CalculateAsync(profile, input, []);

        Assert.Equal(0, result.VaR95);
        Assert.Empty(result.LossDistribution);
    }

    [Fact]
    public void CalculateWeight_HigherWarming_LowerWeight()
    {
        var w1 = ClimateVarEngine.CalculateWeight(1.5, "Orderly");
        var w2 = ClimateVarEngine.CalculateWeight(3.0, "HotHouse");

        Assert.True(w1 > w2);
    }

    [Fact]
    public void CalculateWeight_OrderlyCategory_HigherWeight()
    {
        var orderly = ClimateVarEngine.CalculateWeight(2.0, "Orderly");
        var hothouse = ClimateVarEngine.CalculateWeight(2.0, "HotHouse");

        Assert.True(orderly > hothouse);
    }

    private static CompanyProfile CreateSimpleProfile() => new()
    {
        Name = "Test Corp", Sector = "Sanayi", LocationName = "Ankara",
        Categories =
        [
            new FinancialCategory
            {
                Type = FinancialCategoryType.Opex, Name = "OPEX",
                LineItems = [new FinancialLineItem { Name = "Enerji", Value = 1000, TransitionSensitivity = 1.0 }]
            },
            new FinancialCategory
            {
                Type = FinancialCategoryType.Revenue, Name = "Ciro",
                LineItems = [new FinancialLineItem { Name = "Satis", Value = 5000, PhysicalSensitivity = 0.5 }]
            }
        ]
    };

    private static RiskInput CreateInput() => new()
    {
        Scenario = "SSP2-4.5", Sector = "Sanayi", Horizon = 2050,
        Latitude = 39.93, Longitude = 32.86, CountryIso3 = "TUR",
        CarbonPrice = 85
    };
}
