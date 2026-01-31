# Gözlemlenebilirlik

Intentum, intent inference ve politika kararları için **OpenTelemetry metrikleri** sunar. Bunları Grafana, Prometheus veya herhangi bir OTLP uyumlu backend'e aktarabilirsiniz.

## Intentum.Observability

**Intentum.Observability** paketi şunları sağlar:

- **IntentumMetrics** — kayıt için statik metotlar:
  - `intentum.intent.inference.count` (sayaç) — inference sayısı; etiketler: `confidence.level`, `signal.count`
  - `intentum.intent.inference.duration` (histogram, ms) — inference süresi
  - `intentum.intent.confidence.score` (histogram) — güven skoru
  - `intentum.policy.decision.count` (sayaç) — politika kararları; etiket: `decision` (Allow, Block, Observe vb.)
  - `intentum.behavior.space.size` (histogram) — davranış uzayı olay sayısı
- **ObservablePolicyEngine.DecideWithMetrics** — uzantı: `intent.DecideWithMetrics(policy)` kararı kaydeder ve `intent.Decide(policy)` ile aynı sonucu döndürür.

## Nasıl kullanılır

1. **Paketi ekleyin:** `Intentum.Observability`.
2. **Inference kaydı:** `model.Infer(space)` sonrası `IntentumMetrics.RecordIntentInference(intent, duration)` ve isteğe bağlı `IntentumMetrics.RecordBehaviorSpaceSize(space)` çağırın.
3. **Karar kaydı:** Her karar kaydedilsin diye `intent.Decide(policy)` yerine `intent.DecideWithMetrics(policy)` kullanın.
4. **Metrik dışa aktarma:** OpenTelemetry'yi varsayılan meter provider'ı backend'inize (örn. Grafana Agent veya Prometheus'a OTLP exporter) aktaracak şekilde yapılandırın. "Intentum" meter'ın dahil olduğundan emin olun.

Örnek (sözde kod):

```csharp
// Inference sonrası
var sw = Stopwatch.StartNew();
var intent = model.Infer(space);
sw.Stop();
IntentumMetrics.RecordIntentInference(intent, sw.Elapsed);
IntentumMetrics.RecordBehaviorSpaceSize(space);

// Politika kararı
var decision = intent.DecideWithMetrics(policy);
```

## Örnek dashboard (Grafana)

Grafana'ya (veya Prometheus uyumlu herhangi bir backend'e) aktarırken önerilen paneller:

| Panel            | Metrik / sorgu fikri |
|------------------|------------------------|
| Inference/s      | rate(intentum_intent_inference_count_total[5m]) |
| Inference p95    | histogram_quantile(0.95, rate(intentum_intent_inference_duration_bucket[5m])) |
| Güven ort.       | rate(intentum_intent_confidence_score_sum[5m]) / rate(intentum_intent_confidence_score_count[5m]) |
| Karar türüne göre | sum by (decision) (rate(intentum_policy_decision_count_total[5m])) |
| Davranış uzayı boyutu | histogram_quantile(0.95, rate(intentum_behavior_space_size_bucket[5m])) |

*(Tam metrik adları OTLP/Prometheus exporter'ınıza bağlıdır; OpenTelemetry SDK'nın ürettiği adlara göre uyarlayın.)*

## OpenTelemetry tracing

**Intentum.Observability** paketi **IntentumActivitySource** ile **OpenTelemetry span’leri** de sunar:

- **intentum.infer** — Her intent inference için span; etiketler: intent adı, güven düzeyi/skoru, sinyal sayısı, davranış olay sayısı.
- **intentum.policy.evaluate** — Her policy değerlendirmesi için span; etiketler: policy kararı, eşleşen kural adı.

**ObservableIntentModel** ve **DecideWithMetrics** kullandığınızda bu span’ler otomatik üretilir. Jaeger, Zipkin veya herhangi bir OTLP backend’de trace’lerin görünmesi için OpenTelemetry TracerProvider’a Intentum activity source’u ekleyin.

---

## Özet

| Ne               | Nerede |
|------------------|--------|
| Metrikler        | **Intentum.Observability** — IntentumMetrics, ObservablePolicyEngine |
| Tracing          | **IntentumActivitySource** — intentum.infer, intentum.policy.evaluate span’leri |
| Dışa aktarma     | OpenTelemetry SDK → OTLP exporter → Grafana Agent / Prometheus / vb. |
| Dashboard        | Inference/s, inference süresi, güven, türe göre kararlar |

Sağlık kontrolleri (embedding sağlayıcı, politika motoru) için [Intentum.AspNetCore](api.md) health check'lerine bakın.
