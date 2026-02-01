// Hello Intentum — minimal example: one signal, one intent, console output.
// Run: dotnet run --project examples/hello-intentum
// See: docs/en/examples-overview.md (5-minute quick start)

using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Core.Models;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

Console.WriteLine("=== Hello Intentum ===\n");

// One rule: if we see "user:hello", intent is "Greeting" with high confidence
var rules = new List<Func<BehaviorSpace, RuleMatch?>>
{
    space => space.Events.Any(e => e.Action == "hello")
        ? new RuleMatch("Greeting", 0.9, "user said hello")
        : null
};

var model = new RuleBasedIntentModel(rules);
var policy = new IntentPolicyBuilder()
    .Allow("Greeting", i => i.Name == "Greeting")
    .Observe("Unknown", _ => true)
    .Build();

// One signal: user says hello
var space = new BehaviorSpace().Observe("user", "hello");

var intent = model.Infer(space);
var decision = intent.Decide(policy);

Console.WriteLine($"Intent: {intent.Name}");
Console.WriteLine($"Confidence: {intent.Confidence.Level} ({intent.Confidence.Score:F2})");
Console.WriteLine($"Decision: {decision}");
Console.WriteLine($"Reasoning: {intent.Reasoning}");
Console.WriteLine("\nDone. Run other examples: dotnet run --project examples/vector-normalization");
