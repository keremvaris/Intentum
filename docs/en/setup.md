# Setup (EN)

This page walks you through prerequisites, package installation, and a minimal first project so you can run Intentum end-to-end.

---

## Prerequisites

- **.NET SDK 10.x** (or the version your project targets).

---

## Install packages (NuGet)

**Core** (required for behavior space, intent, and policy):

```bash
dotnet add package Intentum.Core
dotnet add package Intentum.Runtime
dotnet add package Intentum.AI
```

**Providers** (optional; pick one or more for real embedding APIs):

```bash
dotnet add package Intentum.AI.OpenAI
dotnet add package Intentum.AI.Gemini
dotnet add package Intentum.AI.Mistral
dotnet add package Intentum.AI.AzureOpenAI
dotnet add package Intentum.AI.Claude
```

Alternatively, add **Intentum.Providers** to get Core, Runtime, AI, and all providers in one: `dotnet add package Intentum.Providers`. If you add no provider, use **MockEmbeddingProvider** (in Intentum.AI) for local runs — no API key needed.

---

## First project: minimal console app

1. **Create a console app** (if you don’t have one):
   ```bash
   dotnet new console -n MyIntentumApp -o MyIntentumApp
   cd MyIntentumApp
   ```

2. **Add the core packages** (see above).

3. **Replace `Program.cs`** with a minimal flow:

```csharp
using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Runtime.Policy;

// 1) Build behavior: what happened?
var space = new BehaviorSpace()
    .Observe("user", "login")
    .Observe("user", "retry")
    .Observe("user", "submit");

// 2) Infer intent (mock provider = no API key)
var model = new LlmIntentModel(
    new MockEmbeddingProvider(),
    new SimpleAverageSimilarityEngine());
var intent = model.Infer(space);

// 3) Decide with a simple policy
var policy = new IntentPolicy()
    .AddRule(new PolicyRule(
        "AllowHigh",
        i => i.Confidence.Level is "High" or "Certain",
        PolicyDecision.Allow))
    .AddRule(new PolicyRule(
        "ObserveMedium",
        i => i.Confidence.Level == "Medium",
        PolicyDecision.Observe));

var decision = intent.Decide(policy);

Console.WriteLine($"Confidence: {intent.Confidence.Level}, Decision: {decision}");
```

4. **Run**
   ```bash
   dotnet run
   ```

You should see a confidence level and a decision (e.g. Allow or Observe). Next: add more rules in [Scenarios](scenarios.md), switch to a real provider in [Providers](providers.md), and read [API Reference](api.md) for all types.

---

## Using a real provider (e.g. OpenAI)

1. Add the provider package: `dotnet add package Intentum.AI.OpenAI`.
2. Set environment variables (e.g. `OPENAI_API_KEY`, `OPENAI_EMBEDDING_MODEL`). See [Providers](providers.md).
3. Replace the mock with the real provider and options:

```csharp
using Intentum.AI.OpenAI;

var options = OpenAIOptions.FromEnvironment();
var httpClient = new HttpClient { BaseAddress = new Uri(options.BaseUrl ?? "https://api.openai.com/v1/") };
// Add auth header, then:
var embeddingProvider = new OpenAIEmbeddingProvider(options, httpClient);
var model = new LlmIntentModel(embeddingProvider, new SimpleAverageSimilarityEngine());
```

For DI (e.g. ASP.NET Core), use `services.AddIntentumOpenAI(options)` and inject the provider. See [Providers](providers.md).

---

## Create from template (dotnet new)

You can start from a pre-configured project using **dotnet new** and the Intentum templates (install once from the repo `templates` folder):

```bash
# From repo root: install templates
dotnet new install ./templates/intentum-webapi
dotnet new install ./templates/intentum-backgroundservice
dotnet new install ./templates/intentum-function
```

Then create a new project:

| Template | Description |
|----------|-------------|
| **intentum-webapi** | Minimal ASP.NET Core Web API with Intentum: infer endpoint, health check, Mock provider. |
| **intentum-backgroundservice** | .NET Worker Service with **MemoryBehaviorStreamConsumer** and **IntentStreamWorker** that processes behavior event batches through an intent model and policy. |
| **intentum-function** | Azure Functions v4 (isolated worker) with HTTP-triggered **InferIntent** function, Mock model and policy. |

