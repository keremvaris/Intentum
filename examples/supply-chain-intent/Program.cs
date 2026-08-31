// Intentum Example: Supply Chain Risk Intent Detection
// Run: dotnet run --project examples/supply-chain-intent
// No API key needed (uses Mock embedding provider).

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
    .Block("CriticalRisk", i => i.Confidence.Score > 0.7)
    .Warn("HighRisk", i => i.Confidence.Score is > 0.5 and <= 0.7)
    .Observe("MediumRisk", i => i.Confidence.Score is > 0.3 and <= 0.5)
    .Allow("LowRisk", i => i.Confidence.Score <= 0.3)
    .Build();

Console.WriteLine("=== Intentum Example: Supply Chain Risk ===\n");

// Scenario 1: Stock depletion risk
var space1 = new BehaviorSpace()
    .Observe("warehouse", "inventory.low")
    .Observe("warehouse", "stockout.imminent")
    .Observe("supplier", "delivery.delayed")
    .Observe("supplier", "quality.rejected");

var intent1 = intentModel.Infer(space1);
var decision1 = IntentPolicyEngine.Evaluate(intent1, policy);

Console.WriteLine("Scenario 1 — Stock depletion risk (low inventory + delayed delivery)");
Console.WriteLine($"  Confidence: {intent1.Confidence.Level} (score: {intent1.Confidence.Score:F2})");
Console.WriteLine($"  Decision:   {decision1}");
Console.WriteLine();

// Scenario 2: Supplier reliability issue
var space2 = new BehaviorSpace()
    .Observe("supplier", "contract.breach")
    .Observe("supplier", "communication.loss")
    .Observe("logistics", "route.disrupted");

var intent2 = intentModel.Infer(space2);
var decision2 = IntentPolicyEngine.Evaluate(intent2, policy);

Console.WriteLine("Scenario 2 — Supplier reliability issue (contract breach + communication loss)");
Console.WriteLine($"  Confidence: {intent2.Confidence.Level} (score: {intent2.Confidence.Score:F2})");
Console.WriteLine($"  Decision:   {decision2}");
Console.WriteLine();

Console.WriteLine("Intentum treats supply chain anomalies as signals, not failures.");
