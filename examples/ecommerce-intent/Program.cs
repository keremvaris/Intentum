// Intentum Example: E-commerce Risk Intent Detection
// Run: dotnet run --project examples/ecommerce-intent

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
    .Block("Fraudulent", i => i.Confidence.Score > 0.7)
    .Warn("Suspicious", i => i.Confidence.Score is > 0.4 and <= 0.7)
    .Observe("Watch", i => i.Confidence.Score is > 0.2 and <= 0.4)
    .Allow("Normal", i => i.Confidence.Score <= 0.2)
    .Build();

Console.WriteLine("=== Intentum Example: E-commerce Risk ===\n");

// Scenario 1: Fake order attempt
var space1 = new BehaviorSpace()
    .Observe("user", "account.created.recent")
    .Observe("user", "address.mismatch")
    .Observe("order", "high.value")
    .Observe("payment", "card.declined")
    .Observe("user", "vpn.detected");

var intent1 = intentModel.Infer(space1);
var decision1 = IntentPolicyEngine.Evaluate(intent1, policy);

Console.WriteLine("Scenario 1 — Fake order attempt (new account + address mismatch + VPN)");
Console.WriteLine($"  Confidence: {intent1.Confidence.Level} (score: {intent1.Confidence.Score:F2})");
Console.WriteLine($"  Decision:   {decision1}");
Console.WriteLine();

// Scenario 2: Cart abandonment risk
var space2 = new BehaviorSpace()
    .Observe("user", "cart.abandoned")
    .Observe("user", "browse.exit")
    .Observe("user", "price.comparison");

var intent2 = intentModel.Infer(space2);
var decision2 = IntentPolicyEngine.Evaluate(intent2, policy);

Console.WriteLine("Scenario 2 — Cart abandonment risk (abandoned + exit + price comparison)");
Console.WriteLine($"  Confidence: {intent2.Confidence.Level} (score: {intent2.Confidence.Score:F2})");
Console.WriteLine($"  Decision:   {decision2}");
