using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

var embeddingProvider = new MockEmbeddingProvider();
var similarityEngine = new SimpleAverageSimilarityEngine();
var intentModel = new LlmIntentModel(
    embeddingProvider,
    similarityEngine);

var policy = new IntentPolicy()
    .AddRule(new PolicyRule(
        "ExcessiveRetryBlock",
        i => i.Signals.Count(s => s.Description.Contains("retry", StringComparison.OrdinalIgnoreCase)) >= 3,
        PolicyDecision.Block))
    .AddRule(new PolicyRule(
        "HighConfidenceAllow",
        i => i.Confidence.Level is "High" or "Certain",
        PolicyDecision.Allow))
    .AddRule(new PolicyRule(
        "MediumConfidenceObserve",
        i => i.Confidence.Level == "Medium",
        PolicyDecision.Observe))
    .AddRule(new PolicyRule(
        "LowConfidenceWarn",
        i => i.Confidence.Level == "Low",
        PolicyDecision.Warn));

RunScenario(
    "PaymentHappyPath",
    space => space
        .Observe("user", "login")
        .Observe("user", "submit"));

RunScenario(
    "PaymentWithRetries",
    space => space
        .Observe("user", "login")
        .Observe("user", "retry")
        .Observe("user", "retry")
        .Observe("user", "submit"));

RunScenario(
    "SuspiciousRetries",
    space => space
        .Observe("user", "login")
        .Observe("user", "retry")
        .Observe("user", "retry")
        .Observe("user", "retry"));

void RunScenario(string name, Action<BehaviorSpace> build)
{
    var space = new BehaviorSpace();
    build(space);

    var intent = intentModel.Infer(space);
    var decision = intent.Decide(policy);
    var vector = space.ToVector();

    Console.WriteLine($"=== INTENTUM SCENARIO: {name} ===");
    Console.WriteLine($"Events            : {space.Events.Count}");
    Console.WriteLine($"Intent Confidence : {intent.Confidence.Level}");
    Console.WriteLine($"Intent Score      : {intent.Confidence.Score:0.00}");
    Console.WriteLine($"Decision          : {decision}");
    Console.WriteLine("Behavior Vector:");
    foreach (var entry in vector.Dimensions)
    {
        Console.WriteLine($" - {entry.Key} = {entry.Value}");
    }

    Console.WriteLine("Signals:");
    foreach (var signal in intent.Signals)
    {
        Console.WriteLine($" - {signal.Description} ({signal.Weight:0.00})");
    }

    Console.WriteLine();
}
