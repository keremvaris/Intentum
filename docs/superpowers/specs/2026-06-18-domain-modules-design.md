# Domain Modules Design Spec

## Goal

Add 5 domain-specific rule modules (Healthcare, Education, IoT, Finance, Supply Chain) to enable domain-aware intent inference without LLM dependency.

## Context

Intentum already has domain rule modules for Commerce (`CommerceRules.cs`, 4 rules), Fraud (`FraudRules.cs`, 4 rules), and User Behavior Analytics (`UserBehaviorRules.cs`, 3 rules). These follow a proven pattern: static class with factory methods returning `Func<BehaviorSpace, RuleMatch?>`. New domains will follow the same pattern.

## Architecture

```
src/Intentum.Core/Healthcare/HealthcareRules.cs   — 8 rules
src/Intentum.Core/Education/EducationRules.cs      — 8 rules
src/Intentum.Core/IoT/IoTRules.cs                  — 8 rules
src/Intentum.Core/Finance/FinanceRules.cs           — 8 rules
src/Intentum.Core/SupplyChain/SupplyChainRules.cs   — 8 rules
```

Each module:
- Static class in `Intentum.Core.{Domain}` namespace
- Factory methods with configurable parameters (confidence, thresholds)
- Returns `Func<BehaviorSpace, RuleMatch?>`
- `AllRules()` returns `IReadOnlyList<Func<BehaviorSpace, RuleMatch?>>`
- Action matching uses `StringComparison.OrdinalIgnoreCase`

## Domain Rules

### Healthcare (8 rules)
| Rule | Signals | Confidence |
|------|---------|------------|
| PatientDeterioration | vital alerts, rapid response triggered | 0.9 |
| MedicationConflict | overlapping medication orders | 0.85 |
| HospitalReadmissionRisk | discharge, symptom reports, follow-up visits | 0.8 |
| SepsisAlert | infection confirmed, abnormal vitals | 0.95 |
| FallRisk | mobility flags, age alerts, previous falls | 0.8 |
| AbnormalLabResult | critical values, trend warnings | 0.9 |
| TreatmentNonCompliance | missed appointments, medication gaps | 0.75 |
| EmergencyTriage | symptom severity, wait time, resource flags | 0.85 |

### Education (8 rules)
| Rule | Signals | Confidence |
|------|---------|------------|
| StudentDropoutRisk | declining engagement, missed assignments, attendance | 0.85 |
| AcademicIntegrityViolation | rapid submissions, off-hours access | 0.9 |
| CourseRecommendation | subject browsing, prerequisite completion | 0.75 |
| LearningResourceNeeds | repeated help requests | 0.8 |
| EarlyWarning | grade drops, behavioral flags | 0.85 |
| InterventionRequired | sustained low performance, resistance | 0.9 |
| GiftedStudent | advanced completion, enrichment requests | 0.7 |
| CareerPathInterest | course selection, extracurricular focus | 0.75 |

### IoT (8 rules)
| Rule | Signals | Confidence |
|------|---------|------------|
| DeviceFailure | errors, telemetry gaps | 0.9 |
| SecurityBreach | unauthorized access, location anomalies | 0.95 |
| MaintenanceRequired | performance degradation, threshold exceeded | 0.85 |
| AnomalousSensorReading | outliers, rapid changes | 0.8 |
| FirmwareOutdated | version mismatch, update failures | 0.8 |
| NetworkCongestion | latency spikes, packet loss | 0.85 |
| PowerFluctuation | voltage anomalies, battery drain | 0.8 |
| ConfigurationDrift | setting changes, compliance violations | 0.85 |

### Finance (8 rules)
| Rule | Signals | Confidence |
|------|---------|------------|
| MoneyLaunderingPattern | rapid transfers, structuring, high-risk jurisdictions | 0.9 |
| UnauthorizedAccess | unusual login times, new devices | 0.85 |
| HighValueTransaction | large amounts, unusual recipients | 0.75 |
| AccountCompromise | password changes, profile updates, suspicious logins | 0.95 |
| InsiderTrading | unusual trading patterns, pre-announcement activity | 0.9 |
| CreditFraud | rapid applications, identity mismatches | 0.85 |
| WireFraud | unusual wire patterns, beneficiary changes | 0.9 |
| ComplianceViolation | regulatory flags, reporting gaps | 0.85 |

### Supply Chain (8 rules)
| Rule | Signals | Confidence |
|------|---------|------------|
| InventoryShortage | low stock, demand spikes, reorder failures | 0.85 |
| SupplierRisk | delivery delays, quality issues | 0.8 |
| LogisticsDisruption | route changes, carrier issues | 0.9 |
| DemandForecastAnomaly | unexpected patterns, seasonal deviations | 0.75 |
| WarehouseCapacity | utilization spikes, throughput issues | 0.8 |
| OrderFulfillmentDelay | processing delays, pick/pack issues | 0.85 |
| SupplierDependencyRisk | single-source exposure, geopolitical flags | 0.8 |
| ReturnsAnomaly | unusual return rates, pattern changes | 0.8 |

## Testing Strategy

Each domain gets a dedicated test class. Each rule gets:
- 1 positive test (rule matches expected inputs)
- 1 negative test (rule returns null for non-matching inputs)

Plus:
- Threshold parameter tests (configurable min values)
- `AllRules()` count test

~14-16 tests per domain, ~70-80 total.

### Test Pattern (following CommerceRulesTests.cs)

```csharp
[Fact]
public void RuleName_WithCondition_ReturnsMatch()
{
    var rule = DomainRules.RuleName();
    var space = new BehaviorSpace()
        .Observe("actor", "signal.one")
        .Observe("actor", "signal.two");
    var match = rule(space);
    Assert.NotNull(match);
    Assert.Equal("RuleName", match.Name);
}

[Fact]
public void RuleName_WithoutCondition_ReturnsNull()
{
    var rule = DomainRules.RuleName();
    var space = new BehaviorSpace().Observe("actor", "normal.action");
    var match = rule(space);
    Assert.Null(match);
}
```

## Files

### New Source Files
- `src/Intentum.Core/Healthcare/HealthcareRules.cs`
- `src/Intentum.Core/Education/EducationRules.cs`
- `src/Intentum.Core/IoT/IoTRules.cs`
- `src/Intentum.Core/Finance/FinanceRules.cs`
- `src/Intentum.Core/SupplyChain/SupplyChainRules.cs`

### New Test Files
- `tests/Intentum.Tests/HealthcareRulesTests.cs`
- `tests/Intentum.Tests/EducationRulesTests.cs`
- `tests/Intentum.Tests/IoTRulesTests.cs`
- `tests/Intentum.Tests/FinanceRulesTests.cs`
- `tests/Intentum.Tests/SupplyChainRulesTests.cs`

## Acceptance Criteria

1. All 40 rules return correct `RuleMatch` for matching behavior spaces
2. All rules return `null` for non-matching behavior spaces
3. `AllRules()` returns correct count for each domain
4. All 70+ tests pass
5. Build produces 0 warnings, 0 errors
6. All existing tests still pass
