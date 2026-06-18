using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Core.Intents;
using Intentum.Core.Models;
using Intentum.Runtime.Engine;
using Intentum.Runtime.Policy;

Console.WriteLine("=== Gaming Anti-Cheat Demo ===\n");

// Build rule-based detection model
var model = new RuleBasedIntentModel([
    AimbotRule(),
    SpeedHackRule(),
    WallhackRule(),
    NormalPlayRule()
]);

// Scenario 1: Legitimate player
Console.WriteLine("--- Scenario 1: Legitimate Player ---");
var normalSpace = new BehaviorSpace()
    .Observe("player", "headshot:0.45")
    .Observe("player", "tracking:smooth")
    .Observe("player", "reaction:250ms")
    .Observe("player", "movement:WASD");
var normalResult = model.Infer(normalSpace);
Console.WriteLine($"  Intent: {normalResult.Name}, confidence: {normalResult.Confidence.Score:F2}");

// Scenario 2: Aimbot
Console.WriteLine("\n--- Scenario 2: Aimbot Suspect ---");
var aimbotSpace = new BehaviorSpace()
    .Observe("player", "headshot:0.98")
    .Observe("player", "tracking:pixel_perfect")
    .Observe("player", "reaction:5ms")
    .Observe("player", "aim:snap_to_target");
var aimbotResult = model.Infer(aimbotSpace);
Console.WriteLine($"  Intent: {aimbotResult.Name}, confidence: {aimbotResult.Confidence.Score:F2}");

// Policy: ban aimbotters
var policy = new IntentPolicy()
    .AddRule(new PolicyRule("BanAimbot", i => i.Name == "Aimbot" && i.Confidence.Score > 0.7, PolicyDecision.Block))
    .AddRule(new PolicyRule("WarnSpeedHack", i => i.Name == "SpeedHack", PolicyDecision.Warn))
    .AddRule(new PolicyRule("AllowNormal", _ => true, PolicyDecision.Allow));

var aimbotDecision = IntentPolicyEngine.Evaluate(aimbotResult, policy);
Console.WriteLine($"  Policy decision: {aimbotDecision}");

var normalDecision = IntentPolicyEngine.Evaluate(normalResult, policy);
Console.WriteLine($"  Normal player decision: {normalDecision}");

static Func<BehaviorSpace, RuleMatch?> AimbotRule(double confidence = 0.95) => space =>
{
    var hsRate = ParseMetric(space, "headshot");
    var tracking = HasMetric(space, "tracking:pixel_perfect");
    var snapAim = HasMetric(space, "aim:snap_to_target");
    var reaction = ParseMetric(space, "reaction");
    if (hsRate > 0.8 && tracking && confidence > 0.8)
        return new RuleMatch("Aimbot", confidence, $"HS rate: {hsRate:F2}, snap aim: {snapAim}");
    return null;
};

static Func<BehaviorSpace, RuleMatch?> SpeedHackRule(double confidence = 0.9) => space =>
{
    var speed = HasMetric(space, "movement:teleport")
        || HasMetric(space, "speed:exceeds_max")
        || HasMetric(space, "movement:impossible");
    return speed ? new RuleMatch("SpeedHack", confidence, "Impossible movement detected") : null;
};

static Func<BehaviorSpace, RuleMatch?> WallhackRule(double confidence = 0.85) => space =>
{
    var tracking = HasMetric(space, "tracking:through_wall")
        || HasMetric(space, "aim:occluded_target");
    return tracking ? new RuleMatch("Wallhack", confidence, "Tracking through obstacles") : null;
};

static Func<BehaviorSpace, RuleMatch?> NormalPlayRule(double confidence = 0.5) => space =>
{
    var normalActions = space.Events.Count(e =>
        !e.Action.Contains("aimbot") && !e.Action.Contains("hack") && !e.Action.Contains("cheat"));
    return normalActions >= 2 ? new RuleMatch("NormalPlay", confidence, "Typical player behavior") : null;
};

static double ParseMetric(BehaviorSpace space, string prefix)
{
    var evt = space.Events.FirstOrDefault(e => e.Action.StartsWith(prefix));
    if (evt == null) return 0;
    var parts = evt.Action.Split(':');
    return parts.Length > 1 && double.TryParse(parts[1], out var val) ? val : 0;
};

static bool HasMetric(BehaviorSpace space, string action) =>
    space.Events.Any(e => e.Action.Contains(action, StringComparison.OrdinalIgnoreCase));
