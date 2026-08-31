// Intentum Example: Education Risk Intent Detection
// Run: dotnet run --project examples/education-intent

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
    .Escalate("Critical", i => i.Confidence.Score > 0.7)
    .Warn("AtRisk", i => i.Confidence.Score is > 0.4 and <= 0.7)
    .Observe("Watch", i => i.Confidence.Score is > 0.2 and <= 0.4)
    .Allow("Normal", i => i.Confidence.Score <= 0.2)
    .Build();

Console.WriteLine("=== Intentum Example: Education Risk ===\n");

var space1 = new BehaviorSpace()
    .Observe("student", "attendance.declined")
    .Observe("student", "grades.dropped")
    .Observe("student", "assignment.missed")
    .Observe("student", "engagement.low");

var intent1 = intentModel.Infer(space1);
var decision1 = IntentPolicyEngine.Evaluate(intent1, policy);

Console.WriteLine("Scenario 1 — At-risk student (attendance + grades + engagement)");
Console.WriteLine($"  Confidence: {intent1.Confidence.Level} (score: {intent1.Confidence.Score:F2})");
Console.WriteLine($"  Decision:   {decision1}");
Console.WriteLine();

var space2 = new BehaviorSpace()
    .Observe("student", "submission.similar")
    .Observe("student", "source.unattributed")
    .Observe("student", "timing.anomalous");

var intent2 = intentModel.Infer(space2);
var decision2 = IntentPolicyEngine.Evaluate(intent2, policy);

Console.WriteLine("Scenario 2 — Academic integrity concern (similarity + timing)");
Console.WriteLine($"  Confidence: {intent2.Confidence.Level} (score: {intent2.Confidence.Score:F2})");
Console.WriteLine($"  Decision:   {decision2}");