using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Core.Intents;
using Intentum.Core.Models;
using Intentum.Runtime.Engine;
using Intentum.Runtime.Policy;

Console.WriteLine("=== AI Agent Monitor Demo ===\n");

var model = new RuleBasedIntentModel([
    AgentHallucination(),
    AgentToolAbuse(),
    AgentEfficient(),
    AgentNormal()
]);

// Scenario 1: Healthy agent
Console.WriteLine("--- Scenario 1: Healthy Agent ---");
var healthySpace = new BehaviorSpace()
    .Observe("agent", "tool:calculator")
    .Observe("agent", "reasoning:clear")
    .Observe("agent", "tool:search")
    .Observe("agent", "response:accurate");
var healthyResult = model.Infer(healthySpace);
Console.WriteLine($"  Intent: {healthyResult.Name}, confidence: {healthyResult.Confidence.Score:F2}");

// Scenario 2: Hallucinating agent
Console.WriteLine("\n--- Scenario 2: Hallucinating Agent ---");
var hallucinationSpace = new BehaviorSpace()
    .Observe("agent", "tool:search")
    .Observe("agent", "reasoning:contradictory")
    .Observe("agent", "response:unsupported_claim")
    .Observe("agent", "tool:none_used");
var hallResult = model.Infer(hallucinationSpace);
Console.WriteLine($"  Intent: {hallResult.Name}, confidence: {hallResult.Confidence.Score:F2}");

// Policy: intervene on issues
var policy = new IntentPolicy()
    .AddRule(new PolicyRule("EscalateHallucination",
        i => i.Name == "AgentHallucination" && i.Confidence.Score > 0.6,
        PolicyDecision.Escalate))
    .AddRule(new PolicyRule("WarnToolAbuse",
        i => i.Name == "AgentToolAbuse",
        PolicyDecision.Warn))
    .AddRule(new PolicyRule("AllowNormal",
        _ => true, PolicyDecision.Allow));

var hallDecision = IntentPolicyEngine.Evaluate(hallResult, policy);
Console.WriteLine($"  Hallucination decision: {hallDecision}");

var healthyDecision = IntentPolicyEngine.Evaluate(healthyResult, policy);
Console.WriteLine($"  Healthy decision: {healthyDecision}");

static Func<BehaviorSpace, RuleMatch?> AgentHallucination(double confidence = 0.9) => space =>
{
    var contradictory = space.Events.Any(e =>
        e.Action.Contains("contradictory", StringComparison.OrdinalIgnoreCase));
    var unsupported = space.Events.Any(e =>
        e.Action.Contains("unsupported_claim", StringComparison.OrdinalIgnoreCase));
    var noTools = space.Events.Any(e =>
        e.Action.Contains("tool:none_used", StringComparison.OrdinalIgnoreCase));
    return (contradictory && unsupported) || (unsupported && noTools)
        ? new RuleMatch("AgentHallucination", confidence, "Likely hallucination detected")
        : null;
};

static Func<BehaviorSpace, RuleMatch?> AgentToolAbuse(double confidence = 0.85) => space =>
{
    var toolCalls = space.Events.Count(e => e.Action.StartsWith("tool:", StringComparison.OrdinalIgnoreCase));
    var noReasoning = !space.Events.Any(e =>
        e.Action.StartsWith("reasoning:", StringComparison.OrdinalIgnoreCase));
    return toolCalls >= 5 && noReasoning
        ? new RuleMatch("AgentToolAbuse", confidence, $"Excessive tool calls ({toolCalls}) without reasoning")
        : null;
};

static Func<BehaviorSpace, RuleMatch?> AgentEfficient(double confidence = 0.8) => space =>
{
    var toolsUsed = space.Events.Count(e => e.Action.StartsWith("tool:", StringComparison.OrdinalIgnoreCase));
    var accuracy = space.Events.Any(e => e.Action.Contains("accurate", StringComparison.OrdinalIgnoreCase));
    return toolsUsed <= 2 && accuracy
        ? new RuleMatch("AgentEfficient", confidence, "Minimal tool use, accurate results")
        : null;
};

static Func<BehaviorSpace, RuleMatch?> AgentNormal(double confidence = 0.5) => space =>
    space.Events.Any()
        ? new RuleMatch("AgentNormal", confidence, "Standard agent behavior")
        : null;
