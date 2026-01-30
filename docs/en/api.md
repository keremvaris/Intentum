# API Reference (EN)

This page explains the main types and how they fit together. For full method signatures and generated docs, see the [API site](https://keremvaris.github.io/Intentum/api/).

---

## How Intentum works (typical flow)

1. **Observe** — You record user or system events (e.g. login, retry, submit) into a **BehaviorSpace**.
2. **Infer** — An **LlmIntentModel** (with an embedding provider and similarity engine) turns that behavior into an **Intent** and a confidence level (High / Medium / Low / Certain).
3. **Decide** — You pass the **Intent** and an **IntentPolicy** (rules) to **Decide**; you get **Allow**, **Observe**, **Warn**, or **Block**.

So: *behavior → intent → policy decision*. No hard-coded scenario steps; the model infers intent from the observed events.

---

## Core (`Intentum.Core`)

| Type | What it does |
|------|----------------|
| **BehaviorSpace** | Container for observed events. You call `.Observe(actor, action)` (e.g. `"user"`, `"login"`). Use `.ToVector()` or `.ToVector(ToVectorOptions?)` to get a behavior vector for inference; the result is cached until you call `Observe` again. Use **ToVectorOptions** for normalization (Cap, L1, SoftCap). |
| **ToVectorOptions** | Options for building a behavior vector: `Normalization` (None, Cap, L1, SoftCap) and optional `CapPerDimension`. Use with `BehaviorSpace.ToVector(options)` or `ToVector(start, end, options)`. |
| **Intent** | Result of inference: confidence level, score, signals (contributing behaviors with weights), and optional **Reasoning** (human-readable explanation, e.g. which rule matched or fallback used). |
| **IntentConfidence** | Part of Intent: `Level` (string) and `Score` (0–1). |
| **IntentSignal** | One signal in an Intent: `Source`, `Description`, `Weight`. |
| **IntentEvaluator** | Evaluates intent against criteria; used internally by the model. |
| **RuleBasedIntentModel** | Intent model that uses rules only (no LLM). First matching rule wins; returns **RuleMatch** (name, score, reasoning). Fast, deterministic, explainable. |
| **ChainedIntentModel** | Tries a primary model first; if confidence below threshold, falls back to a secondary model (e.g. LlmIntentModel). Use with RuleBasedIntentModel + LlmIntentModel for rule-first + LLM fallback. |
| **RuleMatch** | Result of a rule: `Name`, `Score`, optional `Reasoning`. Returned by rules passed to **RuleBasedIntentModel**. |

**Namespace:** `Intent`, `IntentConfidence`, and `IntentSignal` are in **`Intentum.Core.Intents`**. Use `using Intentum.Core.Intents;` to reference them.

**Where to start:** Create a `BehaviorSpace`, call `.Observe(...)` for each event, then pass the space to your intent model’s `Infer(space)` (optionally pass a pre-computed `BehaviorVector` as second argument to avoid recomputation).

---

## Runtime (`Intentum.Runtime`)

| Type | What it does |
|------|----------------|
| **IntentPolicy** | Ordered list of rules. Add rules with `.AddRule(PolicyRule(...))`. First matching rule wins. Use `.WithBase(basePolicy)` for inheritance; `IntentPolicy.Merge(policy1, policy2, ...)` to combine policies. |
| **PolicyVariantSet** | A/B policy variants: multiple named policies with a selector (`Func<Intent, string>`). Use `intent.Decide(variantSet)` to evaluate with the selected policy. Call `GetVariantNames()` for the set of variant names. |
| **IntentPolicyBuilder** | Fluent builder for creating IntentPolicy instances. Use `.Allow(...)`, `.Block(...)`, `.Escalate(...)`, etc. |
| **PolicyRule** | Name + condition (e.g. lambda on `Intent`) + **PolicyDecision** (Allow, Observe, Warn, Block, Escalate, RequireAuth, RateLimit). |
| **PolicyDecision** | Decision enum: **Allow**, **Observe**, **Warn**, **Block**, **Escalate**, **RequireAuth**, **RateLimit**. |
| **IntentPolicyEngine** | Evaluates an intent against a policy; returns a **PolicyDecision**. |
| **RuntimeExtensions.Decide** | Extension: `intent.Decide(policy)` — runs the policy and returns the decision. |
| **RuntimeExtensions.DecideWithRateLimit** / **DecideWithRateLimitAsync** | When decision is RateLimit, checks **IRateLimiter** and returns **RateLimitResult**. Pass **RateLimitOptions** (Key, Limit, Window). |
| **RateLimitOptions** | Key, Limit, Window — use with DecideWithRateLimit / DecideWithRateLimitAsync. |
| **IRateLimiter** / **MemoryRateLimiter** | Rate limiting for PolicyDecision.RateLimit. **MemoryRateLimiter** = in-memory fixed window; use a distributed implementation for multi-node. |
| **RateLimitResult** | Allowed, CurrentCount, Limit, RetryAfter. |
| **RuntimeExtensions.ToLocalizedString** | Extension: `decision.ToLocalizedString(localizer)` — human-readable text (e.g. for UI). |
| **IIntentumLocalizer** / **DefaultLocalizer** | Localization for decision labels (e.g. "Allow", "Block"). **DefaultLocalizer** uses a culture (e.g. `"tr"`). |

**Where to start:** Build an `IntentPolicy` with `.AddRule(...)` in the order you want (e.g. Block rules first, then Allow). Call `intent.Decide(policy)` after inference.

**Fluent API example:**
```csharp
var policy = new IntentPolicyBuilder()
    .Block("ExcessiveRetry", i => i.Signals.Count(s => s.Description.Contains("retry")) >= 3)
    .Escalate("LowConfidence", i => i.Confidence.Level == "Low")
    .RequireAuth("SensitiveAction", i => i.Signals.Any(s => s.Description.Contains("sensitive")))
    .Allow("HighConfidence", i => i.Confidence.Level is "High" or "Certain")
    .Build();
```

**New decision types:**
- **Escalate** — Escalate to a higher level for review
- **RequireAuth** — Require additional authentication before proceeding
- **RateLimit** — Apply rate limiting to the action

---

## AI (`Intentum.AI`)

| Type | What it does |
|------|----------------|
| **IIntentEmbeddingProvider** | Turns a behavior key (e.g. `"user:login"`) into an **IntentEmbedding** (vector + score). Implemented by each provider (OpenAI, Gemini, etc.) or **MockEmbeddingProvider** for tests. |
| **IIntentSimilarityEngine** | Combines embeddings into a single similarity score. Supports optional **sourceWeights** (e.g. dimension counts) via overload `CalculateIntentScore(embeddings, sourceWeights)`. **SimpleAverageSimilarityEngine** is the built-in option. |
| **ITimeAwareSimilarityEngine** | Similarity engine that can use behavior space timestamps (e.g. time decay). When used with **LlmIntentModel**, the model calls **CalculateIntentScoreWithTimeDecay(behaviorSpace, embeddings)** automatically. |
| **WeightedAverageSimilarityEngine** | Similarity engine that applies weights to embeddings based on their source (actor:action). Uses **sourceWeights** when provided (e.g. from vector dimensions). |
| **TimeDecaySimilarityEngine** | Similarity engine that applies time-based decay to embeddings. Implements **ITimeAwareSimilarityEngine**; recent events have higher influence. Used automatically by LlmIntentModel when passed as the engine. |
| **CosineSimilarityEngine** | Similarity engine that uses cosine similarity between embedding vectors. Falls back to simple average if vectors are not available. |
| **CompositeSimilarityEngine** | Combines multiple similarity engines using weighted combination. Useful for A/B testing. |
| **LlmIntentModel** | Takes an embedding provider + similarity engine; **Infer(BehaviorSpace, BehaviorVector? precomputedVector = null)** returns an **Intent** with confidence and signals. Uses dimension counts as weights when the engine supports it; uses time decay when the engine implements **ITimeAwareSimilarityEngine**. |
| **IntentModelStreamingExtensions** | **InferMany(model, spaces)** — lazy `IEnumerable<Intent>` over many spaces; **InferManyAsync(model, spaces, ct)** — async stream `IAsyncEnumerable<Intent>`. |
| **IEmbeddingCache** / **MemoryEmbeddingCache** | Cache interface and memory implementation for embedding results. Use **CachedEmbeddingProvider** to wrap any provider with caching. |
| **IBatchIntentModel** / **BatchIntentModel** | Batch processing for multiple behavior spaces. Supports async processing with cancellation. |

**Where to start:** Use **MockEmbeddingProvider** and **SimpleAverageSimilarityEngine** for a quick local run; swap in a real provider (see [Providers](providers.md)) for production.

**Similarity engines:** Choose based on your needs:
- **SimpleAverageSimilarityEngine** — Default, equal weight to all events
- **WeightedAverageSimilarityEngine** — Custom weights per behavior (e.g. `login` = 2.0, `retry` = 0.5)
- **TimeDecaySimilarityEngine** — Recent events weighted higher (configurable half-life)

**AI pipeline (summary):**  
1) **Embedding** — Each behavior key (`actor:action`) is sent to the provider; returns vector + score. Mock = hash; real provider = semantic embedding.  
2) **Similarity** — All embeddings are combined into a single score (e.g. average).  
3) **Confidence** — The score is mapped to High/Medium/Low/Certain.  
4) **Signals** — Each behavior’s weight appears in Intent signals; usable in policy rules (e.g. retry count).

