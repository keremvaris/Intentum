# Intentum examples

Minimal runnable examples for real-world use cases. Each example focuses on **one** scenario (fraud intent, AI fallback) and uses the Mock provider so you can run without an API key.

**Samples vs examples**

- **`samples/`** — Full showcase: [Intentum.Sample](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample) runs many scenarios (payment, ESG, compliance, retries) in one console app; [Intentum.Sample.Web](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample.Web) is a CQRS API with intent infer, analytics, and a web UI. Use these to see the full pipeline and all features.
- **`examples/`** — Single-use-case, copy-paste friendly projects. Use these to learn one scenario or to copy into your repo.

| Example | Description |
|--------|-------------|
| [fraud-intent](fraud-intent/) | Fraud / abuse intent detection: infer risk from login failures, IP change, retries, captcha, password reset; policy → StepUpAuth / Allow / Monitor. |
| [ai-fallback-intent](ai-fallback-intent/) | AI decision fallback: infer whether the model rushed (PrematureClassification) or was careful (CarefulUnderstanding); policy → RouteToHuman / AllowAutoDecision. |
| [chained-intent](chained-intent/) | Chained intent: rule-based first, LLM fallback when no rule matches or confidence below threshold; shows Reasoning. |
| [time-decay-intent](time-decay-intent/) | Time decay: recent events weigh more (TimeDecaySimilarityEngine with LlmIntentModel). |
| [vector-normalization](vector-normalization/) | Behavior vector normalization: Cap, L1, SoftCap so repeated events don't dominate. |

## Run

From the repo root:

```bash
dotnet run --project examples/fraud-intent
dotnet run --project examples/ai-fallback-intent
dotnet run --project examples/chained-intent
dotnet run --project examples/time-decay-intent
dotnet run --project examples/vector-normalization
```

No API keys required for fraud, ai-fallback, chained, time-decay, or vector-normalization (Mock or Core only).

## Documentation

- [Real-world scenarios](https://github.com/keremvaris/Intentum/blob/master/docs/en/real-world-scenarios.md) — full narrative and code for fraud, AI fallback, and chained intent.
- [Advanced features](https://github.com/keremvaris/Intentum/blob/master/docs/en/advanced-features.md) — time decay, normalization, rule-based and chained models.
- [Getting Started](https://github.com/keremvaris/Intentum#getting-started-5-minutes) — 5-minute README guide.
