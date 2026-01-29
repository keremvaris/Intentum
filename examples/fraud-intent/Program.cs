// Intentum Example: Fraud / Abuse Intent Detection
// Run: dotnet run --project examples/fraud-intent
// No API key needed (uses Mock embedding provider).

using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

var intentModel = new LlmIntentModel(
    new MockEmbeddingProvider(),
    new SimpleAverageSimilarityEngine());

// Policy: map confidence and signals to decisions (conceptually: StepUpAuth, Allow, Monitor)
var policy = new IntentPolicyBuilder()
    .Block("HighRisk", i => i.Confidence.Score > 0.65 && i.Signals.Count >= 4)
    .Observe("MediumRisk", i => i.Confidence.Score is > 0.4 and <= 0.65)
    .Allow("LowRisk", i => i.Confidence.Score <= 0.4)
    .Build();

Console.WriteLine("=== Intentum Example: Fraud / Abuse Intent ===\n");

// Scenario 1: Suspicious access (failed logins, IP change, retry, captcha)
var space1 = new BehaviorSpace()
    .Observe("user", "login.failed")
    .Observe("user", "login.failed")
    .Observe("user", "ip.changed")
    .Observe("user", "login.retry")
    .Observe("user", "captcha.passed");

var intent1 = intentModel.Infer(space1);
var decision1 = intent1.Decide(policy);

Console.WriteLine("Scenario 1 — Suspicious access (failed logins + IP change + captcha)");
Console.WriteLine($"  Confidence: {intent1.Confidence.Level} (score: {intent1.Confidence.Score:F2})");
Console.WriteLine($"  Decision:   {decision1}");
Console.WriteLine();

// Scenario 2: Account recovery (failed, password reset, success)
var space2 = new BehaviorSpace()
    .Observe("user", "login.failed")
    .Observe("user", "password.reset")
    .Observe("user", "login.success")
    .Observe("user", "device.verified");

var intent2 = intentModel.Infer(space2);
var decision2 = intent2.Decide(policy);

Console.WriteLine("Scenario 2 — Account recovery (reset + success + device verified)");
Console.WriteLine($"  Confidence: {intent2.Confidence.Level} (score: {intent2.Confidence.Score:F2})");
Console.WriteLine($"  Decision:   {decision2}");
Console.WriteLine();

Console.WriteLine("Intentum does not block; it feeds the decision. Use confidence + signals to StepUpAuth(), Allow(), or Monitor().");