---

## Providers (optional packages)

| Type | What it does |
|------|----------------|
| **OpenAIEmbeddingProvider** | Uses OpenAI embedding API; configure with **OpenAIOptions** (e.g. `FromEnvironment()`). |
| **GeminiEmbeddingProvider** | Uses Google Gemini embedding API; **GeminiOptions**. |
| **MistralEmbeddingProvider** | Uses Mistral embedding API; **MistralOptions**. |
| **AzureOpenAIEmbeddingProvider** | Uses Azure OpenAI embedding deployment; **AzureOpenAIOptions**. |
| **ClaudeMessageIntentModel** | Claude-based intent model (message scoring); **ClaudeOptions**. |

Register providers via the **AddIntentum\*** extension methods and options (env vars). See [Providers](providers.md) for setup and env vars.

---

## Testing (`Intentum.Testing`)

| Type | What it does |
|------|----------------|
| **TestHelpers** | Common test object creators: `CreateDefaultModel()`, `CreateDefaultPolicy()`, `CreateSimpleSpace()`, etc. |
| **BehaviorSpaceAssertions** | Assertion helpers for BehaviorSpace: `ContainsEvent()`, `HasEventCount()`, `ContainsActor()`, etc. |
| **IntentAssertions** | Assertion helpers for Intent: `HasConfidenceLevel()`, `HasConfidenceScore()`, `HasSignals()`, etc. |
| **PolicyDecisionAssertions** | Assertion helpers for PolicyDecision: `IsOneOf()`, `IsAllow()`, `IsBlock()`, etc. |

