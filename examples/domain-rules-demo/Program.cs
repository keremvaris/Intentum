using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Core.Healthcare;
using Intentum.Core.Finance;
using Intentum.Core.IoT;
using Intentum.Core.Models;

Console.WriteLine("=== Domain Rules Demo ===\n");

Console.WriteLine("--- Healthcare: Sepsis Detection ---");
var sepsisRule = HealthcareRules.SepsisAlert();
var sepsisSpace = new BehaviorSpace()
    .Observe("lab", "infection.confirmed")
    .Observe("system", "vital.abnormal");
var sepsisMatch = sepsisRule(sepsisSpace);
Console.WriteLine(sepsisMatch != null
    ? $"  Match: {sepsisMatch.Name}, confidence: {sepsisMatch.Score:F2}"
    : "  No sepsis detected");

Console.WriteLine("\n--- Finance: Money Laundering Detection ---");
var mlRule = FinanceRules.MoneyLaunderingPattern();
var mlSpace = new BehaviorSpace()
    .Observe("customer", "transfer.rapid")
    .Observe("system", "structuring.detected")
    .Observe("system", "jurisdiction.high_risk");
var mlMatch = mlRule(mlSpace);
Console.WriteLine(mlMatch != null
    ? $"  Match: {mlMatch.Name}, confidence: {mlMatch.Score:F2}"
    : "  No suspicious pattern");

Console.WriteLine("\n--- IoT: Device Failure Detection ---");
var deviceRule = IoTRules.DeviceFailure();
var deviceSpace = new BehaviorSpace()
    .Observe("device", "error.connection_timeout")
    .Observe("device", "error.read_failure")
    .Observe("system", "telemetry.gap.detected");
var deviceMatch = deviceRule(deviceSpace);
Console.WriteLine(deviceMatch != null
    ? $"  Match: {deviceMatch.Name}, confidence: {deviceMatch.Score:F2}"
    : "  Device healthy");

Console.WriteLine("\n--- All Healthcare Rules via RuleBasedIntentModel ---");
var model = new RuleBasedIntentModel(HealthcareRules.AllRules());
var triageSpace = new BehaviorSpace()
    .Observe("nurse", "vital.signs.alert")
    .Observe("system", "rapid.response.triggered")
    .Observe("lab", "infection.confirmed")
    .Observe("system", "vital.abnormal");
var intent = model.Infer(triageSpace);
Console.WriteLine($"  Inferred: {intent.Name}, confidence: {intent.Confidence.Score:F2}, level: {intent.Confidence.Level}");
