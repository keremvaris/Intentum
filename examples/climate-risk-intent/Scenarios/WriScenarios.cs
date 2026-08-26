using Intentum.Example.ClimateRisk.Models;

namespace Intentum.Example.ClimateRisk.Scenarios;

public static class WriScenarios
{
    public static readonly ClimateScenario WaterStressLow = new(
        Id: "WRI-WATER-LOW",
        Name: "Low Water Stress",
        Description: "WRI Aqueduct: Low baseline water stress, minimal change under climate scenarios",
        Type: ScenarioType.WRI,
        TemperatureIncrease: 2.0,
        EmissionPathway: 4.0,
        PhysicalRiskMultiplier: 0.3,
        TransitionRiskMultiplier: 0.5);

    public static readonly ClimateScenario WaterStressHigh = new(
        Id: "WRI-WATER-HIGH",
        Name: "High Water Stress",
        Description: "WRI Aqueduct: High baseline water stress, significant degradation under warming",
        Type: ScenarioType.WRI,
        TemperatureIncrease: 3.5,
        EmissionPathway: 6.0,
        PhysicalRiskMultiplier: 0.9,
        TransitionRiskMultiplier: 0.6);

    public static readonly ClimateScenario EnergyTransitionFast = new(
        Id: "WRI-ENERGY-FAST",
        Name: "Fast Energy Transition",
        Description: "WRI Energy Transition: Rapid shift to renewables, high transition risk for fossil assets",
        Type: ScenarioType.WRI,
        TemperatureIncrease: 1.8,
        EmissionPathway: 3.0,
        PhysicalRiskMultiplier: 0.4,
        TransitionRiskMultiplier: 0.95);

    public static readonly ClimateScenario EnergyTransitionSlow = new(
        Id: "WRI-ENERGY-SLOW",
        Name: "Slow Energy Transition",
        Description: "WRI Energy Transition: Gradual shift, lower transition risk but higher physical risk",
        Type: ScenarioType.WRI,
        TemperatureIncrease: 4.0,
        EmissionPathway: 7.5,
        PhysicalRiskMultiplier: 0.95,
        TransitionRiskMultiplier: 0.35);

    public static IReadOnlyList<ClimateScenario> All =>
        [WaterStressLow, WaterStressHigh, EnergyTransitionFast, EnergyTransitionSlow];

    public static ClimateScenario? GetById(string id) => All.FirstOrDefault(
        s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
}
