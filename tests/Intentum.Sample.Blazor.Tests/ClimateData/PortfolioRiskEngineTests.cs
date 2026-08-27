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
}
