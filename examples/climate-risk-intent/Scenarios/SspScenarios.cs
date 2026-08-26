using Intentum.Example.ClimateRisk.Models;

namespace Intentum.Example.ClimateRisk.Scenarios;

public static class SspScenarios
{
    public static readonly ClimateScenario Ssp1_26 = new(
        Id: "SSP1-2.6",
        Name: "Sustainable Development",
        Description: "Low emissions, strong climate policies, sustainable growth",
        Type: ScenarioType.SSP,
        TemperatureIncrease: 1.8,
        EmissionPathway: 2.6,
        PhysicalRiskMultiplier: 0.5,
        TransitionRiskMultiplier: 0.8);

    public static readonly ClimateScenario Ssp2_45 = new(
        Id: "SSP2-4.5",
        Name: "Middle of the Road",
        Description: "Moderate emissions, mixed policies, gradual transition",
        Type: ScenarioType.SSP,
        TemperatureIncrease: 2.7,
        EmissionPathway: 4.5,
        PhysicalRiskMultiplier: 0.7,
        TransitionRiskMultiplier: 0.6);

    public static readonly ClimateScenario Ssp3_70 = new(
        Id: "SSP3-7.0",
        Name: "Regional Rivalry",
        Description: "High emissions, regional competition, slow climate action",
        Type: ScenarioType.SSP,
        TemperatureIncrease: 3.6,
        EmissionPathway: 7.0,
        PhysicalRiskMultiplier: 0.85,
        TransitionRiskMultiplier: 0.4);

    public static readonly ClimateScenario Ssp5_85 = new(
        Id: "SSP5-8.5",
        Name: "Fossil-Fueled Development",
        Description: "Very high emissions, fossil fuel dependence, rapid growth",
        Type: ScenarioType.SSP,
        TemperatureIncrease: 4.4,
        EmissionPathway: 8.5,
        PhysicalRiskMultiplier: 1.0,
        TransitionRiskMultiplier: 0.9);

    public static IReadOnlyList<ClimateScenario> All => [Ssp1_26, Ssp2_45, Ssp3_70, Ssp5_85];

    public static ClimateScenario? GetById(string id) => All.FirstOrDefault(
        s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
}
