namespace Intentum.Sample.Blazor.Components.Services.ClimateData;

public sealed class ClimateVarResult
{
    public string CompanyId { get; set; } = "";
    public string CompanyName { get; set; } = "";
    public double ExpectedLoss { get; set; }
    public double VaR95 { get; set; }
    public double VaR99 { get; set; }
    public double CVaR95 { get; set; }
    public string WorstScenario { get; set; } = "";
    public double WorstLoss { get; set; }
    public string BestScenario { get; set; } = "";
    public double BestGain { get; set; }
    public List<ScenarioLoss> LossDistribution { get; set; } = [];
    public string Currency { get; set; } = "TL";
}

public sealed class ScenarioLoss
{
    public string ScenarioName { get; set; } = "";
    public string ScenarioCategory { get; set; } = "";
    public double WarmingLevel { get; set; }
    public double Loss { get; set; }
    public double PhysicalRisk { get; set; }
    public double TransitionRisk { get; set; }
    public double Weight { get; set; }
}
