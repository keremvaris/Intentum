using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Core.Healthcare;

namespace Intentum.Tests;

public sealed class HealthcareRulesTests
{
    [Fact]
    public void PatientDeterioration_WithVitalAlertsAndRapidResponse_ReturnsMatch()
    {
        var rule = HealthcareRules.PatientDeterioration();
        var space = new BehaviorSpace()
            .Observe("nurse", "vital.signs.alert")
            .Observe("nurse", "rapid.response");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("PatientDeterioration", match.Name);
        Assert.Equal(0.9, match.Score);
    }

    [Fact]
    public void PatientDeterioration_WithNoAlerts_ReturnsNull()
    {
        var rule = HealthcareRules.PatientDeterioration();
        var space = new BehaviorSpace();

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void PatientDeterioration_WithMultipleAlertsNoRapidResponse_ReturnsLowerConfidence()
    {
        var rule = HealthcareRules.PatientDeterioration();
        var space = new BehaviorSpace()
            .Observe("nurse", "VitalAlert")
            .Observe("nurse", "VitalAlert");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("PatientDeterioration", match.Name);
        Assert.Equal(0.63, match.Score);
    }

    [Fact]
    public void MedicationConflict_WithMultipleOrders_ReturnsMatch()
    {
        var rule = HealthcareRules.MedicationConflict();
        var space = new BehaviorSpace()
            .Observe("doctor", "medication.ordered.warfarin")
            .Observe("doctor", "medication.ordered.aspirin");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("MedicationConflict", match.Name);
        Assert.Equal(0.85, match.Score);
    }

    [Fact]
    public void MedicationConflict_WithSingleOrder_ReturnsNull()
    {
        var rule = HealthcareRules.MedicationConflict();
        var space = new BehaviorSpace()
            .Observe("doctor", "medication.ordered.warfarin");

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void HospitalReadmissionRisk_WithDischargeAndSymptoms_ReturnsMatch()
    {
        var rule = HealthcareRules.HospitalReadmissionRisk();
        var space = new BehaviorSpace()
            .Observe("system", "discharge")
            .Observe("nurse", "symptom.report");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("HospitalReadmissionRisk", match.Name);
        Assert.Equal(0.8, match.Score);
    }

    [Fact]
    public void HospitalReadmissionRisk_WithoutDischarge_ReturnsNull()
    {
        var rule = HealthcareRules.HospitalReadmissionRisk();
        var space = new BehaviorSpace()
            .Observe("nurse", "symptom.report");

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void SepsisAlert_WithInfectionAndAbnormalVitals_ReturnsMatch()
    {
        var rule = HealthcareRules.SepsisAlert();
        var space = new BehaviorSpace()
            .Observe("lab", "infection.confirmed")
            .Observe("nurse", "vital.abnormal");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("SepsisAlert", match.Name);
        Assert.Equal(0.95, match.Score);
    }

    [Fact]
    public void SepsisAlert_WithoutInfection_ReturnsNull()
    {
        var rule = HealthcareRules.SepsisAlert();
        var space = new BehaviorSpace()
            .Observe("nurse", "vital.abnormal");

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void FallRisk_WithMultipleSignals_ReturnsMatch()
    {
        var rule = HealthcareRules.FallRisk();
        var space = new BehaviorSpace()
            .Observe("nurse", "mobility.issue")
            .Observe("nurse", "fall.incident");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("FallRisk", match.Name);
        Assert.Equal(0.8, match.Score);
    }

    [Fact]
    public void FallRisk_WithNoSignals_ReturnsNull()
    {
        var rule = HealthcareRules.FallRisk();
        var space = new BehaviorSpace();

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void AbnormalLabResult_WithCriticalLab_ReturnsMatch()
    {
        var rule = HealthcareRules.AbnormalLabResult();
        var space = new BehaviorSpace()
            .Observe("lab", "lab.critical");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("AbnormalLabResult", match.Name);
        Assert.Equal(0.9, match.Score);
    }

    [Fact]
    public void AbnormalLabResult_WithNoSignals_ReturnsNull()
    {
        var rule = HealthcareRules.AbnormalLabResult();
        var space = new BehaviorSpace();

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void TreatmentNonCompliance_WithMultipleSignals_ReturnsMatch()
    {
        var rule = HealthcareRules.TreatmentNonCompliance();
        var space = new BehaviorSpace()
            .Observe("system", "appointment.missed")
            .Observe("system", "medication.gap");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("TreatmentNonCompliance", match.Name);
        Assert.Equal(0.75, match.Score);
    }

    [Fact]
    public void TreatmentNonCompliance_WithNoSignals_ReturnsNull()
    {
        var rule = HealthcareRules.TreatmentNonCompliance();
        var space = new BehaviorSpace();

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void EmergencyTriage_WithMultipleSignals_ReturnsMatch()
    {
        var rule = HealthcareRules.EmergencyTriage();
        var space = new BehaviorSpace()
            .Observe("triage", "symptom.severe")
            .Observe("triage", "triage.urgent");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("EmergencyTriage", match.Name);
        Assert.Equal(0.85, match.Score);
    }

    [Fact]
    public void EmergencyTriage_WithNoSignals_ReturnsNull()
    {
        var rule = HealthcareRules.EmergencyTriage();
        var space = new BehaviorSpace();

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void AllRules_ReturnsEightRules()
    {
        var rules = HealthcareRules.AllRules();

        Assert.Equal(8, rules.Count);
    }
}
