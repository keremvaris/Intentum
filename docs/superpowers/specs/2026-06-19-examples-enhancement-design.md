# Examples & Samples Enhancement Design Spec

## Goal

Add 8 new examples covering: (a) new feature demonstrations (Resilience, Domain Rules, Calibration/Ensemble, gRPC), and (b) viral-potential domain demos (Gaming Anti-Cheat, AI Agent Monitor, Healthcare Triage, Content Moderation).

## Architecture

All new examples follow the existing `examples/` pattern: console apps, .NET 10.0, Mock AI (no API key needed), Observe → Infer → Decide pipeline.

```
examples/
├── resilience-demo/           — Circuit Breaker, Retry, Bulkhead, Degradation, Timeout
├── domain-rules-demo/         — Healthcare, Finance, IoT, Education, Supply Chain rules
├── calibration-ensemble-demo/ — Platt, Temperature, Weighted/MajorityVoting ensemble
├── grpc-client/               — gRPC Infer + Evaluate calls
├── gaming-anti-cheat/         — Aimbot, speed hack, wallhack detection
├── agent-monitor/             — AI agent decision monitoring
├── healthcare-triage/         — Sepsis, fall risk, medication conflict triage
└── content-moderation/        — Toxic content, policy violation detection
```

## Examples

### Feature Examples

| Example | Libraries | Key Types | Appeal |
|---------|-----------|-----------|--------|
| resilience-demo | Intentum.Runtime.Resilience | ICircuitBreaker, IRetryPolicy, IBulkhead | Production devs |
| domain-rules-demo | Intentum.Core.{Domain} | HealthcareRules, FinanceRules, IoTRules | Domain experts |
| calibration-ensemble-demo | Intentum.AI.Calibration, .Ensemble | PlattCalibrator, EnsembleIntentModel | ML engineers |
| grpc-client | Intentum.Grpc | IntentumServiceClient | .NET devs |

### Domain Examples

| Example | Libraries | Key Types | Appeal |
|---------|-----------|-----------|--------|
| gaming-anti-cheat | Intentum.Core, .Runtime | BehaviorSpace, IntentPolicy | Gamers, viral |
| agent-monitor | Intentum.Core, .Runtime, .AI | IIntentModel, ChainedIntentModel | AI/Agent devs |
| healthcare-triage | Intentum.Core.Healthcare | HealthcareRules, RuleBasedIntentModel | Healthcare IT |
| content-moderation | Intentum.Core, .Runtime | BehaviorSpace, IntentPolicy | Social platforms |

## Acceptance Criteria

1. All 8 examples build and run with `dotnet run` (no API keys)
2. Each example demonstrates its core concept clearly
3. All examples use Mock AI (no external dependencies)
4. All existing tests still pass

## Files

### New Project Files (8 csproj + 8 Program.cs = 16 files)
- `examples/resilience-demo/` — 2 files
- `examples/domain-rules-demo/` — 2 files
- `examples/calibration-ensemble-demo/` — 2 files
- `examples/grpc-client/` — 2 files
- `examples/gaming-anti-cheat/` — 2 files
- `examples/agent-monitor/` — 2 files
- `examples/healthcare-triage/` — 2 files
- `examples/content-moderation/` — 2 files
