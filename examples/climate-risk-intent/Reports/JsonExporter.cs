using System.Text.Json;
using Intentum.Example.ClimateRisk.Models;

namespace Intentum.Example.ClimateRisk.Reports;

public static class JsonExporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string Export(RiskAssessment assessment)
    {
        var obj = new
        {
            scenario = new { assessment.Scenario.Id, assessment.Scenario.Name, assessment.Scenario.Description },
            sector = new { assessment.Sector.Name },
            horizon = assessment.Horizon.ToString(),
            physicalRisk = new { score = assessment.PhysicalRiskScore, factors = assessment.PhysicalFactors.Select(f => new { f.Category, f.Name, f.WeightedScore }) },
            transitionRisk = new { score = assessment.TransitionRiskScore, factors = assessment.TransitionFactors.Select(f => new { f.Category, f.Name, f.WeightedScore }) },
            overallRisk = assessment.OverallRiskScore,
            recommendedActions = assessment.RecommendedActions
        };

        return JsonSerializer.Serialize(obj, Options);
    }

    public static void ExportToFile(RiskAssessment assessment, string path)
    {
        var json = Export(assessment);
        File.WriteAllText(path, json);
    }
}
