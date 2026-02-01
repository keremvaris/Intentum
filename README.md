# Intentum

**Intent-Driven Development for the AI Era**

[![CI](https://github.com/keremvaris/Intentum/actions/workflows/ci.yml/badge.svg)](https://github.com/keremvaris/Intentum/actions/workflows/ci.yml)
[![NuGet Intentum.Core](https://img.shields.io/nuget/v/Intentum.Core.svg)](https://www.nuget.org/packages/Intentum.Core)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=keremvaris_Intentum&metric=coverage)](https://sonarcloud.io/summary/new_code?id=keremvaris_Intentum)
[![SonarCloud](https://sonarcloud.io/api/project_badges/measure?project=keremvaris_Intentum&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=keremvaris_Intentum)

Most software frameworks ask:
> *What happened?*

Intentum asks:
> **What was the system trying to achieve?**

Modern systems are no longer deterministic.
They adapt, retry, infer, and guess.
Yet we still test them with linear scenarios.

Intentum replaces scenario-based testing with **intent spaces** —
where behavior is treated as a signal,
and correctness is measured by **confidence**, not certainty.

If your system involves:
- AI or probabilistic logic
- user ambiguity
- adaptive workflows
- non-deterministic outcomes

Then Intentum is not an alternative approach.

It is the next one.

---

> **Software should be judged by intent, not by events.**

English | [Türkçe](README.tr.md)

**License:** [MIT](LICENSE) · **Contributing** — [CONTRIBUTING.md](CONTRIBUTING.md) · [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) · [SECURITY.md](SECURITY.md)

---

## The Intentum Manifesto

Intentum is built on eight principles:

- Software is no longer deterministic.
- Behavior is not intent.
- Scenarios are brittle; intent is resilient.
- Tests describe spaces, not scripts.
- AI breaks Given–When–Then.
- Intent is the new contract.
- Failures are signals.
- We design for understanding, not control.

**Full text:** [The Intentum Manifesto](docs/en/manifesto.md) (eight principles). Concise rules: [The Intentum Canon](docs/en/intentum-canon.md) (ten principles).

---

## Intentum vs existing approaches

### Conceptual comparison

| Approach | Center | Assumption | Fits |
|----------|--------|------------|------|
| TDD | Correctness | Deterministic | Algorithms |
| BDD | Scenario | Linear flow | Business rules |
| DDD | Model | Stable domain | Enterprise systems |
| **Intentum** | **Intent** | **Uncertainty** | **AI & adaptive systems** |

### Given–When–Then vs Intentum

| BDD | Intentum |
|-----|----------|
| Given (state) | Observed signals |
| When (action) | Behavior evolution |
| Then (assertion) | Intent confidence |
| Boolean result | Probabilistic outcome |
| Fragile scenarios | Resilient intent spaces |

**BDD answers "Did this happen?"**  
**Intentum answers "Does this make sense?"**

### Test philosophy

| Question | BDD | Intentum |
|----------|-----|----------|
| What does a test represent? | A story | A space |
| What is failure? | Error | Signal |
| Retry | Failure | Context |
| Edge case | Exception | Expected |
| Resilience to change | Low | High |

---

## Intentum is NOT / is

**Intentum is NOT:**
- a test framework replacement
- a BDD extension
- a rule engine
- a magic AI wrapper

**Intentum is:**
- an intent modeling framework
- a reasoning layer for behavior
- a foundation for AI-era correctness

---

## Getting Started (5 minutes)

**Run the minimal example first:** `dotnet run --project examples/hello-intentum` — one signal, one intent, console output. See [examples/hello-intentum](examples/hello-intentum) and [Examples overview](docs/en/examples-overview.md).

### 1. Install packages

```bash
dotnet add package Intentum.Core
dotnet add package Intentum.Runtime
```

For AI-backed inference (optional):

```bash
dotnet add package Intentum.AI
```

### 2. Define observed behavior

In Intentum, a test is not a scenario. It is a set of observed behaviors.

```csharp
using Intentum.Core;
using Intentum.Core.Behavior;

var space = new BehaviorSpace()
    .Observe("user", "login.failed")
    .Observe("user", "login.retry")
    .Observe("user", "password.reset.requested")
    .Observe("user", "login.success");
```

The question is not "Did login succeed?" but "What was the user trying to achieve?"

### 3. Infer intent

```csharp
using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;

var intentModel = new LlmIntentModel(
    new MockEmbeddingProvider(),
    new SimpleAverageSimilarityEngine());
var intent = intentModel.Infer(space);
```

This call does not follow rules or flows or order. It only interprets behavior signals. (No API key needed with Mock; use a real provider for production.)

### 4. Assert on intent, not steps

Assert on confidence (Level: Low, Medium, High, Certain) and optionally on intent name when using a custom model:

```csharp
// intent.Confidence.Level is "High" or "Certain"
// intent.Confidence.Score > 0.75
// With custom intent models: intent.Name == "AccountAccess"
```

This is a test — but it does not step through a script, is not brittle, and tolerates alternative paths.

### 5. Why this matters

The same intent can be captured by different behaviors:

```csharp
var space1 = new BehaviorSpace()
    .Observe("user", "password.reset")
    .Observe("user", "email.confirmed")
    .Observe("user", "login.success");
// intent = intentModel.Infer(space1);
```

Or:

```csharp
var space2 = new BehaviorSpace()
    .Observe("user", "login.failed")
    .Observe("user", "login.failed")
    .Observe("user", "account.locked");
// intent = intentModel.Infer(space2);
```

Scenario differs. Intent aligns. BDD breaks here; Intentum starts here.

### 6. Mental model

| Approach | Leads to |
|----------|----------|
| Events / Flows / Scenarios | noise / assumptions / fragility |
| **Intent / Confidence / Space** | meaning / correctness / resilience |

### 7. When to use Intentum

**Use Intentum if:** outcomes vary, retries are normal, AI makes decisions, users don't follow scripts.

**Do NOT use Intentum if:** logic is strictly deterministic, every step must be enforced, failure must always break the system.

### 8. What's next

- Plug in AI models ([Providers](docs/en/providers.md))
- Build custom intent classifiers
- Use Intentum alongside existing tests

Intentum does not replace your test suite. It explains what your test suite cannot.

---

## Quick example (with policy)

```csharp
using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Runtime.Policy;

var space = new BehaviorSpace()
    .Observe("user", "login")
    .Observe("user", "retry")
    .Observe("user", "submit");

var intentModel = new LlmIntentModel(
    new MockEmbeddingProvider(),
    new SimpleAverageSimilarityEngine());

var intent = intentModel.Infer(space);

var policy = new IntentPolicy()
    .AddRule(new PolicyRule(
        "HighConfidenceAllow",
        i => i.Confidence.Level is "High" or "Certain",
        PolicyDecision.Allow));

var decision = intent.Decide(policy);
```

**Run the sample:**

```bash
dotnet run --project samples/Intentum.Sample
```

**Advanced example (fraud / abuse intent):**

```bash
dotnet run --project examples/fraud-intent
```

No API key required. Infers suspicious vs. legitimate behavior, then policy decides Block / Observe / Allow. See [Real-world scenarios — Fraud](docs/en/real-world-scenarios.md#use-case-1-fraud--abuse-intent-detection).

**More examples:** [customer-intent](examples/customer-intent) (purchase, support), [greenwashing-intent](examples/greenwashing-intent) (ESG/report), [chained-intent](examples/chained-intent) (rule → LLM fallback, reasoning), [time-decay-intent](examples/time-decay-intent) (recent events weigh more), [vector-normalization](examples/vector-normalization) (Cap, L1, SoftCap). See [examples/README.md](examples/README.md).

---

## Documentation

- **GitHub Pages (EN/TR):** https://keremvaris.github.io/Intentum/
- [Why Intentum](docs/en/why-intentum.md) — name, philosophy, positioning
- [The Intentum Manifesto](docs/en/manifesto.md) — eight principles
- [The Intentum Canon](docs/en/intentum-canon.md) — ten principles for Intent-Driven Development
- [Roadmap](docs/en/roadmap.md) — v1.0 criteria, adoption and depth
- [Architecture](docs/en/architecture.md) — core flow, packages, inference pipeline
- [Setup](docs/en/setup.md) — install, first project, env vars
- [API Reference](https://keremvaris.github.io/Intentum/api/)
- [CodeGen](docs/en/codegen.md) — scaffold CQRS + Intentum, dotnet new template
- **Sample.Web:** `dotnet run --project samples/Intentum.Sample.Web` — UI, `POST /api/intent/infer`, `POST /api/intent/explain`, greenwashing (`POST /api/greenwashing/analyze`, `GET /api/greenwashing/recent`), Dashboard (analytics, son çıkarımlar, son greenwashing analizleri), analytics export, health. See [docs/setup](docs/en/setup.md) and [samples/Intentum.Sample.Web/README.md](samples/Intentum.Sample.Web/README.md).
- **Fraud intent:** `dotnet run --project examples/fraud-intent` — fraud/abuse intent, policy Block/Observe/Allow
- **Customer intent:** `dotnet run --project examples/customer-intent` — purchase, support, route by intent
- **Greenwashing intent:** `dotnet run --project examples/greenwashing-intent` — ESG/report detection
- **Chained intent:** `dotnet run --project examples/chained-intent` — rule-based first, LLM fallback, intent reasoning
- **Time decay:** `dotnet run --project examples/time-decay-intent` — recent events weighted higher
- **Vector normalization:** `dotnet run --project examples/vector-normalization` — Cap, L1, SoftCap for behavior vectors

---

## Taglines

- **Vision:** *Software should be judged by intent, not by events.*
- **Developer:** *From scenarios to intent spaces.* / *Understand what users meant, not just what they did.*
- **Short (NuGet/GitHub):** Intentum is an Intent-Driven Development framework that models behavior as intent spaces instead of deterministic scenarios.

---

## Packages

- **Core:** Intentum.Core, Intentum.Runtime, Intentum.AI
- **AI providers:** Intentum.AI.OpenAI, Intentum.AI.Gemini, Intentum.AI.Claude, Intentum.AI.Mistral, Intentum.AI.AzureOpenAI
- **Extensions:** Intentum.Testing, Intentum.AspNetCore, Intentum.Observability, Intentum.Logging
- **Persistence:** Intentum.Persistence, Intentum.Persistence.EntityFramework, Intentum.Analytics
- **Advanced:** Intentum.AI.Caching.Redis, Intentum.Clustering, Intentum.Events, Intentum.Experiments, Intentum.MultiTenancy, Intentum.Explainability, Intentum.Simulation, Intentum.Versioning — see [Advanced Features](docs/en/advanced-features.md)

---

## Tests and benchmarks

- **Unit tests:** `dotnet test tests/Intentum.Tests/Intentum.Tests.csproj` (CI excludes `Category=Integration`) — see [Testing](docs/en/testing.md), [Coverage](docs/en/coverage.md), [SonarCloud](https://sonarcloud.io/summary/new_code?id=keremvaris_Intentum).
- **VerifyAI (local):** `cp .env.example .env`, set at least one provider key, then `dotnet run --project samples/Intentum.VerifyAI` — see [Local integration tests](docs/en/local-integration-tests.md).
- **Per-provider integration tests (local):** `./scripts/run-integration-tests.sh` (OpenAI), `run-mistral-integration-tests.sh`, `run-gemini-integration-tests.sh`, `run-azure-integration-tests.sh`.
- **Benchmarks:** `dotnet run --project benchmarks/Intentum.Benchmarks/Intentum.Benchmarks.csproj -c Release` — latency/throughput for ToVector, Infer, PolicyEngine. Refresh docs: `./scripts/run-benchmarks.sh` → [Case studies — Benchmark results](docs/case-studies/benchmark-results.md). See [Benchmarks](docs/en/benchmarks.md).

---

## Configuration (env vars)

OPENAI_API_KEY, GEMINI_API_KEY, MISTRAL_API_KEY, AZURE_OPENAI_* — see [Setup](docs/en/setup.md) and [Providers](docs/en/providers.md).

---

## Security

Never commit API keys. Use environment variables or secret managers. Avoid logging raw provider requests/responses in production.

---

## Note

AI adapters use deterministic stubs in v1.0. Real HTTP calls are planned for v1.1.

---

## CI & releasing

- **CI** runs on push/PR to `master`: build, test, coverage, SonarCloud. Set `SONAR_TOKEN` in GitHub Secrets to enable analysis.
- **Versioning** is from git tags via [MinVer](https://github.com/adamralph/minver). Tag `v1.0.1` → package `1.0.1`.
- **Release:** `./release.sh` or push tag `v1.0.x`; see `.github/workflows/` and [CONTRIBUTING.md](CONTRIBUTING.md).

---

## License

MIT
