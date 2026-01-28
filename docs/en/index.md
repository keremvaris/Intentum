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

Examples usually use **Mock** (no API key). To try real AI, set the right environment variables and use a provider; see [Providers](providers.md) and [Setup – real provider](setup.md#real-provider-usage-eg-openai).

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
| [Audience & use cases](audience.md) | Project types, user profiles, low/medium/high example test cases (AI and normal), sector-based examples. |
| [Setup](setup.md) | Prerequisites, NuGet install, first project walkthrough, env vars. |
| [API Reference](api.md) | Main types (BehaviorSpace, Intent, Policy, providers) and how they fit together. |
| [Providers](providers.md) | OpenAI, Gemini, Mistral, Azure OpenAI, Claude — env vars and DI setup. |
| [Usage Scenarios](scenarios.md) | Example flows (payment with retries, suspicious retries, policy order). |
| [Testing](testing.md) | Unit tests, coverage, error cases. |
| [Coverage](coverage.md) | How to generate and view coverage. |

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

- **How do I run the first scenario?** — Run the sample with `dotnet run --project samples/Intentum.Sample`; it uses the mock provider and needs no API key. Examples cover both classic flows (payment, login, support, e‑commerce) and ESG (report submission, compliance).
- **How do I add a policy?** — Create an `IntentPolicy`, add rules in **order** with `.AddRule(PolicyRule(...))` (e.g. Block first, then Allow). After inference, call `intent.Decide(policy)`. See [Scenarios](scenarios.md) and [API Reference](api.md) for details.
- **How do I model classic flows (payment, login, support)?** — Record events with `space.Observe(actor, action)` (e.g. `"user"`, `"login"`; `"user"`, `"retry"`; `"user"`, `"submit"`). The model infers intent from behavior; the policy returns Allow, Observe, Warn, or Block. [Usage Scenarios](scenarios.md) includes both classic and ESG examples.
- **How do I build scenarios with AI?** — The same `Observe` flow works with Mock or a real provider (OpenAI, Gemini, etc.); use meaningful behavior keys and base policy on confidence and signals. Details and tips: [Scenarios – How to build scenarios with AI](scenarios.md#how-to-build-scenarios-with-ai).

For more examples and rule ordering, see [Scenarios](scenarios.md) and [Audience & use cases](audience.md).

---

## Global usage notes

- **API keys** — Use environment variables or a secret manager; never commit keys.
- **Regions and latency** — Consider provider endpoint location and rate limits.
- **Production** — Avoid logging raw provider requests/responses.

For full API method signatures, see the [generated API site](https://keremvaris.github.io/Intentum/api/).
