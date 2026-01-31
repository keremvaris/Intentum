# Intentum Documentation (EN)

Welcome to Intentum. This documentation helps you install, configure, and use Intentum in your project.

---

## What is Intentum?

Intentum is a **intent-driven** approach for systems where behavior is not fully deterministic: instead of asserting fixed scenario steps (like classic BDD), you **observe** what happened, **infer** the user’s or system’s intent (with optional AI embeddings), and **decide** what to do (Allow, Observe, Warn, Block) via policy rules.

- **Behavior Space** — You record events (e.g. login, retry, submit). No rigid “Given/When/Then” steps.
- **Intent** — A model infers intent and confidence (High / Medium / Low) from that behavior.
- **Policy** — Rules map intent to decisions: e.g. “high confidence → Allow”, “too many retries → Block”.

So: *observe → infer → decide*. Useful when flows vary, AI adapts, or you care more about intent than exact steps.

---

## What does the AI do?

In Intentum, the **Infer** step optionally uses **AI (embeddings)**: it turns behavior keys (e.g. `user:login`, `analyst:prepare_esg_report`) into vectors, then produces a confidence level (High / Medium / Low / Certain) and signals from a similarity score.

| Step | What happens |
|------|----------------|
| **Embedding** | Each `actor:action` key gets a vector + score from an **embedding provider**. **Mock** (local/test) = hash-based, deterministic; **real provider** (OpenAI, Gemini, Mistral, Azure, Claude) = semantic vectors, so the same behavior can yield slightly different confidence across models. |
| **Similarity** | A **similarity engine** aggregates all embeddings into a single score (e.g. average). That score is mapped to a confidence level. |
| **Intent** | **LlmIntentModel** produces an **Intent** (Confidence + Signals) from this score; the policy returns Allow / Observe / Warn / Block based on that intent. |

