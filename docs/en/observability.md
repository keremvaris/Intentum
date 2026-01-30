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

// Policy decision
var decision = intent.DecideWithMetrics(policy);
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

## Summary

| What              | Where |
|-------------------|--------|
| Metrics           | **Intentum.Observability** — IntentumMetrics, ObservablePolicyEngine |
| Export            | OpenTelemetry SDK → OTLP exporter → Grafana Agent / Prometheus / etc. |
| Dashboard         | Inferences/s, inference duration, confidence, decisions by type |

For health checks (embedding provider, policy engine), see [Intentum.AspNetCore](api.md) health checks.
