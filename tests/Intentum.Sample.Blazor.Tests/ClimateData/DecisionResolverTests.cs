using Intentum.Sample.Blazor.Components.Services.ClimateData;

namespace Intentum.Sample.Blazor.Tests.ClimateData;

public class DecisionResolverTests
{
    private static FinancialImpact Impact(double revenue, double opex, double capex, double cashFlow) => new()
    {
        CategoryImpacts =
        [
            new CategoryImpact { Type = FinancialCategoryType.Revenue, PhysicalImpact = revenue, TransitionImpact = 0 },
            new CategoryImpact { Type = FinancialCategoryType.Opex, PhysicalImpact = opex, TransitionImpact = 0 },
            new CategoryImpact { Type = FinancialCategoryType.Capex, PhysicalImpact = capex, TransitionImpact = 0 },
            new CategoryImpact { Type = FinancialCategoryType.CashFlow, PhysicalImpact = cashFlow, TransitionImpact = 0 }
        ]
    };

    // NetCashFlowImpact = Revenue - Opex - Capex + CashFlow
    // Örn: revenue=0, opex=15M → net = -15M

    [Fact]
    public void DetermineDecision_HighRisk_ReturnsReject()
    {
        Assert.Equal("REJECT", RiskCalculationEngine.DetermineDecision(0.80, "Kritik İklim Riski"));
    }

    [Fact]
    public void DetermineDecision_MediumRisk_ReturnsReview()
    {
        Assert.Equal("REVIEW", RiskCalculationEngine.DetermineDecision(0.65, "Yüksek İklim Riski"));
    }

    [Fact]
    public void DetermineDecision_LowRisk_ReturnsAllow()
    {
        Assert.Equal("ALLOW", RiskCalculationEngine.DetermineDecision(0.35, "Düşük İklim Riski"));
    }

    [Fact]
    public void DetermineDecision_HighFinancialLoss_EscalatesAllowToReview()
    {
        var financial = Impact(revenue: 0, opex: 15_000_000, capex: 0, cashFlow: 0);
        Assert.Equal("REVIEW", RiskCalculationEngine.DetermineDecision(0.50, "Orta İklim Riski", financial));
    }

    [Fact]
    public void DetermineDecision_CriticalFinancialLoss_EscalatesReviewToReject()
    {
        var financial = Impact(revenue: 0, opex: 30_000_000, capex: 0, cashFlow: 0);
        Assert.Equal("REJECT", RiskCalculationEngine.DetermineDecision(0.65, "Yüksek İklim Riski", financial));
    }

    [Fact]
    public void DetermineDecision_SmallFinancialLoss_DoesNotChangeDecision()
    {
        var financial = Impact(revenue: 0, opex: 2_000_000, capex: 0, cashFlow: 0);
        Assert.Equal("ALLOW", RiskCalculationEngine.DetermineDecision(0.50, "Orta İklim Riski", financial));
    }

    [Fact]
    public void DetermineDecision_NoFinancialImpact_UsesRiskOnly()
    {
        Assert.Equal("ALLOW", RiskCalculationEngine.DetermineDecision(0.50, "Orta İklim Riski", null));
    }

    [Fact]
    public void DetermineDecision_LowDataConfidence_EscalatesAllowToReview()
    {
        Assert.Equal("REVIEW", RiskCalculationEngine.DetermineDecision(0.50, "Orta İklim Riski", null, dataConfidence: 0.5));
    }

    [Fact]
    public void DetermineDecision_RegionalEstimate_EscalatesAllowToReview()
    {
        Assert.Equal("REVIEW", RiskCalculationEngine.DetermineDecision(0.50, "Orta İklim Riski", null, dataConfidence: 1.0, isRegionalEstimate: true));
    }

    [Fact]
    public void DetermineDecision_HighRiskWithLowData_StaysReject()
    {
        // Güvenlik ağı: aşırı risk skoru veri düşüklüğünde bile REJECT kalmalı.
        Assert.Equal("REJECT", RiskCalculationEngine.DetermineDecision(0.85, "Kritik İklim Riski", null, dataConfidence: 0.5));
    }
}
