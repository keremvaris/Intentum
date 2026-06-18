# Intentum examples

Minimal runnable examples for real-world use cases. Each example focuses on **one** scenario (fraud intent, AI fallback) and uses the Mock provider so you can run without an API key.

**Samples vs examples**

- **`samples/`** — Full showcase: [Intentum.Sample](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample) runs many scenarios (payment, ESG, compliance, retries) in one console app; [Intentum.Sample.Web](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample.Web) is a CQRS API with intent infer, analytics, and a web UI. Use these to see the full pipeline and all features.
- **`examples/`** — Single-use-case, copy-paste friendly projects. Use these to learn one scenario or to copy into your repo.

| Example | Description |
|--------|-------------|
| [hello-intentum](hello-intentum/) | **5-minute quick start:** one signal, one intent, console output. Minimal "Hello Intentum". |
| [fraud-intent](fraud-intent/) | Fraud / abuse intent detection: infer risk from login failures, IP change, retries, captcha, password reset; policy → StepUpAuth / Allow / Monitor. See [Domain intent templates — Fraud](docs/en/domain-intent-templates.md#fraud--security). |
| [customer-intent](customer-intent/) | Customer intent (purchase, info gathering, support): infer from browse, cart, checkout, search, FAQ, contact; policy → Allow / Observe / route by intent. See [Domain intent templates — Customer](docs/en/domain-intent-templates.md#customer-intent). |
| [greenwashing-intent](greenwashing-intent/) | Greenwashing detection: infer from report text and signals; policy for ESG/claims. See [Greenwashing detection how-to](docs/en/greenwashing-detection-howto.md). |
| [ai-fallback-intent](ai-fallback-intent/) | AI decision fallback: infer whether the model rushed (PrematureClassification) or was careful (CarefulUnderstanding); policy → RouteToHuman / AllowAutoDecision. |
| [chained-intent](chained-intent/) | Chained intent: rule-based first, LLM fallback when no rule matches or confidence below threshold; shows Reasoning. |
| [time-decay-intent](time-decay-intent/) | Time decay: recent events weigh more (TimeDecaySimilarityEngine with LlmIntentModel). |
| [vector-normalization](vector-normalization/) | Behavior vector normalization: Cap, L1, SoftCap so repeated events don't dominate. |
| [resilience-demo](resilience-demo/) | **New:** Circuit Breaker, Retry Policy, Bulkhead patterns in action. |
| [domain-rules-demo](domain-rules-demo/) | **New:** Healthcare, Finance, IoT domain rule evaluation with RuleBasedIntentModel. |
| [calibration-ensemble-demo](calibration-ensemble-demo/) | **New:** Platt/Temperature calibration + Weighted/MajorityVoting ensemble. |
| [gaming-anti-cheat](gaming-anti-cheat/) | **New:** Aimbot, speed hack, wallhack detection for online games. |
| [agent-monitor](agent-monitor/) | **New:** AI agent hallucination, tool abuse, and efficiency monitoring. |
| [healthcare-triage](healthcare-triage/) | **New:** Sepsis alert, patient deterioration, medication conflict triage. |
| [content-moderation](content-moderation/) | **New:** Toxic content, harassment, and spam detection for social platforms. |
| [grpc-client](grpc-client/) | **New:** gRPC client calling Infer and Evaluate endpoints. |

## Run

From the repo root:

```bash
dotnet run --project examples/hello-intentum
dotnet run --project examples/fraud-intent
dotnet run --project examples/customer-intent
dotnet run --project examples/greenwashing-intent
dotnet run --project examples/ai-fallback-intent
dotnet run --project examples/chained-intent
dotnet run --project examples/time-decay-intent
dotnet run --project examples/vector-normalization
dotnet run --project examples/resilience-demo
dotnet run --project examples/domain-rules-demo
dotnet run --project examples/calibration-ensemble-demo
dotnet run --project examples/gaming-anti-cheat
dotnet run --project examples/agent-monitor
dotnet run --project examples/healthcare-triage
dotnet run --project examples/content-moderation
dotnet run --project examples/grpc-client
```

No API keys required for these examples (Mock or Core only).

## Documentation

- [Real-world scenarios](https://github.com/keremvaris/Intentum/blob/master/docs/en/real-world-scenarios.md) — full narrative and code for fraud, AI fallback, and chained intent.
- [Advanced features](https://github.com/keremvaris/Intentum/blob/master/docs/en/advanced-features.md) — time decay, normalization, rule-based and chained models.
- [Getting Started](https://github.com/keremvaris/Intentum#getting-started-5-minutes) — 5-minute README guide.
