// Intentum Example: AI Decision Fallback & Validation
// Run: dotnet run --project examples/ai-fallback-intent
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

// Policy: high confidence + many "rushed" signals → RouteToHuman; moderate + careful signals → Allow
var policy = new IntentPolicyBuilder()
    .Block("PrematureClassification", i => i.Confidence.Score > 0.7 && i.Signals.Count >= 4)
    .Allow("CarefulUnderstanding", i => i.Confidence.Score >= 0.7 && i.Signals.Count <= 3)
    .Observe("Uncertain", i => i.Confidence.Score is > 0.4 and < 0.7)
    .Allow("LowRisk", i => i.Confidence.Score <= 0.4)
    .Build();

Console.WriteLine("=== Intentum Example: AI Decision Fallback ===\n");

// Scenario 1: Model rushed (high confidence, short reasoning, no follow-up, user rephrased, model changed answer)
var space1 = new BehaviorSpace()
    .Observe("llm", "high_confidence")
    .Observe("llm", "short_reasoning")
    .Observe("llm", "no_followup_question")
    .Observe("user", "rephrased_request")
    .Observe("llm", "changed_answer");

var intent1 = intentModel.Infer(space1);
var decision1 = intent1.Decide(policy);

Console.WriteLine("Scenario 1 — Premature classification (rushed, user rephrased, model backtracked)");
Console.WriteLine($"  Confidence: {intent1.Confidence.Level} (score: {intent1.Confidence.Score:F2})");
Console.WriteLine($"  Decision:   {decision1}");
Console.WriteLine();

// Scenario 2: Careful understanding (clarifying question, user details, explicit reasoning, moderate confidence)
var space2 = new BehaviorSpace()
    .Observe("llm", "asked_clarifying_question")
    .Observe("user", "provided_details")
    .Observe("llm", "reasoning_explicit")
    .Observe("llm", "moderate_confidence");

var intent2 = intentModel.Infer(space2);
var decision2 = intent2.Decide(policy);

Console.WriteLine("Scenario 2 — Careful understanding (clarifying question + explicit reasoning)");
Console.WriteLine($"  Confidence: {intent2.Confidence.Level} (score: {intent2.Confidence.Score:F2})");
Console.WriteLine($"  Decision:   {decision2}");
Console.WriteLine();

Console.WriteLine("Intentum does not say the model is 'wrong'; it acts on intent. RouteToHuman / AllowAutoDecision based on confidence and signals.");
