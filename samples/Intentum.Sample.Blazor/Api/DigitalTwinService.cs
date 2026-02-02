using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Models;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

namespace Intentum.Sample.Blazor.Api;

/// <summary>
/// Stateless service: infers digital twin (Oracle of Operations) intent from variant metric events.
/// </summary>
public sealed class DigitalTwinService
{
    private static IIntentModel BuildModel()
    {
        var rules = new List<Func<BehaviorSpace, RuleMatch?>>
        {
            space =>
            {
                var errorRate = space.Events.Any(e => e.Action == "ErrorRate_Report" && e.Metadata?.TryGetValue("DeviationFromBaseline", out var d) == true && Convert.ToDouble(d) >= 2.0);
                var throughput = space.Events.Any(e => e.Action == "Throughput_Report" && e.Metadata?.TryGetValue("DeviationFromBaseline", out var d) == true && Convert.ToDouble(d) <= -10);
                var externalSpike = space.Events.Any(e => e.Action == "ExternalInput_Received" && e.Metadata?.TryGetValue("Type", out var t) == true && string.Equals(t.ToString(), "Demand_Spike", StringComparison.OrdinalIgnoreCase));
                if (errorRate && (throughput || externalSpike))
                    return new RuleMatch("ConvergingTowardSystemicBottleneckAndMissedSLAs", 0.85, "ErrorRate high + Throughput low or Demand_Spike");
                return null;
            },
            space =>
            {
                var singleError = space.Events.Count(e => e.Action == "ErrorRate_Report" && e.Metadata?.TryGetValue("DeviationFromBaseline", out var d) == true && Convert.ToDouble(d) >= 2.0) == 1;
                var othersNormal = space.Events.Where(e => e.Action == "Throughput_Report" || e.Action == "ErrorRate_Report").All(e => e.Metadata?.TryGetValue("DeviationFromBaseline", out var d) == true && Math.Abs(Convert.ToDouble(d)) < 15);
                if (singleError && othersNormal)
                    return new RuleMatch("SinglePointOfFailure_Emerging", 0.86, "Single component ErrorRate high, others normal");
                return null;
            },
            space =>
            {
                var energyHigh = space.Events.Any(e => e.Action == "EnergyConsumption_Report" && e.Metadata?.TryGetValue("DeviationFromBaseline", out var d) == true && Convert.ToDouble(d) > 15);
                var throughputLow = space.Events.Any(e => e.Action == "Throughput_Report" && e.Metadata?.TryGetValue("DeviationFromBaseline", out var d) == true && Convert.ToDouble(d) < -20);
                if (energyHigh && throughputLow)
                    return new RuleMatch("OptimizingForCostOverSpeed", 0.82, "Energy high + Throughput low");
                return null;
            },
            space =>
            {
                var allBaseline = space.Events.All(e => e.Metadata?.TryGetValue("DeviationFromBaseline", out var d) == true && Math.Abs(Convert.ToDouble(d)) <= 5);
                if (space.Events.Count >= 2 && allBaseline)
                    return new RuleMatch("StableWithinSLA", 0.88, "All metrics near baseline");
                return null;
            }
        };
        return new RuleBasedIntentModel(rules);
    }

    private static readonly IntentPolicy DigitalTwinPolicy = new IntentPolicyBuilder()
        .Escalate("Bottleneck", i => i.Name.Contains("Bottleneck", StringComparison.OrdinalIgnoreCase) || i.Name.Contains("SinglePointOfFailure", StringComparison.OrdinalIgnoreCase))
        .Warn("CostOverSpeed", i => i.Name.Contains("OptimizingForCostOverSpeed", StringComparison.OrdinalIgnoreCase))
        .Allow("Stable", i => i.Name.Contains("StableWithinSLA", StringComparison.OrdinalIgnoreCase))
        .Observe("Default", _ => true)
        .Build();

    private readonly IIntentModel _model = BuildModel();

    public DigitalTwinInferResult Infer(string variant)
    {
        var baseTime = DateTimeOffset.UtcNow;
        var space = DigitalTwinVariants.BuildSpace(variant, baseTime);
        var intent = _model.Infer(space);
        var decision = intent.Decide(DigitalTwinPolicy);
        var events = DigitalTwinVariants.GetEvents(variant, baseTime);
        var recommendedScenario = (decision.ToString() == "Escalate" || decision.ToString() == "Warn") ? DigitalTwinVariants.GetRecommendedScenario(variant) : null;
        return new DigitalTwinInferResult(
            intent.Name,
            intent.Confidence.Level,
            intent.Confidence.Score,
            decision.ToString(),
            recommendedScenario ?? "",
            events.Select(e => e.Summary).ToList()
        );
    }
}

/// <summary>Response for POST /api/digital-twin/infer.</summary>
public sealed record DigitalTwinInferResult(
    string IntentName,
    string ConfidenceLevel,
    double ConfidenceScore,
    string Decision,
    string RecommendedScenario,
    IReadOnlyList<string> EventsSummary
);

/// <summary>Request body for POST /api/digital-twin/infer.</summary>
public sealed record DigitalTwinInferRequest(string? Variant);