Example:

```bash
dotnet new intentum-webapi -n MyIntentumApi -o MyIntentumApi
cd MyIntentumApi
dotnet run
```

See [Advanced Features](advanced-features.md) for Policy Store, Context-Aware Policy, Multi-Stage Model, and other extensions.

---

## Environment variables (overview)

Set these only when using real HTTP adapters:

| Provider | Main variables |
|----------|-----------------|
| OpenAI | `OPENAI_API_KEY`, `OPENAI_EMBEDDING_MODEL`, `OPENAI_BASE_URL` |
| Gemini | `GEMINI_API_KEY`, `GEMINI_EMBEDDING_MODEL`, `GEMINI_BASE_URL` |
| Mistral | `MISTRAL_API_KEY`, `MISTRAL_EMBEDDING_MODEL`, `MISTRAL_BASE_URL` |
| Azure OpenAI | `AZURE_OPENAI_ENDPOINT`, `AZURE_OPENAI_API_KEY`, `AZURE_OPENAI_EMBEDDING_DEPLOYMENT`, `AZURE_OPENAI_API_VERSION` |
| Claude | `CLAUDE_API_KEY`, `CLAUDE_MODEL`, `CLAUDE_BASE_URL`, etc. |

Details and examples: [Providers](providers.md).

For **local development** (e.g. running integration tests or VerifyAI), you can use a `.env` file: copy `.env.example` to `.env`, set at least one provider’s key (OpenAI, Mistral, Gemini, Azure), and run `dotnet run --project samples/Intentum.VerifyAI` or the per-provider scripts (`./scripts/run-integration-tests.sh`, `run-mistral-integration-tests.sh`, etc.). See [Local integration tests](local-integration-tests.md). Never commit `.env`; it is in `.gitignore`.

---

## Repository structure

The solution contains many packages and two sample applications.

**Core & runtime** (required for behavior space, intent, policy):

- `Intentum.Core` — BehaviorSpace, Intent (with optional Reasoning), BehaviorEvent, BehaviorSpaceBuilder, ToVectorOptions, BatchIntentModel, RuleBasedIntentModel, ChainedIntentModel
- `Intentum.Runtime` — IntentPolicy, IntentPolicyBuilder, PolicyDecision, IRateLimiter, MemoryRateLimiter
- `Intentum.AI` — LlmIntentModel, embedding cache, similarity engines (SimpleAverage, TimeDecay, Cosine, Composite), ITimeAwareSimilarityEngine

**AI providers** (optional; pick one or more for real embeddings):

- `Intentum.AI.OpenAI`, `Intentum.AI.Gemini`, `Intentum.AI.Mistral`, `Intentum.AI.AzureOpenAI`, `Intentum.AI.Claude`

**Extensions** (optional; add as needed):

- `Intentum.AspNetCore` — Behavior observation middleware, health checks
- `Intentum.Testing` — TestHelpers, assertions for BehaviorSpace, Intent, PolicyDecision
- `Intentum.Observability` — OpenTelemetry metrics for inference and policy
- `Intentum.Logging` — Serilog integration for intent and policy
- `Intentum.Persistence` — IBehaviorSpaceRepository, IIntentHistoryRepository
- `Intentum.Persistence.EntityFramework` — EF Core implementation (SQL Server, SQLite, in-memory)
- `Intentum.Persistence.Redis` — Redis-backed behavior spaces and intent history; `AddIntentumPersistenceRedis(redis, keyPrefix?)`
- `Intentum.Persistence.MongoDB` — MongoDB-backed behavior spaces and intent history; `AddIntentumPersistenceMongoDB(database, collectionNames?)`
- `Intentum.Analytics` — IIntentAnalytics: trends, decision distribution, anomaly detection, JSON/CSV export
- `Intentum.CodeGen` — Scaffold CQRS + Intentum, YAML/JSON spec validation

**Samples:**

