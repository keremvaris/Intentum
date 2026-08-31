// Intentum Community Example Template
// Copy this template and customize for your use case.
// Run: dotnet run --project examples/community/template

using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Runtime.Engine;
using Intentum.Runtime.Policy;

// 1. Create intent model with Mock provider
var intentModel = new LlmIntentModel(
    new MockEmbeddingProvider(),
    new SimpleAverageSimilarityEngine());

// 2. Define your policy
var policy = new IntentPolicyBuilder()
    .Block("HighRisk", i => i.Confidence.Score > 0.7)
    .Warn("MediumRisk", i => i.Confidence.Score is > 0.4 and <= 0.7)
    .Allow("LowRisk", i => i.Confidence.Score <= 0.4)
    .Build();

Console.WriteLine("=== Intentum Community Example ===\n");

// 3. Observe behavior events
var space = new BehaviorSpace()
    .Observe("actor1", "action1")
    .Observe("actor1", "action2");

// 4. Infer intent
var intent = intentModel.Infer(space);

// 5. Apply policy
var decision = IntentPolicyEngine.Evaluate(intent, policy);

// 6. Output results
Console.WriteLine($"Intent:     {intent.Name}");
Console.WriteLine($"Confidence: {intent.Confidence.Level} (score: {intent.Confidence.Score:F2})");
Console.WriteLine($"Decision:   {decision}");
