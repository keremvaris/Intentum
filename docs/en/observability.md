# Observability

Intentum exposes **OpenTelemetry metrics** for intent inference and policy decisions. You can export them to Grafana, Prometheus, or any OTLP-compatible backend.

## Intentum.Observability

The **Intentum.Observability** package provides:

- **IntentumMetrics** — static methods to record:
  - `intentum.intent.inference.count` (counter) — number of inferences; tags: `confidence.level`, `signal.count`
  - `intentum.intent.inference.duration` (histogram, ms) — inference duration
  - `intentum.intent.confidence.score` (histogram) — confidence score
  - `intentum.policy.decision.count` (counter) — policy decisions; tag: `decision` (Allow, Block, Observe, etc.)
  - `intentum.behavior.space.size` (histogram) — behavior space event count
- **ObservablePolicyEngine.DecideWithMetrics** — extension: `intent.DecideWithMetrics(policy)` records the decision and returns the same result as `intent.Decide(policy)`.
- **ObservablePolicyEngine.DecideWithExecutionLog** — extension: `(decision, record) = intent.DecideWithExecutionLog(policy)` returns a **PolicyExecutionRecord** (IntentName, MatchedRuleName, Decision, DurationMs, Success, ExceptionMessage, ExceptionTrace) for logging. When evaluation throws, the record has Success = false and ExceptionMessage/ExceptionTrace; the method returns (Observe, record) so you can log the failure trace.

## How to use

1. **Add the package:** `Intentum.Observability`.
2. **Record inference:** After `model.Infer(space)`, call `IntentumMetrics.RecordIntentInference(intent, duration)` and optionally `IntentumMetrics.RecordBehaviorSpaceSize(space)`.
3. **Record decisions:** Use `intent.DecideWithMetrics(policy)` instead of `intent.Decide(policy)` so each decision is recorded.
4. **Export metrics:** Configure OpenTelemetry to export the default meter provider to your backend (e.g. OTLP exporter to Grafana Agent or Prometheus). Ensure the "Intentum" meter is included.

Example (pseudo-code):

```csharp
// After inference
var sw = Stopwatch.StartNew();
var intent = model.Infer(space);
sw.Stop();
IntentumMetrics.RecordIntentInference(intent, sw.Elapsed);
IntentumMetrics.RecordBehaviorSpaceSize(space);

// Policy decision (metrics only)
var decision = intent.DecideWithMetrics(policy);

// Policy decision with execution log (for logging matched rule, intent, decision, duration; on failure, record has ExceptionMessage and ExceptionTrace)
var (decision2, record) = intent.DecideWithExecutionLog(policy);
logger.LogInformation("Policy: {Intent} -> {Decision} (rule: {Rule}, {Duration}ms)", record.IntentName, record.Decision, record.MatchedRuleName, record.DurationMs);
if (!record.Success)
    logger.LogError("Policy evaluation failed: {Message}\n{Trace}", record.ExceptionMessage, record.ExceptionTrace);
```

## Example dashboard (Grafana)

Suggested panels when exporting to Grafana (or any Prometheus-compatible backend):

| Panel            | Metric / query idea |
|------------------|----------------------|
| Inferences/s     | rate(intentum_intent_inference_count_total[5m]) |
| Inference p95    | histogram_quantile(0.95, rate(intentum_intent_inference_duration_bucket[5m])) |
| Confidence avg   | rate(intentum_intent_confidence_score_sum[5m]) / rate(intentum_intent_confidence_score_count[5m]) |
| Decisions by type| sum by (decision) (rate(intentum_policy_decision_count_total[5m])) |
| Behavior space size | histogram_quantile(0.95, rate(intentum_behavior_space_size_bucket[5m])) |

*(Exact metric names depend on your OTLP/Prometheus exporter; adjust to match the names emitted by the OpenTelemetry SDK.)*

## OpenTelemetry tracing

The **Intentum.Observability** package also exposes **OpenTelemetry spans** via **IntentumActivitySource**:

- **intentum.infer** — Span for each intent inference; tags: `intentum.intent.name`, `intentum.intent.confidence.level`, `intentum.intent.confidence.score`, `intentum.intent.signal.count`, `intentum.behavior.event.count`, `intentum.behavior.signal_summary` (truncated list of actor:action for signal→intent correlation).
- **intentum.policy.evaluate** — Span for each policy evaluation; tags: `intentum.policy.decision`, `intentum.intent.name`, `intentum.intent.confidence.level`, `intentum.policy.matched_rule`.

When you use **ObservableIntentModel** and **DecideWithMetrics**, these spans are emitted automatically. Configure your OpenTelemetry TracerProvider to add the Intentum activity source (`IntentumActivitySource.Source.Name`) so traces appear in Jaeger, Zipkin, or any OTLP backend. Export: use the standard OTLP trace exporter in your tracer provider configuration.

---

## Summary

| What              | Where |
|-------------------|--------|
| Metrics           | **Intentum.Observability** — IntentumMetrics, ObservablePolicyEngine |
| Tracing           | **IntentumActivitySource** — intentum.infer, intentum.policy.evaluate spans |
| Export            | OpenTelemetry SDK → OTLP exporter → Grafana Agent / Prometheus / etc. |
| Dashboard         | Inferences/s, inference duration, confidence, decisions by type |

For health checks (embedding provider, policy engine), see [Intentum.AspNetCore](api.md) health checks.
