# Roadmap

**Why you're reading this page:** This page summarizes Intentum's roadmap: v1.0 criteria, next steps, and longer-term goals. It is the right place if you are curious about project direction or where to contribute.

Intentum's direction: adoption first, then depth.

---

## v1.0 criteria

- Core flow: Observe → Infer → Decide (BehaviorSpace, IIntentModel, IntentPolicy).
- Packages: Core, Runtime, AI, AI providers (OpenAI, Gemini, Claude, Mistral, Azure), Testing, AspNetCore, Observability, Logging, Persistence, Analytics.
- Documentation: Why Intentum, Manifesto, Canon, Real-world scenarios, Designing intent models, Why Intent ≠ Logs.
- Examples: fraud-intent, ai-fallback-intent.
- CI: build, test, coverage, SonarCloud.

---

## After v1.0: adoption (A)

- "Getting Started in 5 minutes" — done in README.
- 2–3 real use-cases — fraud, AI fallback in docs and examples.
- Minimal template repo — dotnet new / CodeGen.
- Community: HN / Reddit / X share when ready.

---

## After v1.0: depth (B)

- Intent scoring strategies and confidence calibration.
- Richer AI adapters and hybrid models.
- Academic-grade docs and design notes.

---

## Recent additions (post–v1.0)

Already implemented and documented:

- **Intent Timeline** — Entity-scoped intent history over time; `GetIntentTimelineAsync`, Sample: `GET /api/intent/analytics/timeline/{entityId}`.
- **Intent Tree** — Decision tree explainability; `IIntentTreeExplainer`, Sample: `POST /api/intent/explain-tree`.
- **Context-Aware Policy** — Policy rules with context (load, region, recent intents); `ContextAwarePolicyEngine`, `intent.Decide(context, policy)`.
- **Policy Store** — Declarative JSON policy with hot-reload; `IPolicyStore`, `FilePolicyStore` (Intentum.Runtime.PolicyStore).
- **Behavior Pattern Detector** — Patterns and anomalies in intent history; `IBehaviorPatternDetector`.
- **Multi-Stage Intent Model** — Chain models with confidence thresholds; `MultiStageIntentModel`.
- **Scenario Runner** — Run scenarios through model + policy; `IScenarioRunner`, `IntentScenarioRunner`.
- **Real-time stream** — `IBehaviorStreamConsumer`, `MemoryBehaviorStreamConsumer`; Worker template uses it.
- **OpenTelemetry tracing** — Spans for infer and policy.evaluate; `IntentumActivitySource`.
- **Playground** — Compare models via `POST /api/intent/playground/compare`.
- **dotnet new templates** — `intentum-webapi`, `intentum-backgroundservice`, `intentum-function`.

See [Advanced Features](advanced-features.md), [Setup – Create from template](setup.md#create-from-template-dotnet-new), and [API Reference](api.md).

**Next step:** When you're done with this page → [Advanced features](advanced-features.md) or [Setup](setup.md).

---
