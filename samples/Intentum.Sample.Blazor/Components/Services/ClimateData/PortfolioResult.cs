namespace Intentum.Sample.Blazor.Components.Services.ClimateData;

public sealed class PortfolioResult
{
    public List<PortfolioCompanyResult> Companies { get; set; } = [];
    public double TotalRevenue { get; set; }
    public double TotalAtRisk { get; set; }
    public double AggregateVaR95 { get; set; }
    public double ConcentrationRisk { get; set; }
    public double DiversificationBenefit { get; set; }
    public List<PortfolioCompanyResult> RiskRanking { get; set; } = [];
    public string PortfolioDecision { get; set; } = "";
}

public sealed class PortfolioCompanyResult
{
    public CompanyProfile Company { get; set; } = new();
    public RiskAssessment Assessment { get; set; } = new();
    public FinancialImpact? FinancialImpact { get; set; }
    public double RiskScore { get; set; }
    public double RevenueAtRisk { get; set; }
    public string Decision { get; set; } = "";
    public string RiskLevel { get; set; } = "";
}
