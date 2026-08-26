using Intentum.Core.Intents;
using Intentum.Example.ClimateRisk.Models;
using Intentum.Example.ClimateRisk.Risks;
using Intentum.Runtime.Policy;

namespace Intentum.Example.ClimateRisk.Reports;

public static class ConsoleReporter
{
    public static void PrintHeader()
    {
        Console.WriteLine("=== Intentum Example: Climate Risk Assessment ===\n");
    }

    public static void PrintScenario(ClimateScenario scenario, SectorProfile sector, TimeHorizon horizon)
    {
        Console.WriteLine($"Scenario: {scenario.DisplayName}");
        Console.WriteLine($"Sector: {sector.Name}");
        Console.WriteLine($"Horizon: {horizon.GetDescription()}\n");
    }

    public static void PrintBehaviorSpace(int eventCount)
    {
        Console.WriteLine($"Behavior Space: {eventCount} signals collected\n");
    }

    public static void PrintIntent(Intent intent)
    {
        Console.WriteLine($"Intent: {intent.Name}");
        Console.WriteLine($"Confidence: {intent.Confidence.Level} ({intent.Confidence.Score:F2})");

        if (intent.Reasoning != null)
            Console.WriteLine($"Reasoning: {intent.Reasoning}");

        Console.WriteLine($"Signals: {string.Join(", ", intent.Signals.Select(s => s.Description))}\n");
    }

    public static void PrintPolicyDecision(PolicyDecision decision, PolicyRule? rule)
    {
        Console.WriteLine($"Policy decision: {decision}");

        if (rule != null)
            Console.WriteLine($"Matched rule: {rule.Name}\n");
    }

    public static void PrintPhysicalRisk(double score, IReadOnlyList<RiskFactor> factors)
    {
        Console.WriteLine($"Physical Risk: {score:F2}");
        foreach (var f in factors)
            Console.WriteLine($"  {f.Name}: {f.WeightedScore:F3} (P:{f.Probability:F2} × S:{f.Severity:F2} × E:{f.Exposure:F2})");
        Console.WriteLine();
    }

    public static void PrintTransitionRisk(double score, IReadOnlyList<RiskFactor> factors)
    {
        Console.WriteLine($"Transition Risk: {score:F2}");
        foreach (var f in factors)
            Console.WriteLine($"  {f.Name}: {f.WeightedScore:F3} (I:{f.Probability:F2} × Sp:{f.Severity:F2} × U:{f.Exposure:F2})");
        Console.WriteLine();
    }

    public static void PrintEconomicImpact(EconomicImpact impact)
    {
        Console.WriteLine($"Economic Impact:");
        Console.WriteLine($"  GDP: {impact.GdpImpactPercent:+0.00%;-0.00%}");
        Console.WriteLine($"  CAPEX: +{impact.InvestmentImpactPercent:P0}");
        Console.WriteLine($"  Insurance: +{impact.InsuranceCostIncreasePercent:P0}");
        Console.WriteLine($"  Borrowing: +{impact.BorrowingCostIncreasePercent:P0}");
        Console.WriteLine($"  Workforce: {impact.WorkforceImpactPercent:+0.00%;-0.00%}\n");
    }

    public static void PrintActions(IReadOnlyList<string> actions)
    {
        Console.WriteLine("Suggested Actions:");
        foreach (var action in actions)
            Console.WriteLine($"  • {action}");
        Console.WriteLine();
    }

    public static void PrintAssessment(RiskAssessment assessment, Intent intent, PolicyDecision decision)
    {
        PrintHeader();
        PrintScenario(assessment.Scenario, assessment.Sector, assessment.Horizon);
        PrintBehaviorSpace(assessment.PhysicalFactors.Count + assessment.TransitionFactors.Count);
        PrintIntent(intent);
        PrintPolicyDecision(decision, null);
        PrintPhysicalRisk(assessment.PhysicalRiskScore, assessment.PhysicalFactors);
        PrintTransitionRisk(assessment.TransitionRiskScore, assessment.TransitionFactors);
        PrintEconomicImpact(EconomicImpactAnalyzer.Calculate(assessment.PhysicalRiskScore, assessment.TransitionRiskScore, assessment.Sector));
        PrintActions(assessment.RecommendedActions);
    }
}