**Where to start:** Add `Intentum.Testing` package to your test project and use helpers for cleaner test code.

---

## ASP.NET Core (`Intentum.AspNetCore`)

| Type | What it does |
|------|----------------|
| **BehaviorObservationMiddleware** | Middleware that automatically observes HTTP request behaviors into a BehaviorSpace. |
| **IntentumAspNetCoreExtensions** | Extension methods: `AddIntentum()` for DI, `UseIntentumBehaviorObservation()` for middleware. |

**Where to start:** Add `Intentum.AspNetCore` package, call `services.AddIntentum()` in `Program.cs`, then `app.UseIntentumBehaviorObservation()`.

---

## Observability (`Intentum.Observability`)

| Type | What it does |
|------|----------------|
| **IntentumMetrics** | OpenTelemetry metrics for Intentum operations: intent inference count/duration, confidence scores, policy decisions, behavior space sizes. |
| **ObservableIntentModel** | Wrapper around IIntentModel that adds observability metrics. |
| **ObservablePolicyEngine** | Extension methods for policy decisions with metrics: `DecideWithMetrics()`. |

**Where to start:** Add `Intentum.Observability` package and wrap your model with `ObservableIntentModel` for automatic metrics.

---

## Logging (`Intentum.Logging`)

| Type | What it does |
|------|----------------|
| **IntentumLogger** | Structured logging for Intentum operations using Serilog. Logs intent inference, policy decisions, and behavior observations. |
| **LoggingExtensions** | Extension methods: `LogIntentInference()`, `LogPolicyDecision()`, `LogBehaviorSpace()`. |

