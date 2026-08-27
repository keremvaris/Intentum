namespace Intentum.Sample.Blazor.Components.Services.ClimateData;

public sealed record StressFactors
{
    public double TemperatureMultiplier { get; set; } = 1.0;
    public double PrecipitationMultiplier { get; set; } = 1.0;
    public double SeaLevelMultiplier { get; set; } = 1.0;
    public double WaterStressMultiplier { get; set; } = 1.0;
    public double CarbonPriceMultiplier { get; set; } = 1.0;
    public double WindSpeedMultiplier { get; set; } = 1.0;
    public double PhysicalRiskMultiplier { get; set; } = 1.0;
    public double TransitionRiskMultiplier { get; set; } = 1.0;
}

public sealed class StressTestResult
{
    public RiskAssessment BaselineAssessment { get; set; } = new();
    public RiskAssessment StressedAssessment { get; set; } = new();
    public StressDelta Delta { get; set; } = new();
    public List<FactorContribution> FactorContributions { get; set; } = [];
    public string BreakEvenFactor { get; set; } = "";
    public double BreakEvenMultiplier { get; set; }
    public List<SensitivityItem> SensitivityRanking { get; set; } = [];
}

public sealed class StressDelta
{
    public double PhysicalRiskDelta { get; set; }
    public double TransitionRiskDelta { get; set; }
    public double OverallRiskDelta { get; set; }
    public string DecisionChange { get; set; } = "";
    public double FinancialImpactDelta { get; set; }
}

public sealed class FactorContribution
{
    public string FactorName { get; set; } = "";
    public string FactorNameTr { get; set; } = "";
    public double Multiplier { get; set; }
    public double ContributionPct { get; set; }
    public double MarginalEffect { get; set; }
}

public sealed class SensitivityItem
{
    public string FactorName { get; set; } = "";
    public double SensitivityScore { get; set; }
    public string Risk { get; set; } = "";
}
