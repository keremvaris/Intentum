using Intentum.Example.ClimateRisk.Models;

namespace Intentum.Example.ClimateRisk.Scenarios;

public static class RcpScenarios
{
    public static readonly ClimateScenario Rcp26 = new(
        Id: "RCP2.6",
        Name: "Peak and Decline",
        Description: "Radiative forcing 2.6 W/m², peak emissions by 2020, decline thereafter",
        Type: ScenarioType.RCP,
        TemperatureIncrease: 1.5,
        EmissionPathway: 2.6,
        PhysicalRiskMultiplier: 0.45,
        TransitionRiskMultiplier: 0.85);

    public static readonly ClimateScenario Rcp45 = new(
        Id: "RCP4.5",
        Name: "Stabilization",
        Description: "Radiative forcing 4.5 W/m², emissions peak by 2040, decline slowly",
        Type: ScenarioType.RCP,
        TemperatureIncrease: 2.4,
        EmissionPathway: 4.5,
        PhysicalRiskMultiplier: 0.65,
        TransitionRiskMultiplier: 0.65);

    public static readonly ClimateScenario Rcp60 = new(
        Id: "RCP6.0",
        Name: "High Stabilization",
        Description: "Radiative forcing 6.0 W/m², emissions peak by 2060, slow decline",
        Type: ScenarioType.RCP,
        TemperatureIncrease: 3.0,
        EmissionPathway: 6.0,
        PhysicalRiskMultiplier: 0.8,
        TransitionRiskMultiplier: 0.45);

    public static readonly ClimateScenario Rcp85 = new(
        Id: "RCP8.5",
        Name: "Very High Emissions",
        Description: "Radiative forcing 8.5 W/m², increasing emissions throughout 21st century",
        Type: ScenarioType.RCP,
        TemperatureIncrease: 4.3,
        EmissionPathway: 8.5,
        PhysicalRiskMultiplier: 1.0,
        TransitionRiskMultiplier: 0.3);

    public static IReadOnlyList<ClimateScenario> All => [Rcp26, Rcp45, Rcp60, Rcp85];

    public static ClimateScenario? GetById(string id) => All.FirstOrDefault(
        s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
}