- `samples/Intentum.Sample` — Console: ESG, Carbon, EU Green Bond, workflow, classic (payment, support, e‑commerce), fluent API, caching, batch, rate limiting demo
- `samples/Intentum.Sample.Web` — ASP.NET Core API with Scalar docs and web UI: CQRS (carbon, orders), intent infer (`POST /api/intent/infer`), intent explain (`POST /api/intent/explain`), **greenwashing detection** (`POST /api/greenwashing/analyze`, `GET /api/greenwashing/recent`), rate limiting, persistence (in-memory), **Dashboard** (analytics, son çıkarımlar, son greenwashing analizleri), reporting & analytics (`GET /api/intent/analytics/summary`, `/api/intent/history`, `/export/json`, `/export/csv`), health checks. See [Greenwashing detection (how-to)](greenwashing-detection-howto.md#6-sample-application-intentumsampleweb) and [samples/Intentum.Sample.Web/README.md](../../samples/Intentum.Sample.Web/README.md).

---

## Build and run the repo samples

From the repository root:

```bash
dotnet build Intentum.slnx
```

**Console sample** (scenarios, batch, rate limit demo):

```bash
dotnet run --project samples/Intentum.Sample
```

Runs ESG, Carbon, EU Green Bond, workflow, and classic (payment, support, e‑commerce) scenarios. By default it uses **mock** embedding (no API key); you’ll see `AI: Mock (no API key) → similarity → confidence → policy` in the output.

**To try real AI:** Set the `OPENAI_API_KEY` (and optionally `OPENAI_EMBEDDING_MODEL`) environment variable; the sample will use **OpenAI embeddings**. See [Providers](providers.md).

**Web sample** (API + UI, intent infer, explain, greenwashing, Dashboard, analytics):

```bash
dotnet run --project samples/Intentum.Sample.Web
```

- **UI:** http://localhost:5150/ (or the port in `launchSettings.json`) — **Örnekler** (carbon, orders, greenwashing, intent infer, explain) and **Dashboard** (analytics, son çıkarımlar, son greenwashing analizleri)
- **API docs (Scalar):** http://localhost:5150/scalar
- **Endpoints:**
  - Carbon: `POST /api/carbon/calculate`, `GET /api/carbon/report/{id}`
  - Orders: `POST /api/orders`
  - Intent: `POST /api/intent/infer` (body: `{ "events": [ { "actor": "user", "action": "login" }, ... ] }`), `POST /api/intent/explain` (same body; returns signal contributions), `POST /api/intent/explain-tree` (decision tree), `POST /api/intent/playground/compare` (compare models)
  - Greenwashing: `POST /api/greenwashing/analyze` (body: `{ "report": "...", "sourceType": "Report", "language": "tr", "imageBase64": null }`), `GET /api/greenwashing/recent?limit=15`
  - Analytics: `GET /api/intent/analytics/summary`, `GET /api/intent/history`, `GET /api/intent/analytics/timeline/{entityId}`, `GET /api/intent/analytics/export/json`, `GET /api/intent/analytics/export/csv`
  - Health: `/health`

---

## Install from local NuGet (development)

If you’re building Intentum from source and want to reference local packages:

```bash
dotnet pack Intentum.slnx -c Release
dotnet nuget add source /path/to/Intentum/src/Intentum.Core/bin/Release -n IntentumLocal
dotnet add package Intentum.Core --source IntentumLocal
```

Repeat for other projects (Intentum.Runtime, Intentum.AI, etc.) as needed.

---

## Tests and benchmarks

- **Unit and contract tests:** From repo root, `dotnet test tests/Intentum.Tests/Intentum.Tests.csproj`. See [Testing](testing.md).
- **Integration tests and VerifyAI (local only):** Set at least one provider’s key in `.env` (see `.env.example`), then run `dotnet run --project samples/Intentum.VerifyAI` or per-provider scripts (`./scripts/run-integration-tests.sh`, `run-mistral-integration-tests.sh`, `run-gemini-integration-tests.sh`, `run-azure-integration-tests.sh`). See [Local integration tests](local-integration-tests.md).
- **Benchmarks:** Run `dotnet run --project benchmarks/Intentum.Benchmarks/Intentum.Benchmarks.csproj -c Release` for latency and throughput. Results go to `BenchmarkDotNet.Artifacts/results/`. See [benchmarks/README.md](../../benchmarks/README.md) and [Case studies](../case-studies/README.md).
