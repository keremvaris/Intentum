using Intentum.Sample.Blazor.Components.Services.ClimateData;

namespace Intentum.Sample.Blazor.Tests.ClimateData;

public class PortfolioRiskEngineTests
{
    [Fact]
    public void PortfolioResult_DefaultValues_AreZero()
    {
        var result = new PortfolioResult();
        Assert.Equal(0, result.TotalRevenue);
        Assert.Empty(result.Companies);
    }

    [Fact]
    public void PortfolioCompanyResult_RiskLevel_DefaultsEmpty()
    {
        var result = new PortfolioCompanyResult();
        Assert.Equal("", result.RiskLevel);
    }

    [Fact]
    public void CalculateRevenueAtRisk_Reject_MultiplierIsOne()
    {
        var risk = PortfolioRiskEngine.CalculateRevenueAtRisk(1000000, 0.8, "REJECT");
        Assert.Equal(800000, risk);
    }

    [Fact]
    public void CalculateRevenueAtRisk_Allow_MultiplierIsPointOne()
    {
        var risk = PortfolioRiskEngine.CalculateRevenueAtRisk(1000000, 0.8, "ALLOW");
        Assert.Equal(80000, risk);
    }

    [Fact]
    public void CalculateConcentrationRisk_SingleCompany_IsHundred()
    {
        var companies = new List<PortfolioCompanyResult>
        {
            new() { RevenueAtRisk = 500000 }
        };
        var concentration = PortfolioRiskEngine.CalculateConcentrationRisk(companies, 500000);
        Assert.Equal(100, concentration);
    }

    [Fact]
    public void ClassifyRisk_HighScore_ReturnsKritik()
    {
        var level = PortfolioRiskEngine.ClassifyRisk(0.9);
        Assert.Equal("Kritik", level);
    }
}
