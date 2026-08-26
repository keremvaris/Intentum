using Intentum.Example.ClimateRisk.Models;

namespace Intentum.Example.ClimateRisk.Risks;

public static class TransitionRiskCalculator
{
    public static (double Score, IReadOnlyList<RiskFactor> Factors) Calculate(
        ClimateScenario scenario,
        SectorProfile sector,
        TimeHorizon horizon)
    {
        var multiplier = scenario.TransitionRiskMultiplier * horizon.GetMultiplier();
        var factors = new List<RiskFactor>
        {
            CalculateFactor("Policy", GetPolicyImpact(sector), GetPolicySpeed(scenario), multiplier),
            CalculateFactor("Technology", GetTechnologyImpact(sector), GetTechnologySpeed(scenario), multiplier),
            CalculateFactor("Market", GetMarketImpact(sector), GetMarketSpeed(scenario), multiplier),
            CalculateFactor("Reputation", GetReputationImpact(sector), GetReputationSpeed(scenario), multiplier)
        };

        var score = factors.Average(f => f.WeightedScore);
        return (score, factors);
    }

    private static RiskFactor CalculateFactor(string name, double impact, double speed, double multiplier)
    {
        var uncertainty = 0.5;
        var weightedScore = impact * speed * uncertainty * multiplier;
        return new RiskFactor("Transition", name, impact, speed, uncertainty, weightedScore);
    }

    private static double GetPolicyImpact(SectorProfile sector) => sector.TransitionSensitivity * 0.9;
    private static double GetTechnologyImpact(SectorProfile sector) => sector.TransitionSensitivity * 0.8;
    private static double GetMarketImpact(SectorProfile sector) => sector.TransitionSensitivity * 0.7;
    private static double GetReputationImpact(SectorProfile sector) => sector.TransitionSensitivity * 0.6;

    private static double GetPolicySpeed(ClimateScenario scenario) => scenario.EmissionPathway switch
    {
        <= 3.0 => 0.9,
        <= 5.0 => 0.6,
        <= 7.0 => 0.4,
        _ => 0.2
    };

    private static double GetTechnologySpeed(ClimateScenario scenario) => scenario.EmissionPathway switch
    {
        <= 3.0 => 0.85,
        <= 5.0 => 0.65,
        <= 7.0 => 0.45,
        _ => 0.3
    };

    private static double GetMarketSpeed(ClimateScenario scenario) => scenario.EmissionPathway switch
    {
        <= 3.0 => 0.8,
        <= 5.0 => 0.55,
        <= 7.0 => 0.35,
        _ => 0.25
    };

    private static double GetReputationSpeed(ClimateScenario scenario) => scenario.EmissionPathway switch
    {
        <= 3.0 => 0.75,
        <= 5.0 => 0.5,
        <= 7.0 => 0.3,
        _ => 0.2
    };
}
