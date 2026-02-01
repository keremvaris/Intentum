# Testing (EN)

Intentum tests focus on **contracts** (correct parsing and behavior of providers and core types) and **core flows** (BehaviorSpace → Infer → Decide). Tests use **mock HTTP** and in-memory providers so you can run them without API keys or network.

This page explains what is tested, how to run tests, and how to add your own. For coverage generation and reports, see [Coverage](coverage.md).

---

## Why test this way?

- **No real API calls in CI:** Mock HTTP and stub responses keep tests fast and stable; no secrets needed.
- **Contract tests:** We assert that each provider correctly parses its API response shape (embedding array, score) into `IntentEmbedding` and that the intent model and policy engine behave as expected.
- **Core behavior:** BehaviorSpace vectorization, intent inference, and policy decisions are covered so refactors don’t break the main flow.

---

## How to run tests

From the repository root:

```bash
dotnet test tests/Intentum.Tests/Intentum.Tests.csproj
```

With verbose output:

```bash
dotnet test tests/Intentum.Tests/Intentum.Tests.csproj -v n
```

---

## What is covered?

| Area | What we test |
|------|----------------|
| **BehaviorSpace** | Building a space with `Observe(actor, action)`, event count, and `ToVector()` dimensions; `IntentumCoreExtensions` (Observe, EvaluateIntent, IntentEvaluator.Normalize). |
| **Intent inference** | `LlmIntentModel` with mock provider and `SimpleAverageSimilarityEngine`: confidence level and score, signals; **IntentConfidence** (FromScore for Low/Medium/High/Certain). |
| **Policy decisions** | `IntentPolicy` and **IntentPolicyEngine**: rule order, first matching rule wins, no rule match, empty policy; Allow, Observe, Warn, Block outcomes; **PolicyDecisionTypes** (ToLocalizedString); **RuntimeExtensions** (DecideWithRateLimit / DecideWithRateLimitAsync with and without rate limit). |
| **Localization** | **DefaultLocalizer** for decision labels (culture, known/unknown keys). |
| **Options validation** | **OpenAIOptions**, **AzureOpenAIOptions**, **GeminiOptions**, **MistralOptions** `Validate()`: valid case and invalid (empty API key, embedding model, base URL) throw. |
| **Provider response parsing** | Each embedding provider (OpenAI, Gemini, Mistral, Azure OpenAI) with a **mock HttpClient** that returns a fixed JSON response; we assert the parsed embedding score (or exception on non-200). |
| **Provider IntentModels** | **OpenAIIntentModel**, **GeminiIntentModel**, **MistralIntentModel**, **AzureOpenAIIntentModel** with **MockEmbeddingProvider**: infer returns expected confidence and signals. |
| **Clustering** | **AddIntentClustering** registration; **IntentClusterer** (cluster Id, RecordIds, ClusterSummary average/min/max). |
| **Testing utilities** | **IntentAssertions** (HasSignalCount, ContainsSignal), **PolicyDecisionAssertions** (IsAllow, IsBlock, IsNotBlock), **TestHelpers.CreateModel**. |
| **Redis embedding cache** | **RedisEmbeddingCache** with in-memory `IDistributedCache` (Get/Set/Remove); no real Redis. |
| **Webhook / Events** | **WebhookIntentEventHandler**: AddWebhook options, HandleAsync posts to mock HttpClient; **AddIntentumEvents** DI registration. |
| **Experiments** | **IntentExperiment**: AddVariant, SplitTraffic, RunAsync with mock model/policy. |
| **Simulation** | **BehaviorSpaceSimulator**: FromSequence, GenerateRandom (with seed). |
| **Explainability** | **IntentExplainer**: GetSignalContributions, GetExplanation. **IntentTreeExplainer**: GetIntentTree (decision tree, matched rule, signal nodes). |
| **Policy observability** | **DecideWithExecutionLog**: execution record (matched rule, intent name, decision, duration, success, exception); **DecideWithMetrics** consistency. |
| **Multi-tenancy** | **TenantAwareBehaviorSpaceRepository**: SaveAsync injects TenantId, GetByIdAsync filters by tenant; uses in-memory repo. |
| **Versioning** | **PolicyVersionTracker**: Add, Current, Rollback, Rollforward, SetCurrent; **VersionedPolicy**. |

So: we don’t call real APIs; we feed fake JSON or use mock providers and check that the provider and model produce the expected `Intent` and `PolicyDecision`.

---

## Error cases

- **HTTP non-200:** Providers throw when the HTTP response is not successful (e.g. 401, 500). Tests can simulate this with a mock client that returns 401/500 and assert that an exception is thrown.
- **Empty embeddings:** If the API returns an empty embedding array (or missing embedding), the provider returns a score of 0 (or equivalent); tests cover this so behavior is predictable.

---

## How to add tests (mock HTTP)

To test a provider without calling the real API:

1. Create an `HttpClient` that returns a fixed response (e.g. `new HttpClient(new MockHttpMessageHandler(json))` or use a test server).
2. Instantiate the provider with options (e.g. dummy API key) and this client.
3. Call `provider.Embed("user:login")` (or similar) and assert on `result.Score` or on an expected exception.

Example pattern (conceptually):

```csharp
var json = """{ "data": [ { "embedding": [0.5, -0.5] } ] }""";
var client = CreateMockClient(json);
var provider = new OpenAIEmbeddingProvider(options, client);
var result = provider.Embed("user:login");
Assert.InRange(result.Score, 0.49, 0.51);
```

See `ProviderHttpTests` in the repo for real examples. For coverage options (e.g. CollectCoverage, OpenCover), see [Coverage](coverage.md).

---

## What is not covered (yet)

| Area | Status |
|------|--------|
| **Intentum.Persistence.MongoDB** | No test project reference; no unit or integration tests for `MongoBehaviorSpaceRepository` / `MongoIntentHistoryRepository`. |
| **Intentum.Persistence.EntityFramework** | No test project reference; no tests for EF repositories or PostgreSQL/SQL Server. |
| **Intentum.Persistence.Redis** | No test project reference; no tests for Redis-backed behavior space or intent history repositories. |
| **Real Redis / MongoDB / PostgreSQL** | All current tests use in-memory or mock implementations. Integration tests against real databases (e.g. Testcontainers) are not in place. |

If you add persistence or run against a real store, consider adding integration tests (e.g. Testcontainers for MongoDB/PostgreSQL/Redis) or at least contract tests with a fake repository that implements the same interface.
