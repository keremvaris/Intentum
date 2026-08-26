namespace Intentum.Example.ClimateRisk.Models;

public sealed record RiskAssessment(
    ClimateScenario Scenario,
    SectorProfile Sector,
    TimeHorizon Horizon,
    double PhysicalRiskScore,
    double TransitionRiskScore,
    double EconomicImpactScore,
    IReadOnlyList<RiskFactor> PhysicalFactors,
    IReadOnlyList<RiskFactor> TransitionFactors,
    IReadOnlyList<string> RecommendedActions)
{
    public double OverallRiskScore => (PhysicalRiskScore + TransitionRiskScore) / 2.0;
}

public sealed record RiskFactor(
    string Category,
    string Name,
    double Probability,
    double Severity,
    double Exposure,
    double WeightedScore)
{
    public double RawScore => Probability * Severity * Exposure;
}