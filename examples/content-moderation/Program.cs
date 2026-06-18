using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Core.Intents;
using Intentum.Core.Models;
using Intentum.Runtime.Policy;
using Intentum.Runtime.Engine;

Console.WriteLine("=== Content Moderation Demo ===\n");

var model = new RuleBasedIntentModel([
    ToxicContent(),
    Harassment(),
    SpamContent(),
    AcceptableContent()
]);

// Scenario 1: Healthy discussion
Console.WriteLine("--- Scenario 1: Healthy Discussion ---");
var healthySpace = new BehaviorSpace()
    .Observe("user", "post:constructive_feedback")
    .Observe("mod", "report:none")
    .Observe("user", "reply:on_topic");
var healthyIntent = model.Infer(healthySpace);
Console.WriteLine($"  Intent: {healthyIntent.Name}, confidence: {healthyIntent.Confidence.Score:F2}");

// Scenario 2: Toxic content
Console.WriteLine("\n--- Scenario 2: Toxic Content ---");
var toxicSpace = new BehaviorSpace()
    .Observe("user", "post:hate_speech")
    .Observe("user", "post:personal_attack")
    .Observe("mod", "report:multiple")
    .Observe("user", "reply:inflammatory");
var toxicIntent = model.Infer(toxicSpace);
Console.WriteLine($"  Intent: {toxicIntent.Name}, confidence: {toxicIntent.Confidence.Score:F2}");

// Policy
var policy = new IntentPolicy()
    .AddRule(new PolicyRule("BlockToxic",
        i => i.Name == "ToxicContent" && i.Confidence.Score > 0.7,
        PolicyDecision.Block))
    .AddRule(new PolicyRule("EscalateHarassment",
        i => i.Name == "Harassment",
        PolicyDecision.Escalate))
    .AddRule(new PolicyRule("FlagSpam",
        i => i.Name == "SpamContent" && i.Confidence.Score > 0.6,
        PolicyDecision.Observe))
    .AddRule(new PolicyRule("AllowHealthy",
        _ => true, PolicyDecision.Allow));

var toxicDecision = IntentPolicyEngine.Evaluate(toxicIntent, policy);
Console.WriteLine($"  Toxic decision: {toxicDecision}");

var healthyDecision = IntentPolicyEngine.Evaluate(healthyIntent, policy);
Console.WriteLine($"  Healthy decision: {healthyDecision}");

static Func<BehaviorSpace, RuleMatch?> ToxicContent(double confidence = 0.9) => space =>
{
    var signals = space.Events.Count(e =>
        e.Action.Contains("hate_speech") || e.Action.Contains("personal_attack") ||
        e.Action.Contains("inflammatory") || e.Action.Contains("threat"));
    return signals >= 2 ? new RuleMatch("ToxicContent", confidence, $"Toxic signals: {signals}") : null;
};

static Func<BehaviorSpace, RuleMatch?> Harassment(double confidence = 0.95) => space =>
{
    var repeated = space.Events.Count(e =>
        e.Action.Contains("personal_attack") || e.Action.Contains("threat"));
    var reports = space.Events.Any(e => e.Action.Contains("report:multiple"));
    return repeated >= 3 && reports
        ? new RuleMatch("Harassment", confidence, $"Repeated harassment ({repeated}x), multiple reports")
        : null;
};

static Func<BehaviorSpace, RuleMatch?> SpamContent(double confidence = 0.8) => space =>
{
    var spamSignals = space.Events.Count(e =>
        e.Action.Contains("spam") || e.Action.Contains("scam") || e.Action.Contains("misleading"));
    return spamSignals >= 2
        ? new RuleMatch("SpamContent", confidence, $"Spam signals: {spamSignals}")
        : null;
};

static Func<BehaviorSpace, RuleMatch?> AcceptableContent(double confidence = 0.5) => space =>
    space.Events.Any() ? new RuleMatch("Acceptable", confidence, "Content within guidelines") : null;
