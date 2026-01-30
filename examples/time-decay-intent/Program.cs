// Intentum Example: Time Decay in Intent Inference
// Run: dotnet run --project examples/time-decay-intent
// Uses TimeDecaySimilarityEngine so recent events weigh more than old ones.

using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core.Behavior;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

// Time decay: events from 1 hour ago have half the weight (half-life = 1 hour)
var halfLife = TimeSpan.FromHours(1);
var referenceTime = DateTimeOffset.UtcNow;
var engine = new TimeDecaySimilarityEngine(halfLife, referenceTime);

var intentModel = new LlmIntentModel(
    new MockEmbeddingProvider(),
    engine);

// LlmIntentModel detects ITimeAwareSimilarityEngine and calls CalculateIntentScoreWithTimeDecay(behaviorSpace, embeddings)
// so "5 min ago login.failed" weighs more than "1 hour ago login.failed"

var policy = new IntentPolicyBuilder()
    .Observe("MediumRisk", i => i.Confidence.Score is > 0.4 and <= 0.7)
    .Allow("LowRisk", i => i.Confidence.Score <= 0.4)
    .Build();

Console.WriteLine("=== Intentum Example: Time Decay ===\n");

// Scenario: same events but different timestamps — recent ones matter more
var oneHourAgo = referenceTime - TimeSpan.FromHours(1);
var fiveMinAgo = referenceTime - TimeSpan.FromMinutes(5);

var space = new BehaviorSpace();
// Old event (will be decayed)
space.Observe(new BehaviorEvent("user", "login.failed", oneHourAgo));
space.Observe(new BehaviorEvent("user", "login.failed", oneHourAgo));
// Recent events (full weight)
space.Observe(new BehaviorEvent("user", "login.retry", fiveMinAgo));
space.Observe(new BehaviorEvent("user", "captcha.passed", fiveMinAgo));

var intent = intentModel.Infer(space);
var decision = intent.Decide(policy);

Console.WriteLine("Events: 2× login.failed (1h ago), login.retry + captcha.passed (5 min ago)");
Console.WriteLine($"With time decay (half-life 1h), recent events weigh more.");
Console.WriteLine($"  Intent: {intent.Name}, Confidence: {intent.Confidence.Level} ({intent.Confidence.Score:F2})");
Console.WriteLine($"  Decision: {decision}");
Console.WriteLine();
Console.WriteLine("In fraud/risk scenarios, 'login fail 5 min ago' vs '1 hour ago' should not weigh the same; TimeDecaySimilarityEngine does that.");
