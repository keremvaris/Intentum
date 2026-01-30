// Intentum Example: Greenwashing detection
// Run: dotnet run --project examples/greenwashing-intent
// No API key needed (rule-based intent model).

using System.Text.RegularExpressions;
using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;
using Intentum.Runtime;
using Intentum.Runtime.Policy;
using GreenwashingExample;

// Sample sustainability report (vague claims, metrics without proof)
var report = """
    EcoCorp is committed to a sustainable future and green transition. Our values include
    respect for nature and ecological balance. We have achieved 40% emissions reduction
    and 25% less water use. Our clean production methods support the environment.
    We are on a journey toward carbon neutrality. See our annual report for more.
    """;

Console.WriteLine("=== Intentum Example: Greenwashing detection ===\n");
Console.WriteLine("Report excerpt: \"...sustainable future...40% emissions reduction...carbon neutrality...\"\n");

// 1. Build behavior space from report
var space = SustainabilityReporter.AnalyzeReport(report);
Console.WriteLine($"Collected {space.Events.Count} behavioral signals.\n");

// 2. Infer intent
var intentModel = new GreenwashingIntentModel();
var intent = intentModel.Infer(space);

Console.WriteLine($"Intent: {intent.Name}");
Console.WriteLine($"Confidence: {intent.Confidence.Level} ({intent.Confidence.Score:F2})");
if (intent.Reasoning != null)
    Console.WriteLine($"Reasoning: {intent.Reasoning}");
Console.WriteLine($"Signals: {string.Join(", ", intent.Signals.Select(s => s.Description))}\n");

// 3. Policy decision
var policy = new IntentPolicyBuilder()
    .Escalate("CriticalGreenwashing", i => i is { Name: "ActiveGreenwashing", Confidence.Score: >= 0.7 })
    .Warn("NeedsVerification", i => i.Name == "StrategicObfuscation" || i is { Name: "SelectiveDisclosure", Confidence.Score: >= 0.5 })
    .Observe("Monitor", i => i.Confidence.Score > 0.3)
.Allow("LowRisk", _ => true)
.Build();

var decision = intent.Decide(policy);
Console.WriteLine($"Policy decision: {decision}\n");

// 4. Solution suggestions (application layer)
var solutions = SustainabilitySolutionGenerator.Suggest(intent, space, decision);
Console.WriteLine("Suggested actions:");
foreach (var action in solutions)
    Console.WriteLine($"  • {action}");

Console.WriteLine("\nSee docs/en/greenwashing-detection-howto.md and examples/greenwashing-intent/README.md.");

namespace GreenwashingExample
{
// --- SustainabilityReporter: report text -> BehaviorSpace ---
internal static partial class SustainabilityReporter
{
    private static readonly string[] VaguePatterns = ["sustainable future", "green transition", "eco-friendly", "clean production", "ecological balance", "carbon neutrality", "respect for nature"];

    [GeneratedRegex(@"%\s*(reduction|increase|improvement)|(\d+\s*(ton|kg|kWh|CO2|CO₂))", RegexOptions.IgnoreCase)]
    private static partial Regex MetricsPattern();

    [GeneratedRegex(@"(more|less|better|greener)\s+(than|ever)", RegexOptions.IgnoreCase)]
    private static partial Regex UnsubstantiatedComparisonPattern();

    public static BehaviorSpace AnalyzeReport(string report)
    {
        var space = new BehaviorSpace();
        if (string.IsNullOrWhiteSpace(report))
            return space;

        // Vague claims
        foreach (var pattern in VaguePatterns)
        {
            var count = Regex.Count(report, Regex.Escape(pattern), RegexOptions.IgnoreCase);
            for (var i = 0; i < count; i++)
                space.Observe("language", "claim.vague");
        }

        // Metrics without proof
        var hasMetrics = MetricsPattern().IsMatch(report);
        var hasProof = report.Contains("ISO", StringComparison.OrdinalIgnoreCase) || report.Contains("verified", StringComparison.OrdinalIgnoreCase) || report.Contains("audit", StringComparison.OrdinalIgnoreCase);
        if (hasMetrics && !hasProof)
            space.Observe("data", "metrics.without.proof");

        // Unsubstantiated comparison
        if (UnsubstantiatedComparisonPattern().IsMatch(report))
            space.Observe("language", "comparison.unsubstantiated");

        return space;
    }
}

// --- GreenwashingIntentModel: BehaviorSpace -> Intent ---
internal sealed class GreenwashingIntentModel : IIntentModel
{
    private static readonly Dictionary<string, double> SignalWeights = new(StringComparer.OrdinalIgnoreCase)
    {
        ["language:claim.vague"] = 0.15,
        ["language:comparison.unsubstantiated"] = 0.25,
        ["data:metrics.without.proof"] = 0.35,
        ["data:baseline.manipulation"] = 0.45,
        ["imagery:nature.without.data"] = 0.2
    };

    private static string GetIntentNameFromScore(double score)
    {
        if (score >= 0.8) return "ActiveGreenwashing";
        if (score >= 0.6) return "StrategicObfuscation";
        if (score >= 0.4) return "SelectiveDisclosure";
        if (score >= 0.2) return "UnintentionalMisrepresentation";
        return "GenuineSustainability";
    }

    public Intent Infer(BehaviorSpace behaviorSpace, BehaviorVector? precomputedVector = null)
    {
        var vector = precomputedVector ?? behaviorSpace.ToVector();
        var totalWeight = 0.0;
        var signalList = new List<IntentSignal>();

        foreach (var (dim, count) in vector.Dimensions)
        {
            var weight = SignalWeights.GetValueOrDefault(dim, 0.1) * Math.Min(count, 5);
            totalWeight += weight;
            signalList.Add(new IntentSignal("greenwashing", dim, weight));
        }

        // Normalize score to [0, 1] (cap effect of many signals)
        var score = Math.Min(1.0, totalWeight / 3.0);
        var confidence = IntentConfidence.FromScore(score);
        var name = GetIntentNameFromScore(score);
        var reasoning = $"{behaviorSpace.Events.Count} signals; weighted score {totalWeight:F2} -> {name}";

        return new Intent(Name: name, Signals: signalList, Confidence: confidence, Reasoning: reasoning);
    }
}

// --- Solution generator: intent + space + decision -> action list ---
internal static class SustainabilitySolutionGenerator
{
    public static IReadOnlyList<string> Suggest(Intent intent, BehaviorSpace space, PolicyDecision decision)
    {
        var actions = new List<string>();

        if (decision == PolicyDecision.Escalate && intent.Name == "ActiveGreenwashing")
        {
            actions.Add("IMMEDIATE: Suspend environmental marketing claims");
            actions.Add("IMMEDIATE: Initiate internal review");
            actions.Add("WITHIN 24H: Prepare public clarification");
        }

        if (decision == PolicyDecision.Warn)
        {
            actions.Add("Third-party data audit");
            actions.Add("Methodology review");
            actions.Add("Stakeholder consultation");
        }

        if (space.Events.Any(e => e.Action == "metrics.without.proof"))
            actions.Add("Publish supporting data for all metric claims");

        if (space.Events.Any(e => e.Action == "baseline.manipulation"))
            actions.Add("Recalculate using industry-standard baseline");

        if (intent.Confidence.Score > 0.3 && actions.Count == 0)
            actions.Add("Enhanced quarterly monitoring of language and data completeness");

        if (actions.Count == 0)
            actions.Add("No urgent actions; continue standard disclosure.");

        return actions;
    }
}
}
