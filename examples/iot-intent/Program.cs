// Intentum Example: IoT Device Risk Intent Detection
// Run: dotnet run --project examples/iot-intent

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
    .Block("SecurityBreach", i => i.Confidence.Score > 0.7)
    .Warn("DeviceFailure", i => i.Confidence.Score is > 0.4 and <= 0.7)
    .Observe("Anomaly", i => i.Confidence.Score is > 0.2 and <= 0.4)
    .Allow("Normal", i => i.Confidence.Score <= 0.2)
    .Build();

Console.WriteLine("=== Intentum Example: IoT Device Risk ===\n");

// Scenario 1: Device failure imminent
var space1 = new BehaviorSpace()
    .Observe("sensor", "temperature.critical")
    .Observe("sensor", "battery.low")
    .Observe("device", "firmware.outdated")
    .Observe("device", "heartbeat.missed");

var intent1 = intentModel.Infer(space1);
var decision1 = IntentPolicyEngine.Evaluate(intent1, policy);

Console.WriteLine("Scenario 1 — Device failure imminent (temperature + battery + heartbeat)");
Console.WriteLine($"  Confidence: {intent1.Confidence.Level} (score: {intent1.Confidence.Score:F2})");
Console.WriteLine($"  Decision:   {decision1}");
Console.WriteLine();

// Scenario 2: Security breach attempt
var space2 = new BehaviorSpace()
    .Observe("device", "unauthorized.access")
    .Observe("device", "firmware.tampered")
    .Observe("network", "traffic.anomalous")
    .Observe("device", "privilege.escalation");

var intent2 = intentModel.Infer(space2);
var decision2 = IntentPolicyEngine.Evaluate(intent2, policy);

Console.WriteLine("Scenario 2 — Security breach attempt (unauthorized + tampered + anomalous)");
Console.WriteLine($"  Confidence: {intent2.Confidence.Level} (score: {intent2.Confidence.Score:F2})");
Console.WriteLine($"  Decision:   {decision2}");