**Where to start:** Add `Intentum.Logging` package and use extension methods or `IntentumLogger` static methods.

---

## Persistence (`Intentum.Persistence`)

| Type | What it does |
|------|----------------|
| **IBehaviorSpaceRepository** | Repository interface for persisting and querying behavior spaces. |
| **IIntentHistoryRepository** | Repository interface for storing intent inference results and policy decisions. |
| **Intentum.Persistence.EntityFramework** | EF Core implementation; `AddIntentumPersistence()`. |
| **Intentum.Persistence.Redis** | Redis-backed repositories; `AddIntentumPersistenceRedis(IConnectionMultiplexer, keyPrefix?)`. |
| **Intentum.Persistence.MongoDB** | MongoDB-backed repositories; `AddIntentumPersistenceMongoDB(IMongoDatabase, behaviorSpaceCollectionName?, intentHistoryCollectionName?)`. |

**Where to start:** Add `Intentum.Persistence.EntityFramework`, `Intentum.Persistence.Redis`, or `Intentum.Persistence.MongoDB`; register with the corresponding `AddIntentumPersistence*` extension method.

---

## Extended packages (API overview)

The following packages add optional capabilities. For detailed usage (what it is, when to use, how to configure), see [Advanced Features](advanced-features.md).

### Redis embedding cache (`Intentum.AI.Caching.Redis`)

| Type | What it does |
|------|----------------|
| **RedisEmbeddingCache** | `IEmbeddingCache` implementation using Redis (`IDistributedCache`). |
| **RedisCachingExtensions.AddIntentumRedisCache** | Registers Redis cache and options (ConnectionString, InstanceName, DefaultExpiration). |
| **IntentumRedisCacheOptions** | ConnectionString, InstanceName, DefaultExpiration. |

### Clustering (`Intentum.Clustering`)

| Type | What it does |
|------|----------------|
| **IIntentClusterer** | Clusters intent history records (pattern or score buckets). |
| **IntentClusterer** | Default: `ClusterByPatternAsync(records)` (by ConfidenceLevel + Decision), `ClusterByConfidenceScoreAsync(records, k)`. |
| **IntentCluster** | Id, Label, RecordIds, Count, Summary (ClusterSummary). |
| **ClusterSummary** | AverageConfidenceScore, MinScore, MaxScore. |
| **ClusteringExtensions.AddIntentClustering** | Registers `IIntentClusterer` in DI. |

### Events / Webhook (`Intentum.Events`)

