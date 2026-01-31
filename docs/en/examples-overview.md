# Examples Overview — Simple, Medium, Hard

This page groups Intentum **examples** and **samples** by difficulty and ties them to real-life use cases. All examples run without an API key (Mock provider) unless noted.

---

## Difficulty levels

| Level | Description |
|-------|-------------|
| **Simple** | Single concept, minimal code, run in a few minutes. Ideal for "what is intent inference?" |
| **Medium** | Combines 2–3 concepts (e.g. rules + LLM, time decay, normalization). Good for production-like flows. |
| **Hard** | Full pipeline, domain-specific (fraud, greenwashing, customer journey), or multi-stage / analytics. |

---

## Simple examples

### [vector-normalization](https://github.com/keremvaris/Intentum/tree/master/examples/vector-normalization)

**Concept:** Behavior vector normalization (Cap, L1, SoftCap) so repeated events don’t dominate the score.

**Real-life:** When one action (e.g. `user:click`) appears 100 times, raw counts can skew intent; normalization keeps the signal balanced.

```bash
dotnet run --project examples/vector-normalization
```

---

### [time-decay-intent](https://github.com/keremvaris/Intentum/tree/master/examples/time-decay-intent)

**Concept:** Recent events weigh more than old ones (`TimeDecaySimilarityEngine` with half-life).

**Real-life:** Session-based intent: "what did the user do in the last 5 minutes?" matters more than an action from an hour ago.

```bash
dotnet run --project examples/time-decay-intent
```

---

## Medium examples

### [chained-intent](https://github.com/keremvaris/Intentum/tree/master/examples/chained-intent)

**Concept:** Rule-based model first; if confidence is below a threshold, fall back to LLM (`ChainedIntentModel`). Reduces cost and keeps high-confidence cases deterministic.

**Real-life:** Most traffic hits clear rules (e.g. "login failed 3x + IP change → Suspicious"); only ambiguous cases go to the LLM.

```bash
dotnet run --project examples/chained-intent
```

---

### [ai-fallback-intent](https://github.com/keremvaris/Intentum/tree/master/examples/ai-fallback-intent)

**Concept:** Infer whether the AI "rushed" (PrematureClassification) or was "careful" (CarefulUnderstanding); policy routes to human or auto.

**Real-life:** Quality gate: low-confidence or contradictory signals → escalate to human review.

```bash
dotnet run --project examples/ai-fallback-intent
```

---

## Hard examples (domain + full pipeline)

### [fraud-intent](https://github.com/keremvaris/Intentum/tree/master/examples/fraud-intent)

**Concept:** Fraud/abuse intent: login failures, IP change, retries, captcha, password reset → infer SuspiciousAccess vs AccountRecovery; policy Block / Observe / Allow.

**Real-life:** Login flow: after N failures and IP change, decide whether to block, step-up auth, or allow.

```bash
dotnet run --project examples/fraud-intent
```

**Docs:** [Real-world scenarios — Fraud / Abuse](real-world-scenarios.md), [Domain intent templates — Fraud](domain-intent-templates.md#fraud--security).

---

### [customer-intent](https://github.com/keremvaris/Intentum/tree/master/examples/customer-intent)

**Concept:** Customer intent (purchase, info gathering, support) from browse, cart, checkout, search, FAQ, contact; policy Allow / Observe / route by intent.

**Real-life:** E-commerce or support: route the user to the right flow (checkout vs FAQ vs contact) from behavior.

```bash
dotnet run --project examples/customer-intent
```

**Docs:** [Domain intent templates — Customer](domain-intent-templates.md#customer-intent).

---

### [greenwashing-intent](https://github.com/keremvaris/Intentum/tree/master/examples/greenwashing-intent)

**Concept:** Greenwashing detection from report text and signals; policy for ESG/claims. Can use real provider for semantic analysis.

**Real-life:** Analyze sustainability reports or claims; flag low-confidence or misleading intent for review.

```bash
dotnet run --project examples/greenwashing-intent
```

**Docs:** [Greenwashing detection how-to](greenwashing-detection-howto.md), [Case study — Greenwashing metrics](../case-studies/greenwashing-metrics.md).

---

## Samples (full applications)

| Sample | Description | Difficulty |
|--------|-------------|------------|
| [Intentum.Sample](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample) | Console: many scenarios (payment, ESG, compliance, retries) in one app. | Medium |
| [Intentum.Sample.Web](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample.Web) | CQRS Web API: infer, explain, explain-tree, analytics, timeline, playground compare, greenwashing. | Hard |

Run the web sample:

```bash
dotnet run --project samples/Intentum.Sample.Web
```

Then open the UI and Scalar API docs; try `POST /api/intent/infer`, `POST /api/intent/explain-tree`, `GET /api/intent/analytics/timeline/{entityId}`, `POST /api/intent/playground/compare`.

---

## Quick reference

| Example | Level | Run |
|---------|--------|-----|
| vector-normalization | Simple | `dotnet run --project examples/vector-normalization` |
| time-decay-intent | Simple | `dotnet run --project examples/time-decay-intent` |
| chained-intent | Medium | `dotnet run --project examples/chained-intent` |
| ai-fallback-intent | Medium | `dotnet run --project examples/ai-fallback-intent` |
| fraud-intent | Hard | `dotnet run --project examples/fraud-intent` |
| customer-intent | Hard | `dotnet run --project examples/customer-intent` |
| greenwashing-intent | Hard | `dotnet run --project examples/greenwashing-intent` |
| Sample.Web | Hard | `dotnet run --project samples/Intentum.Sample.Web` |

No API keys are required for the examples above; they use the Mock provider. For real AI, set environment variables and use a provider — see [Providers](providers.md) and [AI providers how-to](ai-providers-howto.md).
