# Intentum examples

Minimal runnable examples for real-world use cases. Each example focuses on **one** scenario (fraud intent, AI fallback) and uses the Mock provider so you can run without an API key.

**Samples vs examples**

- **`samples/`** — Full showcase: [Intentum.Sample](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample) runs many scenarios (payment, ESG, compliance, retries) in one console app; [Intentum.Sample.Web](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample.Web) is a CQRS API with intent infer, analytics, and a web UI. Use these to see the full pipeline and all features.
- **`examples/`** — Single-use-case, copy-paste friendly projects (fraud-intent, ai-fallback-intent). Use these to learn one scenario or to copy into your repo.

| Example | Description |
|--------|-------------|
| [fraud-intent](fraud-intent/) | Fraud / abuse intent detection: infer risk from login failures, IP change, retries, captcha, password reset; policy → StepUpAuth / Allow / Monitor. |
| [ai-fallback-intent](ai-fallback-intent/) | AI decision fallback: infer whether the model rushed (PrematureClassification) or was careful (CarefulUnderstanding); policy → RouteToHuman / AllowAutoDecision. |

## Run

From the repo root:

```bash
dotnet run --project examples/fraud-intent
dotnet run --project examples/ai-fallback-intent
```

No API keys required; both use the Mock embedding provider.

## Documentation

- [Real-world scenarios](https://github.com/keremvaris/Intentum/blob/master/docs/en/real-world-scenarios.md) — full narrative and code for fraud and AI fallback.
- [Getting Started](https://github.com/keremvaris/Intentum#getting-started-5-minutes) — 5-minute README guide.
