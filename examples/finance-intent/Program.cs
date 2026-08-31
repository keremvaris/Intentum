// Intentum Example: Finance Risk Intent Detection
// Run: dotnet run --project examples/finance-intent

using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Runtime.Engine;
using Intentum.Runtime.Policy;

var intentModel = new LlmIntentModel(
    new MockEmbeddingProvider(),
    new SimpleAverageSimilarityEngine());

var policy = new IntentPolicyBuilder()
    .Block("HighRisk", i => i.Confidence.Score > 0.7)
    .Warn("ElevatedRisk", i => i.Confidence.Score is > 0.45 and <= 0.7)
    .Observe("Monitor", i => i.Confidence.Score is > 0.25 and <= 0.45)
    .Allow("Normal", i => i.Confidence.Score <= 0.25)
    .Build();

Console.WriteLine("=== Intentum Example: Finance Risk ===\n");

// Scenario 1: Credit risk escalation
var space1 = new BehaviorSpace()
    .Observe("account", "payment.missed")
    .Observe("account", "balance.negative")
    .Observe("account", "credit.utilization.high")
    .Observe("account", "inquiry.multiple");

var intent1 = intentModel.Infer(space1);
var decision1 = IntentPolicyEngine.Evaluate(intent1, policy);

Console.WriteLine("Scenario 1 — Credit risk escalation (missed payment + high utilization)");
Console.WriteLine($"  Confidence: {intent1.Confidence.Level} (score: {intent1.Confidence.Score:F2})");
Console.WriteLine($"  Decision:   {decision1}");
Console.WriteLine();

// Scenario 2: Market volatility response
var space2 = new BehaviorSpace()
    .Observe("market", "volatility.spike")
    .Observe("portfolio", "exposure.high")
    .Observe("portfolio", "hedging.insufficient");

var intent2 = intentModel.Infer(space2);
var decision2 = IntentPolicyEngine.Evaluate(intent2, policy);

Console.WriteLine("Scenario 2 — Market volatility (volatility spike + high exposure)");
Console.WriteLine($"  Confidence: {intent2.Confidence.Level} (score: {intent2.Confidence.Score:F2})");
Console.WriteLine($"  Decision:   {decision2}");