| Type | What it does |
|------|----------------|
| **IIntentEventHandler** | `HandleAsync(payload, eventType)` — e.g. dispatch to webhook. |
| **WebhookIntentEventHandler** | POSTs JSON to configured URLs (IntentInferred, PolicyDecisionChanged) with retry. |
| **IntentEventPayload** | BehaviorSpaceId, Intent, Decision, RecordedAt. |
| **IntentumEventType** | IntentInferred, PolicyDecisionChanged. |
| **EventsExtensions.AddIntentumEvents** | Registers event handling; **AddWebhook(url, events?)** on options. |
| **IntentumEventsOptions** | Webhooks (list of Url + EventTypes), RetryCount. |

### Experiments (`Intentum.Experiments`)

| Type | What it does |
|------|----------------|
| **IntentExperiment** | A/B test: `.AddVariant(name, model, policy)`, `.SplitTraffic(percentages)`, `.RunAsync(behaviorSpaces)` / `.Run(behaviorSpaces)`. |
| **ExperimentResult** | VariantName, Intent, Decision (per behavior space). |
| **ExperimentVariant** | Name, Model, Policy (internal). |

### Explainability (`Intentum.Explainability`)

| Type | What it does |
|------|----------------|
| **IIntentExplainer** | Explains how intent was inferred (signal contributions, text summary). |
| **IntentExplainer** | `GetSignalContributions(intent)` → list of **SignalContribution** (Source, Description, Weight, ContributionPercent); `GetExplanation(intent, maxSignals?)` → string. |
| **SignalContribution** | Source, Description, Weight, ContributionPercent. |

### Simulation (`Intentum.Simulation`)

| Type | What it does |
|------|----------------|
| **IBehaviorSpaceSimulator** | Generates synthetic behavior spaces. |
| **BehaviorSpaceSimulator** | `FromSequence((actor, action)[])` — fixed sequence; `GenerateRandom(actors, actions, eventCount, randomSeed?)` — random space. |

### Multi-tenancy (`Intentum.MultiTenancy`)

| Type | What it does |
|------|----------------|
| **ITenantProvider** | `GetCurrentTenantId()` — e.g. from HTTP context or claims. |
| **TenantAwareBehaviorSpaceRepository** | Wraps `IBehaviorSpaceRepository`: injects TenantId on Save, filters by tenant on Get/Delete. |
| **MultiTenancyExtensions.AddTenantAwareBehaviorSpaceRepository** | Registers tenant-aware repo in DI (requires inner repo + ITenantProvider). |

### Versioning (`Intentum.Versioning`)

| Type | What it does |
|------|----------------|
| **IVersionedPolicy** | Version (string) + Policy (IntentPolicy). |
| **VersionedPolicy** | Record: `new VersionedPolicy(version, policy)`. |
| **PolicyVersionTracker** | `Add(versionedPolicy)`, `Current`, `Versions`, `Rollback()`, `Rollforward()`, `SetCurrent(index)`, `CompareVersions(a, b)`. |

---

## Analytics (`Intentum.Analytics`)

| Type | What it does |
|------|----------------|
| **IIntentAnalytics** | Analytics over intent history: confidence trends, decision distribution, anomaly detection, export. |
| **IntentAnalytics** | Default implementation using IIntentHistoryRepository. |
| **ConfidenceTrendPoint** | One bucket in a confidence trend (BucketStart, BucketEnd, ConfidenceLevel, Count, AverageScore). |
| **DecisionDistributionReport** | Count per PolicyDecision in a time window. |
| **AnomalyReport** | Detected anomaly (Type, Description, Severity, Details). |
| **AnalyticsSummary** | Dashboard-ready summary (trends, distribution, anomalies). |

**Where to start:** Register `IIntentHistoryRepository` (e.g. via `AddIntentumPersistence`), then add `AddIntentAnalytics()` and inject `IIntentAnalytics`. Use `GetSummaryAsync()`, `GetConfidenceTrendsAsync()`, `GetDecisionDistributionAsync()`, `DetectAnomaliesAsync()`, `ExportToJsonAsync()`, `ExportToCsvAsync()`.

---

## Sample Web HTTP API (`Intentum.Sample.Web`)

