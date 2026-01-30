# Clustering and Experiments: use cases

Short summary of **use cases** for **Intentum.Clustering** and **Intentum.Experiments**. Full API and setup: [Advanced Features](advanced-features.md#intent-clustering), [Advanced Features — A/B Experiments](advanced-features.md#ab-experiments).

---

## Intentum.Clustering

**What it does:** Groups intent history records (e.g. from `IIntentHistoryRepository`) into clusters by pattern (confidence level + decision) or by confidence score buckets.

**Use cases:**

- **Policy by cluster:** Analyze which clusters (e.g. "High + Allow", "Low + Block") appear most; tune policy rules or thresholds so that "Low + Block" stays within a target share.
- **Anomaly detection:** If a new cluster appears (e.g. many "Medium + Observe" in a short window), trigger alerting or review.
- **Dashboard:** Show distribution of intents by confidence band and decision (e.g. pie chart: Allow 60%, Observe 25%, Block 15%).

**How:** Register `AddIntentClustering()`, resolve `IIntentClusterer`, fetch records by time window, call `ClusterByPatternAsync` or `ClusterByConfidenceScoreAsync`. See [Intent Clustering](advanced-features.md#intent-clustering).

---

## Intentum.Experiments

**What it does:** A/B testing over intent inference — multiple variants (model + policy), traffic split (e.g. 50% / 50%), run behavior spaces through the experiment and get one result per space (variant name, intent, decision).

**Use cases:**

- **A/B intent model:** Compare two models (e.g. current LlmIntentModel vs new model or new embedding provider) on the same traffic; measure confidence and decision distribution per variant.
- **A/B policy:** Compare two policies (e.g. current vs stricter Block rules) on the same model output; measure Allow/Block/Observe per variant.
- **Rollout:** Ramp traffic to a new model or policy (e.g. 10% treatment, 90% control) and use experiment results to decide full rollout.

**How:** Build `IntentExperiment` with `AddVariant` (name, model, policy, traffic %), run `RunAsync(spaces)`. See [A/B Experiments](advanced-features.md#ab-experiments).

---

## Summary

| Package              | Use case summary |
|----------------------|-------------------|
| **Intentum.Clustering**  | Group intent history by pattern or score; policy tuning, anomaly detection, dashboards. |
| **Intentum.Experiments** | A/B test model or policy; compare variants, rollout new model/policy. |
