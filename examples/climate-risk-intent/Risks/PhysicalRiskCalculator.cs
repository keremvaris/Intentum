using Intentum.Example.ClimateRisk.Models;

namespace Intentum.Example.ClimateRisk.Risks;

public static class PhysicalRiskCalculator
{
    public static (double Score, IReadOnlyList<RiskFactor> Factors) Calculate(
        ClimateScenario scenario,
        SectorProfile sector,
        TimeHorizon horizon)
    {
        var multiplier = scenario.PhysicalRiskMultiplier * horizon.GetMultiplier();
        var factors = new List<RiskFactor>
        {
            CalculateFactor("Flood", GetFloodProbability(sector), GetFloodSeverity(sector), multiplier),
            CalculateFactor("Drought", GetDroughtProbability(sector), GetDroughtSeverity(sector), multiplier),
            CalculateFactor("Storm", GetStormProbability(sector), GetStormSeverity(sector), multiplier),
            CalculateFactor("SeaLevelRise", GetSeaLevelProbability(sector), GetSeaLevelSeverity(sector), multiplier),
            CalculateFactor("Heatwave", GetHeatwaveProbability(sector), GetHeatwaveSeverity(sector), multiplier)
        };

        var score = factors.Average(f => f.WeightedScore);
        return (score, factors);
    }

    private static RiskFactor CalculateFactor(string name, double probability, double severity, double multiplier)
    {
        var exposure = multiplier;
        var weightedScore = probability * severity * exposure;
        return new RiskFactor("Physical", name, probability, severity, exposure, weightedScore);
    }

    private static double GetFloodProbability(SectorProfile sector) => sector.Name switch
    {
        "RealEstate" => 0.7,
        "Agriculture" => 0.6,
        "Energy" => 0.5,
        "Tourism" => 0.55,
        "Finance" => 0.2,
        _ => 0.4
    };

    private static double GetFloodSeverity(SectorProfile sector) => sector.Name switch
    {
        "RealEstate" => 0.8,
        "Agriculture" => 0.7,
        "Energy" => 0.6,
        "Tourism" => 0.65,
        "Finance" => 0.3,
        _ => 0.5
    };

    private static double GetDroughtProbability(SectorProfile sector) => sector.Name switch
    {
        "Agriculture" => 0.8,
        "Energy" => 0.5,
        "Tourism" => 0.4,
        "RealEstate" => 0.3,
        "Finance" => 0.15,
        _ => 0.4
    };

    private static double GetDroughtSeverity(SectorProfile sector) => sector.Name switch
    {
        "Agriculture" => 0.9,
        "Energy" => 0.6,
        "Tourism" => 0.5,
        "RealEstate" => 0.4,
        "Finance" => 0.2,
        _ => 0.5
    };

    private static double GetStormProbability(SectorProfile sector) => sector.Name switch
    {
        "RealEstate" => 0.65,
        "Tourism" => 0.6,
        "Energy" => 0.55,
        "Agriculture" => 0.5,
        "Finance" => 0.15,
        _ => 0.4
    };

    private static double GetStormSeverity(SectorProfile sector) => sector.Name switch
    {
        "RealEstate" => 0.75,
        "Tourism" => 0.7,
        "Energy" => 0.65,
        "Agriculture" => 0.6,
        "Finance" => 0.2,
        _ => 0.5
    };

    private static double GetSeaLevelProbability(SectorProfile sector) => sector.Name switch
    {
        "RealEstate" => 0.6,
        "Tourism" => 0.55,
        "Energy" => 0.4,
        "Agriculture" => 0.3,
        "Finance" => 0.1,
        _ => 0.3
    };

    private static double GetSeaLevelSeverity(SectorProfile sector) => sector.Name switch
    {
        "RealEstate" => 0.85,
        "Tourism" => 0.7,
        "Energy" => 0.6,
        "Agriculture" => 0.5,
        "Finance" => 0.15,
        _ => 0.4
    };

    private static double GetHeatwaveProbability(SectorProfile sector) => sector.Name switch
    {
        "Agriculture" => 0.7,
        "Tourism" => 0.65,
        "Energy" => 0.6,
        "RealEstate" => 0.5,
        "Finance" => 0.2,
        _ => 0.45
    };

    private static double GetHeatwaveSeverity(SectorProfile sector) => sector.Name switch
    {
        "Agriculture" => 0.8,
        "Tourism" => 0.7,
        "Energy" => 0.65,
        "RealEstate" => 0.5,
        "Finance" => 0.25,
        _ => 0.5
    };
}
