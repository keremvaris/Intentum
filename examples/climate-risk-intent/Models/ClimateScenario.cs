namespace Intentum.Example.ClimateRisk.Models;

public sealed record ClimateScenario(
    string Id,
    string Name,
    string Description,
    ScenarioType Type,
    double TemperatureIncrease,
    double EmissionPathway,
    double PhysicalRiskMultiplier,
    double TransitionRiskMultiplier)
{
    public string DisplayName => $"{Id} ({Name})";
}

public enum ScenarioType
{
    SSP,
    RCP,
    WRI
}