Examples usually use **Mock** (no API key). To try real AI, set the right environment variables and use a provider; see [Providers](providers.md), [How to use AI providers](ai-providers-howto.md), and [Setup – real provider](setup.md#using-a-real-provider-eg-openai). **Samples** (`samples/`) are full showcase apps (many scenarios, Web API with infer, explain, explain-tree, playground, analytics, timeline); **examples** (`examples/`) are minimal single-use-case projects (fraud-intent, customer-intent, ai-fallback-intent, chained-intent, time-decay-intent, vector-normalization, greenwashing-intent). **Templates:** `dotnet new intentum-webapi`, `intentum-backgroundservice`, `intentum-function` — see [Setup – Create from template](setup.md#create-from-template-dotnet-new). **Examples overview** and **Tests overview** are in the docs sidebar. Intent may include optional **Reasoning** (e.g. which rule matched or fallback used).

---

## What replaced Given/When/Then?

In classic BDD you write **Given** (precondition), **When** (action), **Then** (assertion). That assumes fixed steps and a single pass/fail outcome. Intentum does not use Given/When/Then; it uses a different flow that fits non-deterministic and intent-based systems.

| BDD (Given/When/Then) | Intentum (replacement) |
|-----------------------|-------------------------|
| **Given** — fixed preconditions | **Observe** — you record what actually happened (events like login, retry, submit) in a **BehaviorSpace**. No fixed “given” state; you capture real behavior. |
| **When** — single action | Same **Observe** — events are the “when”; you add them with `space.Observe(actor, action)`. Multiple events, order preserved. |
| **Then** — one assertion, pass/fail | **Infer** + **Decide** — a model **infers** intent and confidence (High/Medium/Low) from the behavior, then a **policy** **decides** the outcome: **Allow**, **Observe**, **Warn**, or **Block**. So instead of “then X should be true” you get “given this behavior, intent is Y, decision is Z.” |

In short: **Given/When/Then is gone; in its place you have Observe (record events) → Infer (intent + confidence) → Decide (policy outcome).** You describe what happened, let the model interpret intent, and let rules choose the decision. See [API Reference](api.md) for the types and [Scenarios](scenarios.md) for examples.

---

## Who is it for?

**Use Intentum when:**

- Flows are non-deterministic or AI-driven.
- You want to reason about *intent* rather than strict pass/fail steps.
- You need policy-based decisions (allow / observe / warn / block) from observed behavior.

**Skip Intentum when:**

- Your system is fully deterministic with stable requirements.
- You only have small scripts or one-off tools where behavior drift doesn’t matter.

---

## Documentation contents

| Page | What you’ll find |
|------|------------------|
| [Architecture](architecture.md) | Core flow (Observe → Infer → Decide), package layout, inference pipeline, persistence/analytics/rate-limiting/multi-tenancy flows (Mermaid diagrams). |
| [Audience & use cases](audience.md) | Project types, user profiles, low/medium/high example test cases (AI and normal), sector-based examples. |
| [Setup](setup.md) | Prerequisites, NuGet install, first project walkthrough, env vars. |
| [API Reference](api.md) | Main types (BehaviorSpace, Intent, Policy, providers) and how they fit together. |
| [Providers](providers.md) | OpenAI, Gemini, Mistral, Azure OpenAI, Claude — env vars and DI setup. |
| [How to use AI providers](ai-providers-howto.md) | Easy / medium / hard usage examples for each AI provider (Mock, OpenAI, Gemini, Mistral, Azure, Claude). |
| [Usage Scenarios](scenarios.md) | Example flows (payment with retries, suspicious retries, policy order). |
| [CodeGen](codegen.md) | Scaffold CQRS + Intentum projects; generate Features from test assembly or YAML spec. |
| [Testing](testing.md) | Unit tests, coverage, error cases. |
| [Local integration tests](local-integration-tests.md) | Run VerifyAI (all providers) or per-provider integration tests locally with `.env` and scripts. |
| [Coverage](coverage.md) | How to generate and view coverage; SonarCloud findings and quality gate. |
| [Benchmarks](benchmarks.md) | BenchmarkDotNet: ToVector, Infer, PolicyEngine; run and refresh docs with `./scripts/run-benchmarks.sh`. |
| [Examples overview](examples-overview.md) | Examples and samples by difficulty (simple / medium / hard) and real-life use cases. |
| [Tests overview](tests-overview.md) | Test projects, sample links, and how to run unit and integration tests. |
| [Advanced Features](advanced-features.md) | Similarity engines, vector normalization, rule-based and chained intent models, fluent APIs, caching, **Intent Timeline**, **Intent Tree**, **Context-Aware Policy**, **Policy Store**, **Behavior Pattern Detector**, **Multi-Stage Model**, **Scenario Runner**, **stream processing**, **OpenTelemetry tracing**, rate limiting, analytics, middleware, observability, batch processing, persistence. |

---

## Quick start (3 steps)

1. **Install core packages**
   ```bash
   dotnet add package Intentum.Core
   dotnet add package Intentum.Runtime
   dotnet add package Intentum.AI
   ```

2. **Run the sample** (no API key needed; uses mock provider)
   ```bash
   dotnet run --project samples/Intentum.Sample
   ```

3. **Read [Setup](setup.md)** for a minimal “first project” and [API Reference](api.md) for the main types and flow.

---

## How to

- **How do I run the first scenario?** — Run the console sample with `dotnet run --project samples/Intentum.Sample` (mock provider, no API key). For a full API with intent infer, rate limiting, and analytics, run `dotnet run --project samples/Intentum.Sample.Web`; see [Setup](setup.md) for repo structure and endpoints.
- **How do I add a policy?** — Create an `IntentPolicy`, add rules in **order** with `.AddRule(PolicyRule(...))` (e.g. Block first, then Allow). After inference, call `intent.Decide(policy)`. See [Scenarios](scenarios.md) and [API Reference](api.md) for details.
- **How do I model classic flows (payment, login, support)?** — Record events with `space.Observe(actor, action)` (e.g. `"user"`, `"login"`; `"user"`, `"retry"`; `"user"`, `"submit"`). The model infers intent from behavior; the policy returns Allow, Observe, Warn, or Block. [Usage Scenarios](scenarios.md) includes both classic (payment, e‑commerce) and ESG examples.
- **How do I build scenarios with AI?** — The same `Observe` flow works with Mock or a real provider (OpenAI, Gemini, etc.); use meaningful behavior keys and base policy on confidence and signals. Details and tips: [Scenarios – How to build scenarios with AI](scenarios.md#how-to-build-scenarios-with-ai).

For more examples and rule ordering, see [Scenarios](scenarios.md) and [Audience & use cases](audience.md).

---

## Global usage notes

- **API keys** — Use environment variables or a secret manager; never commit keys.
- **Regions and latency** — Consider provider endpoint location and rate limits.
- **Production** — Avoid logging raw provider requests/responses.

For full API method signatures, see the [generated API site](https://keremvaris.github.io/Intentum/api/).
