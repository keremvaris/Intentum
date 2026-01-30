// Intentum Example: Chained Intent Model (Rule-based first, LLM fallback)
// Run: dotnet run --project examples/chained-intent
// No API key needed (uses RuleBasedIntentModel + Mock embedding for fallback).

using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Core.Models;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

// Primary: rule-based (fast, deterministic, explainable)
const string ActionLoginFailed = "login.failed";
var rules = new List<Func<BehaviorSpace, RuleMatch?>>
{
    space =>
    {
        var loginFails = space.Events.Count(e => e.Action == ActionLoginFailed);
        var hasReset = space.Events.Any(e => e.Action == "password.reset");
        var hasSuccess = space.Events.Any(e => e.Action == "login.success");
        if (loginFails >= 2 && hasReset && hasSuccess)
            return new RuleMatch("AccountRecovery", 0.85, $"{ActionLoginFailed}>=2 and password.reset and login.success");
        return null;
    },
    space =>
    {
        var loginFails = space.Events.Count(e => e.Action == ActionLoginFailed);
        var ipChanged = space.Events.Any(e => e.Action == "ip.changed");
        if (loginFails >= 3 && ipChanged)
            return new RuleMatch("SuspiciousAccess", 0.9, $"{ActionLoginFailed}>=3 and ip.changed");
        return null;
    }
};

var primary = new RuleBasedIntentModel(rules);

// Fallback: LLM (when no rule matches or confidence below threshold)
var fallback = new LlmIntentModel(
    new MockEmbeddingProvider(),
    new SimpleAverageSimilarityEngine());

// Chain: try primary first; if confidence < 0.7, use LLM
var intentModel = new ChainedIntentModel(primary, fallback, confidenceThreshold: 0.7);

var policy = new IntentPolicyBuilder()
    .Block("HighRisk", i => i.Confidence.Score > 0.65)
    .Observe("MediumRisk", i => i.Confidence.Score is > 0.4 and <= 0.65)
    .Allow("LowRisk", i => i.Confidence.Score <= 0.4)
    .Build();

Console.WriteLine("=== Intentum Example: Chained Intent (Rule → LLM Fallback) ===\n");

// Scenario 1: Rule matches (AccountRecovery) — no LLM call
var space1 = new BehaviorSpace()
    .Observe("user", ActionLoginFailed)
    .Observe("user", ActionLoginFailed)
    .Observe("user", "password.reset")
    .Observe("user", "login.success")
    .Observe("user", "device.verified");

var intent1 = intentModel.Infer(space1);
var decision1 = intent1.Decide(policy);

Console.WriteLine("Scenario 1 — Account recovery (rule matched, no LLM)");
Console.WriteLine($"  Intent: {intent1.Name}, Confidence: {intent1.Confidence.Level} ({intent1.Confidence.Score:F2})");
Console.WriteLine($"  Reasoning: {intent1.Reasoning}");
Console.WriteLine($"  Decision: {decision1}");
Console.WriteLine();

// Scenario 2: No rule matches — fallback to LLM
var space2 = new BehaviorSpace()
    .Observe("user", ActionLoginFailed)
    .Observe("user", "captcha.passed")
    .Observe("user", "login.retry");

var intent2 = intentModel.Infer(space2);
var decision2 = intent2.Decide(policy);

Console.WriteLine("Scenario 2 — Ambiguous (no rule matched → LLM fallback)");
Console.WriteLine($"  Intent: {intent2.Name}, Confidence: {intent2.Confidence.Level} ({intent2.Confidence.Score:F2})");
Console.WriteLine($"  Reasoning: {intent2.Reasoning}");
Console.WriteLine($"  Decision: {decision2}");
Console.WriteLine();

Console.WriteLine("Chained model reduces cost and latency by using rules first; LLM only when needed.");
