using Intentum.Core.Behavior;
using Intentum.Core.Models;

namespace Intentum.Core.Healthcare;

public static class HealthcareRules
{
    public static Func<BehaviorSpace, RuleMatch?> PatientDeterioration(
        double confidence = 0.9) => space =>
    {
        var vitalAlerts = space.Events.Count(e =>
            e.Action.Contains("vital.signs.alert", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("VitalAlert", StringComparison.OrdinalIgnoreCase));
        var rapidResponse = space.Events.Any(e =>
            e.Action.Contains("rapid.response", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("RapidResponse", StringComparison.OrdinalIgnoreCase));

        if (vitalAlerts >= 1 && rapidResponse)
            return new RuleMatch("PatientDeterioration", confidence,
                $"Vital alerts: {vitalAlerts}, rapid response: true");
        if (vitalAlerts >= 2)
            return new RuleMatch("PatientDeterioration", confidence * 0.7,
                $"Multiple vital alerts: {vitalAlerts}, no rapid response");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> MedicationConflict(
        double confidence = 0.85) => space =>
    {
        var medicationOrders = space.Events.Count(e =>
            e.Action.StartsWith("medication.ordered", StringComparison.OrdinalIgnoreCase));

        if (medicationOrders >= 2)
            return new RuleMatch("MedicationConflict", confidence,
                $"Distinct medication orders: {medicationOrders}");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> HospitalReadmissionRisk(
        double confidence = 0.8) => space =>
    {
        var discharged = space.Events.Any(e =>
            e.Action.Contains("discharge", StringComparison.OrdinalIgnoreCase));
        var symptoms = space.Events.Count(e =>
            e.Action.Contains("symptom.report", StringComparison.OrdinalIgnoreCase));
        var followUp = space.Events.Any(e =>
            e.Action.Contains("followup.visit", StringComparison.OrdinalIgnoreCase));

        if (discharged && (symptoms >= 1 || followUp))
            return new RuleMatch("HospitalReadmissionRisk", confidence,
                $"Discharged: true, symptoms: {symptoms}, follow-up: {followUp}");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> SepsisAlert(
        double confidence = 0.95) => space =>
    {
        var infectionConfirmed = space.Events.Any(e =>
            e.Action.Contains("infection.confirmed", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("InfectionConfirmed", StringComparison.OrdinalIgnoreCase));
        var abnormalVitals = space.Events.Count(e =>
            e.Action.Contains("vital.abnormal", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("AbnormalVital", StringComparison.OrdinalIgnoreCase));

        if (infectionConfirmed && abnormalVitals >= 1)
            return new RuleMatch("SepsisAlert", confidence,
                $"Infection confirmed: true, abnormal vitals: {abnormalVitals}");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> FallRisk(
        double confidence = 0.8) => space =>
    {
        var signals = space.Events.Count(e =>
            e.Action.Contains("mobility.issue", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("age.fall.risk", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("fall.incident", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("balance.problem", StringComparison.OrdinalIgnoreCase));

        if (signals >= 2)
            return new RuleMatch("FallRisk", confidence,
                $"Fall risk signals: {signals}");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> AbnormalLabResult(
        double confidence = 0.9) => space =>
    {
        var signals = space.Events.Count(e =>
            e.Action.Contains("lab.critical", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("lab.trend.abnormal", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("lab.result.abnormal", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("lab.panic.value", StringComparison.OrdinalIgnoreCase));

        if (signals >= 1)
            return new RuleMatch("AbnormalLabResult", confidence,
                $"Abnormal lab signals: {signals}");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> TreatmentNonCompliance(
        double confidence = 0.75) => space =>
    {
        var signals = space.Events.Count(e =>
            e.Action.Contains("appointment.missed", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("medication.gap", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("treatment.refused", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("followup.missed", StringComparison.OrdinalIgnoreCase));

        if (signals >= 2)
            return new RuleMatch("TreatmentNonCompliance", confidence,
                $"Compliance signals: {signals}");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> EmergencyTriage(
        double confidence = 0.85) => space =>
    {
        var signals = space.Events.Count(e =>
            e.Action.Contains("symptom.severe", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("triage.urgent", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("wait.time.critical", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("resource.shortage", StringComparison.OrdinalIgnoreCase));

        if (signals >= 2)
            return new RuleMatch("EmergencyTriage", confidence,
                $"Triage signals: {signals}");
        return null;
    };

    public static IReadOnlyList<Func<BehaviorSpace, RuleMatch?>> AllRules() =>
    [
        SepsisAlert(),
        PatientDeterioration(),
        MedicationConflict(),
        HospitalReadmissionRisk(),
        FallRisk(),
        AbnormalLabResult(),
        TreatmentNonCompliance(),
        EmergencyTriage()
    ];
}