The web sample exposes HTTP endpoints for intent inference, explainability, greenwashing detection, and analytics. Run with `dotnet run --project samples/Intentum.Sample.Web`; UI at http://localhost:5150/, API docs at http://localhost:5150/scalar.

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/intent/infer` | Infer intent from events. Body: `{ "events": [ { "actor": "user", "action": "login" }, ... ] }`. Returns intent name, confidence, decision, signals. |
| POST | `/api/intent/explain` | Same body as infer; returns signal contributions (source, description, weight, contribution percent) and text explanation. |
| GET | `/api/intent/history` | Paginated intent history (in-memory in sample). Query: `skip`, `take`. |
| GET | `/api/intent/analytics/summary` | Dashboard summary: confidence trends, decision distribution, anomaly list. |
| GET | `/api/intent/analytics/export/json` | Export analytics to JSON. |
| GET | `/api/intent/analytics/export/csv` | Export analytics to CSV. |
| POST | `/api/greenwashing/analyze` | Analyze report for greenwashing. Body: `{ "report": "...", "sourceType": "Report", "language": "tr", "imageBase64": null }`. Returns intent, decision, signals, suggested actions, `sourceMetadata`, `visualResult` (if image sent). |
| GET | `/api/greenwashing/recent?limit=15` | Last greenwashing analyses (in-memory; used by Dashboard). |
| POST | `/api/carbon/calculate` | Carbon footprint calculation (CQRS sample). |
| GET | `/api/carbon/report/{reportId}` | Get carbon report by id. |
| POST | `/api/orders` | Place order (CQRS sample). |
| GET | `/health` | Health check. |

See [Setup](setup.md#build-and-run-the-repo-samples) and [Greenwashing detection (how-to)](greenwashing-detection-howto.md#6-sample-application-intentumsampleweb) for details.

---

## Batch Processing (`Intentum.Core.Batch`)

| Type | What it does |
|------|----------------|
| **IBatchIntentModel** | Interface for batch intent inference operations. |
| **BatchIntentModel** | Processes multiple behavior spaces efficiently in batch. Supports async processing with cancellation. |

**Where to start:** Wrap your `IIntentModel` with `BatchIntentModel` and use `InferBatch()` or `InferBatchAsync()`.

---

## Minimal code reference

```csharp
// 1) Build behavior (using fluent API)
var space = new BehaviorSpaceBuilder()
    .WithActor("user")
        .Action("login")
        .Action("retry")
        .Action("submit")
    .Build();

// 2) Infer intent (Mock = no API key, with caching and cosine similarity)
var cache = new MemoryEmbeddingCache(new MemoryCache(new MemoryCacheOptions()));
var cachedProvider = new CachedEmbeddingProvider(
    new MockEmbeddingProvider(),
    cache);
var compositeEngine = new CompositeSimilarityEngine(new[]
{
    new SimpleAverageSimilarityEngine(),
    new CosineSimilarityEngine()
});
var model = new LlmIntentModel(cachedProvider, compositeEngine);
var intent = model.Infer(space);

// 3) Decide (using fluent API with new decision types)
var policy = new IntentPolicyBuilder()
    .Block("ExcessiveRetry", i => i.Signals.Count(s => s.Description.Contains("retry")) >= 3)
    .Escalate("LowConfidence", i => i.Confidence.Level == "Low")
    .RequireAuth("SensitiveAction", i => i.Signals.Any(s => s.Description.Contains("sensitive")))
    .Allow("HighConfidence", i => i.Confidence.Level is "High" or "Certain")
    .Build();
var decision = intent.Decide(policy);

// 4) Log and persist (optional)
intent.LogIntentInference(logger, space);
decision.LogPolicyDecision(logger, intent, policy);
await repository.SaveAsync(space);
await historyRepository.SaveAsync(spaceId, intent, decision);
```

For a full runnable example, see the [sample](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample) and [Setup](setup.md).
