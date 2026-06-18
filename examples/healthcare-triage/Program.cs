using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Core.Healthcare;
using Intentum.Core.Models;
using Intentum.Runtime.Engine;
using Intentum.Runtime.Policy;

Console.WriteLine("=== Healthcare Triage Demo ===\n");

var model = new RuleBasedIntentModel(HealthcareRules.AllRules());

// Scenario 1: Sepsis emergency
Console.WriteLine("--- Scenario 1: Sepsis Emergency ---");
var sepsisSpace = new BehaviorSpace()
    .Observe("lab", "infection.confirmed")
    .Observe("nurse", "vital.signs.alert")
    .Observe("system", "vital.signs.abnormal")
    .Observe("system", "rapid.response.triggered");
var sepsisIntent = model.Infer(sepsisSpace);
Console.WriteLine($"  Intent: {sepsisIntent.Name}, confidence: {sepsisIntent.Confidence.Score:F2}");

// Scenario 2: Patient deterioration
Console.WriteLine("\n--- Scenario 2: Patient Deterioration ---");
var detSpace = new BehaviorSpace()
    .Observe("nurse", "vital.signs.alert")
    .Observe("nurse", "VitalAlert")
    .Observe("system", "rapid.response.triggered");
var detIntent = model.Infer(detSpace);
Console.WriteLine($"  Intent: {detIntent.Name}, confidence: {detIntent.Confidence.Score:F2}");

// Scenario 3: Medication conflict
Console.WriteLine("\n--- Scenario 3: Medication Conflict ---");
var medSpace = new BehaviorSpace()
    .Observe("pharmacist", "medication.ordered.warfarin")
    .Observe("pharmacist", "medication.ordered.aspirin");
var medIntent = model.Infer(medSpace);
Console.WriteLine($"  Intent: {medIntent.Name}, confidence: {medIntent.Confidence.Score:F2}");

// Policy mapping
var policy = new IntentPolicy()
    .AddRule(new PolicyRule("Emergency",
        i => i.Name == "SepsisAlert" && i.Confidence.Level == "Certain",
        PolicyDecision.RequireAuth))
    .AddRule(new PolicyRule("UrgentReview",
        i => i.Name == "PatientDeterioration",
        PolicyDecision.Escalate))
    .AddRule(new PolicyRule("AlertPharmacist",
        i => i.Name == "MedicationConflict",
        PolicyDecision.Warn));

var sepsisDecision = IntentPolicyEngine.Evaluate(sepsisIntent, policy);
Console.WriteLine($"  Sepsis policy: {sepsisDecision}");

var medDecision = IntentPolicyEngine.Evaluate(medIntent, policy);
Console.WriteLine($"  Medication conflict policy: {medDecision}");
