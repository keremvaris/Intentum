using Intentum.Sample.Blazor.Components.Services.ClimateData;

namespace Intentum.Sample.Blazor.Tests.ClimateData;

public class FinancialImpactEngineTests
{
    [Fact]
    public void Calculate_WithSimpleProfile_ReturnsExpectedImpacts()
    {
        var profile = new CompanyProfile
        {
            Categories =
            [
                new FinancialCategory
                {
                    Type = FinancialCategoryType.Opex,
                    Name = "OPEX",
                    LineItems =
                    [
                        new FinancialLineItem { Name = "Enerji", Value = 1000, TransitionSensitivity = 1.0, MappedRiskSignals = ["carbon_price"] }
                    ]
                },
                new FinancialCategory
                {
                    Type = FinancialCategoryType.Revenue,
                    Name = "Ciro",
                    LineItems =
                    [
                        new FinancialLineItem { Name = "Satis", Value = 5000, PhysicalSensitivity = 0.5 }
                    ]
                }
            ]
        };

        var engine = new FinancialImpactEngine();
        var result = engine.Calculate(profile, physicalRisk: 0.4, transitionRisk: 0.6, activeSignals: ["carbon_price"]);

        // Opex: 1000 * 0.6 * 1.0 * 1.2 = 720
        Assert.Equal(720, result.TotalOpexImpact, precision: 2);
        // Revenue: -5000 * 0.4 * 0.5 = -1000
        Assert.Equal(-1000, result.TotalRevenueImpact, precision: 2);
        // Net: -1000 - 720 = -1720 (revenue loss minus opex cost increase)
        Assert.Equal(-1720, result.NetCashFlowImpact, precision: 2);
    }

    [Fact]
    public void Calculate_WithNoSignals_BoostIsOne()
    {
        var profile = new CompanyProfile
        {
            Categories =
            [
                new FinancialCategory
                {
                    Type = FinancialCategoryType.Opex,
                    Name = "OPEX",
                    LineItems =
                    [
                        new FinancialLineItem { Name = "Bakim", Value = 2000, PhysicalSensitivity = 0.5 }
                    ]
                }
            ]
        };

        var engine = new FinancialImpactEngine();
        var result = engine.Calculate(profile, physicalRisk: 0.5, transitionRisk: 0.0, activeSignals: []);

        // 2000 * 0.5 * 0.5 = 500
        Assert.Equal(500, result.TotalOpexImpact, precision: 2);
    }

    [Fact]
    public void Calculate_CapexAndCashFlow_HaveCorrectSigns()
    {
        var profile = new CompanyProfile
        {
            Categories =
            [
                new FinancialCategory
                {
                    Type = FinancialCategoryType.Capex,
                    Name = "CAPEX",
                    LineItems =
                    [
                        new FinancialLineItem { Name = "Tesis", Value = 10000, PhysicalSensitivity = 0.5 }
                    ]
                },
                new FinancialCategory
                {
                    Type = FinancialCategoryType.CashFlow,
                    Name = "Nakit",
                    LineItems =
                    [
                        new FinancialLineItem { Name = "FCF", Value = 8000, PhysicalSensitivity = 0.5 }
                    ]
                }
            ]
        };

        var engine = new FinancialImpactEngine();
        var result = engine.Calculate(profile, physicalRisk: 0.5, transitionRisk: 0.0, activeSignals: []);

        Assert.Equal(2500, result.TotalCapexImpact, precision: 2);      // positive cost magnitude
        Assert.Equal(-2000, result.TotalCashFlowImpact, precision: 2);  // negative loss
        Assert.Equal(-4500, result.NetCashFlowImpact, precision: 2);    // -2500 cost - 2000 loss
    }

    [Fact]
    public void Calculate_PartialSignalBoost_AppliesCorrectMultiplier()
    {
        var profile = new CompanyProfile
        {
            Categories =
            [
                new FinancialCategory
                {
                    Type = FinancialCategoryType.Opex,
                    Name = "OPEX",
                    LineItems =
                    [
                        new FinancialLineItem
                        {
                            Name = "Enerji",
                            Value = 1000,
                            TransitionSensitivity = 1.0,
                            MappedRiskSignals = ["carbon_price", "policy"]
                        }
                    ]
                }
            ]
        };

        var engine = new FinancialImpactEngine();
        var result = engine.Calculate(profile, physicalRisk: 0.0, transitionRisk: 0.5, activeSignals: ["carbon_price"]);

        // 1000 * 0.5 * 1.0 * (1 + 0.2 * 1/2) = 550
        Assert.Equal(550, result.TotalOpexImpact, precision: 2);
    }

    [Fact]
    public void Calculate_NullProfile_ThrowsArgumentNullException()
    {
        var engine = new FinancialImpactEngine();
        Assert.Throws<ArgumentNullException>(() => engine.Calculate(null!, 0.0, 0.0, []));
    }
}
