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
| **BehaviorSpace** | Container for observed events. You call `.Observe(actor, action)` (e.g. `"user"`, `"login"`). Use `.ToVector()` to get a behavior vector for inference. |
| **Intent** | Result of inference: confidence level, score, and signals (contributing behaviors with weights). |
| **IntentConfidence** | Part of Intent: `Level` (string) and `Score` (0–1). |
| **IntentEvaluator** | Evaluates intent against criteria; used internally by the model. |

**Where to start:** Create a `BehaviorSpace`, call `.Observe(...)` for each event, then pass the space to your intent model’s `Infer(space)`.

---

## Runtime (`Intentum.Runtime`)

| Type | What it does |
|------|----------------|
| **IntentPolicy** | Ordered list of rules. Add rules with `.AddRule(PolicyRule(...))`. First matching rule wins. |
| **IntentPolicyBuilder** | Fluent builder for creating IntentPolicy instances. Use `.Allow(...)`, `.Block(...)`, `.Escalate(...)`, etc. |
| **PolicyRule** | Name + condition (e.g. lambda on `Intent`) + **PolicyDecision** (Allow, Observe, Warn, Block, Escalate, RequireAuth, RateLimit). |
| **PolicyDecision** | Decision enum: **Allow**, **Observe**, **Warn**, **Block**, **Escalate**, **RequireAuth**, **RateLimit**. |
| **IntentPolicyEngine** | Evaluates an intent against a policy; returns a **PolicyDecision**. |
| **RuntimeExtensions.Decide** | Extension: `intent.Decide(policy)` — runs the policy and returns the decision. |
| **RuntimeExtensions.DecideWithRateLimit** / **DecideWithRateLimitAsync** | When decision is RateLimit, checks **IRateLimiter** and returns **RateLimitResult** (Allowed, CurrentCount, Limit, RetryAfter). |
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
| **IIntentSimilarityEngine** | Combines embeddings into a single similarity score. **SimpleAverageSimilarityEngine** is the built-in option. |
| **WeightedAverageSimilarityEngine** | Similarity engine that applies weights to embeddings based on their source (actor:action). Useful when certain behaviors should have more influence. |
| **TimeDecaySimilarityEngine** | Similarity engine that applies time-based decay to embeddings. More recent events have higher influence on intent inference. |
| **CosineSimilarityEngine** | Similarity engine that uses cosine similarity between embedding vectors. Falls back to simple average if vectors are not available. |
| **CompositeSimilarityEngine** | Combines multiple similarity engines using weighted combination. Useful for A/B testing. |
| **LlmIntentModel** | Takes an embedding provider + similarity engine; **Infer(BehaviorSpace)** returns an **Intent** with confidence and signals. |
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
| **BehaviorSpaceRepository** (EF Core) | Entity Framework Core implementation of IBehaviorSpaceRepository. |
| **IntentHistoryRepository** (EF Core) | Entity Framework Core implementation of IIntentHistoryRepository. |

**Where to start:** Add `Intentum.Persistence.EntityFramework` package, configure DbContext, and use `AddIntentumPersistence()` extension method.

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
