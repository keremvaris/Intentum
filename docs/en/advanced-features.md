# Advanced Features (EN)

This page covers advanced features added in recent versions: similarity engines, fluent APIs, caching, testing utilities, and more.

---

## Packages and features overview

Extended packages (beyond Core, Runtime, AI, and providers) and where they are documented on this page:

| Package | What it is | What it does | Section |
|---------|------------|--------------|---------|
| **Intentum.AI.Caching.Redis** | Redis-based embedding cache | `IEmbeddingCache` for multi-node production; store embeddings in Redis | [Redis (Distributed) Cache](#redis-distributed-cache) |
| **Intentum.Clustering** | Intent clustering | Groups intent history for pattern detection; `IIntentClusterer`, `AddIntentClustering` | [Intent Clustering](#intent-clustering) |
| **Intentum.Events** | Webhook / event system | Dispatches intent events (IntentInferred, PolicyDecisionChanged) via HTTP POST; `IIntentEventHandler`, `WebhookIntentEventHandler` | [Webhook / Event System](#webhook--event-system) |
| **Intentum.Experiments** | A/B testing | Traffic split across model/policy variants; `IntentExperiment`, `ExperimentResult`, `AddVariant` | [A/B Experiments](#ab-experiments) |
| **Intentum.MultiTenancy** | Multi-tenancy | Tenant-scoped behavior space repository; `ITenantProvider`, `TenantAwareBehaviorSpaceRepository` | [Multi-tenancy](#multi-tenancy) |
| **Intentum.Explainability** | Intent explainability | Signal contribution scores, human-readable summary; `IIntentExplainer`, `IntentExplainer` | [Intent Explainability](#intent-explainability) |
| **Intentum.Simulation** | Intent simulation | Synthetic behavior spaces for testing; `IBehaviorSpaceSimulator`, `BehaviorSpaceSimulator` | [Intent Simulation](#intent-simulation) |
| **Intentum.Versioning** | Policy versioning | Policy/model version tracking for rollback; `IVersionedPolicy`, `PolicyVersionTracker` | [Policy Versioning](#policy-versioning) |

Core packages (Intentum.Core, Intentum.Runtime, Intentum.AI, providers, Testing, AspNetCore, Persistence, Analytics) are listed in [Architecture](architecture.md) and [README](../README.md).

---

## Similarity Engines

Intentum provides multiple similarity engines for combining embeddings into intent scores.

### SimpleAverageSimilarityEngine (Default)

The default engine that averages all embedding scores equally.

```csharp
var engine = new SimpleAverageSimilarityEngine();
```

### WeightedAverageSimilarityEngine

Applies custom weights to embeddings based on their source (actor:action). Useful when certain behaviors should have more influence.

```csharp
var weights = new Dictionary<string, double>
{
    { "user:login", 2.0 },      // Login is twice as important
    { "user:submit", 1.5 },     // Submit is 1.5x important
    { "user:retry", 0.5 }        // Retry is less important
};
var engine = new WeightedAverageSimilarityEngine(weights, defaultWeight: 1.0);
```

### TimeDecaySimilarityEngine

Applies time-based decay to embeddings. More recent events have higher influence on intent inference.

```csharp
var engine = new TimeDecaySimilarityEngine(
    halfLife: TimeSpan.FromHours(1),
    referenceTime: DateTimeOffset.UtcNow);

// Use with BehaviorSpace to access timestamps
var score = engine.CalculateIntentScoreWithTimeDecay(behaviorSpace, embeddings);
```

### CosineSimilarityEngine

Uses cosine similarity between embedding vectors. Calculates the angle between vectors to measure similarity.

```csharp
var engine = new CosineSimilarityEngine();

// Automatically uses vectors if available, falls back to simple average if not
var score = engine.CalculateIntentScore(embeddings);
```

**Note:** Requires embeddings with vector data. MockEmbeddingProvider automatically generates vectors for testing.

### CompositeSimilarityEngine

Combines multiple similarity engines using weighted combination. Useful for A/B testing or combining different strategies.

```csharp
var engine1 = new SimpleAverageSimilarityEngine();
var engine2 = new WeightedAverageSimilarityEngine(weights);
var engine3 = new CosineSimilarityEngine();

// Equal weights
var composite = new CompositeSimilarityEngine(new[] { engine1, engine2, engine3 });

// Custom weights
var compositeWeighted = new CompositeSimilarityEngine(new[]
{
    (engine1, 1.0),
    (engine2, 2.0),
    (engine3, 1.5)
});
```

---

## Fluent API

### BehaviorSpaceBuilder

Create behavior spaces with a more readable fluent API.

```csharp
var space = new BehaviorSpaceBuilder()
    .WithActor("user")
        .Action("login")
        .Action("retry")
        .Action("submit")
    .WithActor("system")
        .Action("validate")
    .Build();
```

With timestamps and metadata:

```csharp
var space = new BehaviorSpaceBuilder()
    .WithActor("user")
        .Action("login", DateTimeOffset.UtcNow)
        .Action("submit", DateTimeOffset.UtcNow, new Dictionary<string, object> { { "sessionId", "abc123" } })
    .Build();
```

### IntentPolicyBuilder

Create policies with a fluent API.

```csharp
var policy = new IntentPolicyBuilder()
    .Block("ExcessiveRetry", i => i.Signals.Count(s => s.Description.Contains("retry")) >= 3)
    .Escalate("LowConfidence", i => i.Confidence.Level == "Low")
    .RequireAuth("SensitiveAction", i => i.Signals.Any(s => s.Description.Contains("sensitive")))
    .RateLimit("HighFrequency", i => i.Signals.Count > 10)
    .Allow("HighConfidence", i => i.Confidence.Level is "High" or "Certain")
    .Observe("MediumConfidence", i => i.Confidence.Level == "Medium")
    .Warn("LowConfidence", i => i.Confidence.Level == "Low")
    .Build();
```

---

## Policy Decision Types

Intentum supports multiple policy decision types:

- **Allow** — Allow the action to proceed
- **Observe** — Observe the action but allow it to proceed
- **Warn** — Warn about the action but allow it to proceed
- **Block** — Block the action
- **Escalate** — Escalate to a higher level for review
- **RequireAuth** — Require additional authentication before proceeding
- **RateLimit** — Apply rate limiting to the action

All decision types support localization:

```csharp
var localizer = new DefaultLocalizer("tr");
var text = PolicyDecision.Escalate.ToLocalizedString(localizer); // "Yükselt"
```

---

## Policy composition

Combine or extend policies without duplicating rules.

### Inheritance (WithBase)

Evaluate a derived policy after a base policy: base rules first, then derived. First matching rule wins.

```csharp
var basePolicy = new IntentPolicyBuilder()
    .Block("BaseBlock", i => i.Confidence.Level == "Low")
    .Build();
var derived = new IntentPolicyBuilder()
    .Allow("DerivedAllow", i => i.Confidence.Level == "High")
    .Build();
var composed = derived.WithBase(basePolicy);
var decision = intent.Decide(composed);
```

### Merge (multiple policies)

Combine several policies into one; rules from the first policy are evaluated first, then the second, etc.

```csharp
var merged = IntentPolicy.Merge(policyA, policyB, policyC);
var decision = intent.Decide(merged);
```

### A/B policy variants (PolicyVariantSet)

Use different policies per intent (e.g. by experiment or confidence). The selector returns which variant name to use.

```csharp
var variants = new PolicyVariantSet(
    new Dictionary<string, IntentPolicy> { ["control"] = controlPolicy, ["treatment"] = treatmentPolicy },
    intent => intent.Confidence.Score > 0.8 ? "treatment" : "control");
var decision = intent.Decide(variants);
```

---

## Rate Limiting

When policy returns **RateLimit**, use **IRateLimiter** to enforce limits. **MemoryRateLimiter** provides in-memory fixed-window limiting (single-node or development).

### Basic usage

```csharp
var rateLimiter = new MemoryRateLimiter();

// After intent.Decide(policy) returns RateLimit:
var result = await rateLimiter.TryAcquireAsync(
    key: "user-123",
    limit: 100,
    window: TimeSpan.FromMinutes(1));

if (!result.Allowed)
{
    // Return 429 with Retry-After: result.RetryAfter
    return Results.Json(new { error = "Rate limit exceeded" }, statusCode: 429);
}
```

### With policy (DecideWithRateLimitAsync)

```csharp
var options = new RateLimitOptions("user-123", 100, TimeSpan.FromMinutes(1));
var (decision, rateLimitResult) = await intent.DecideWithRateLimitAsync(
    policy,
    rateLimiter,
    options);

if (decision == PolicyDecision.RateLimit && rateLimitResult is { Allowed: false })
{
    // Enforce: return 429, set Retry-After header from rateLimitResult.RetryAfter
}
```

### Reset

```csharp
rateLimiter.Reset("user-123"); // Clear counter (e.g. after admin override)
```

**Note:** **MemoryRateLimiter** is per-process. For multi-node apps, use a distributed rate limiter implementing **IRateLimiter**.

---

## Embedding Caching

Cache embedding results to improve performance and reduce API calls.

### Memory Cache

```csharp
var memoryCache = new MemoryCache(new MemoryCacheOptions());
var cache = new MemoryEmbeddingCache(memoryCache, new MemoryCacheEntryOptions
{
    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
});

var cachedProvider = new CachedEmbeddingProvider(
    new MockEmbeddingProvider(),
    cache);

var model = new LlmIntentModel(cachedProvider, new SimpleAverageSimilarityEngine());
```

### Redis (Distributed) Cache

**What it is:** **Intentum.AI.Caching.Redis** implements `IEmbeddingCache` using Redis (via `IDistributedCache`). Embedding results (behavior key → vector/score) are stored in Redis so that multiple app instances share the same cache.

**What it's for:** Use it when you run more than one node (e.g. multiple web servers or workers). In-memory cache (`MemoryEmbeddingCache`) is per-process; Redis cache is shared, so you avoid duplicate embedding API calls and reduce cost and latency. Typical use: production with OpenAI/Gemini/Mistral where the same behavior keys are requested from different instances.

**How to use it:**

1. Add the package: `Intentum.AI.Caching.Redis`.
2. Ensure a Redis server is available (local or managed, e.g. Azure Cache for Redis).
3. Register the cache and wrap your embedding provider with `CachedEmbeddingProvider`:

```csharp
builder.Services.AddIntentumRedisCache(options =>
{
    options.ConnectionString = "localhost:6379";  // or your Redis connection string
    options.InstanceName = "Intentum:";            // key prefix (default)
    options.DefaultExpiration = TimeSpan.FromHours(24);
});
builder.Services.AddSingleton<IIntentEmbeddingProvider>(sp =>
{
    var provider = new OpenAIEmbeddingProvider(/* ... */);  // or any IIntentEmbeddingProvider
    var cache = sp.GetRequiredService<IEmbeddingCache>();
    return new CachedEmbeddingProvider(provider, cache);
});
```

4. Inject `IIntentEmbeddingProvider` and use it with `LlmIntentModel` as usual. Cache hits are served from Redis; misses call the underlying provider and then store the result.

**Options:** `ConnectionString`, `InstanceName` (key prefix), `DefaultExpiration` (TTL for cached embeddings).

---

## Behavior Space Metadata and Time Windows

### Metadata

Associate metadata with behavior spaces:

```csharp
var space = new BehaviorSpace();
space.SetMetadata("sector", "ESG");
space.SetMetadata("sessionId", "abc123");
space.SetMetadata("userId", "user456");

var sector = space.GetMetadata<string>("sector");
```

### Time Windows

Analyze events within specific time windows:

```csharp
// Get events in the last hour
var recentEvents = space.GetEventsInWindow(TimeSpan.FromHours(1));

// Get events in a specific range
var eventsInRange = space.GetEventsInWindow(startTime, endTime);

// Get time span of all events
var span = space.GetTimeSpan();

// Build vector from time window
var vector = space.ToVector(startTime, endTime);
```

---

## Testing Utilities

The `Intentum.Testing` package provides helpers for writing tests.

### Test Helpers

```csharp
var model = TestHelpers.CreateDefaultModel();
var policy = TestHelpers.CreateDefaultPolicy();
var space = TestHelpers.CreateSimpleSpace();
var spaceWithRetries = TestHelpers.CreateSpaceWithRetries(3);
```

### Assertions

```csharp
// BehaviorSpace assertions
BehaviorSpaceAssertions.ContainsEvent(space, "user", "login");
BehaviorSpaceAssertions.HasEventCount(space, 5);
BehaviorSpaceAssertions.ContainsActor(space, "user");

// Intent assertions
IntentAssertions.HasConfidenceLevel(intent, "High");
IntentAssertions.HasConfidenceScore(intent, 0.7, 1.0);
IntentAssertions.HasSignals(intent);
IntentAssertions.ContainsSignal(intent, "retry");

// Policy decision assertions
PolicyDecisionAssertions.IsOneOf(decision, PolicyDecision.Allow, PolicyDecision.Observe);
PolicyDecisionAssertions.IsAllow(decision);
PolicyDecisionAssertions.IsNotBlock(decision);
```

---

## Intent Analytics & Reporting

The `Intentum.Analytics` package provides analytics and reporting over intent history (requires `IIntentHistoryRepository`).

### Setup

```csharp
// After adding Intentum.Persistence (e.g. EF Core) and Intentum.Analytics
builder.Services.AddIntentumPersistence(options => options.UseSqlServer(connectionString));
builder.Services.AddIntentAnalytics();
```

### Confidence trends

```csharp
var analytics = serviceProvider.GetRequiredService<IIntentAnalytics>();

var trends = await analytics.GetConfidenceTrendsAsync(
    start: DateTimeOffset.UtcNow.AddDays(-30),
    end: DateTimeOffset.UtcNow,
    bucketSize: TimeSpan.FromDays(1));

foreach (var point in trends)
    Console.WriteLine($"{point.BucketStart:yyyy-MM-dd} {point.ConfidenceLevel}: {point.Count} (avg score {point.AverageScore:F2})");
```

### Decision distribution

```csharp
var report = await analytics.GetDecisionDistributionAsync(start, end);
Console.WriteLine($"Total: {report.TotalCount}");
foreach (var (decision, count) in report.CountByDecision)
    Console.WriteLine($"  {decision}: {count}");
```

### Anomaly detection

```csharp
var anomalies = await analytics.DetectAnomaliesAsync(start, end, TimeSpan.FromHours(1));
foreach (var a in anomalies)
    Console.WriteLine($"{a.Type}: {a.Description} (severity {a.Severity:F2})");
```

### Dashboard summary

```csharp
var summary = await analytics.GetSummaryAsync(start, end, TimeSpan.FromDays(1));
// summary.TotalInferences, summary.UniqueBehaviorSpaces, summary.ConfidenceTrend,
// summary.DecisionDistribution, summary.Anomalies
```

### Export to JSON / CSV

```csharp
var json = await analytics.ExportToJsonAsync(start, end);
var csv = await analytics.ExportToCsvAsync(start, end);
```

---

## ASP.NET Core Middleware

Automatically observe HTTP request behaviors.

### Setup

```csharp
// Program.cs
builder.Services.AddIntentum();
// or with custom BehaviorSpace
builder.Services.AddIntentum(customBehaviorSpace);

app.UseIntentumBehaviorObservation(new BehaviorObservationOptions
{
    Enabled = true,
    IncludeHeaders = false,
    GetActor = ctx => "http",
    GetAction = ctx => $"{ctx.Request.Method.ToLowerInvariant()}_{ctx.Request.Path.Value?.Replace("/", "_")}"
});
```

---

## Observability

The `Intentum.Observability` package provides OpenTelemetry metrics.

### Setup

```csharp
var model = new ObservableIntentModel(
    new LlmIntentModel(embeddingProvider, similarityEngine));

// Metrics are automatically recorded
var intent = model.Infer(space);

// Or use extension method for policy decisions
var decision = intent.DecideWithMetrics(policy);
```

### Metrics

- `intentum.intent.inference.count` — Number of intent inferences
- `intentum.intent.inference.duration` — Duration of inference operations (ms)
- `intentum.intent.confidence.score` — Confidence scores
- `intentum.policy.decision.count` — Number of policy decisions
- `intentum.behavior.space.size` — Size of behavior spaces

---

## Batch Processing

Process multiple behavior spaces efficiently in batch.

### BatchIntentModel

```csharp
var model = new LlmIntentModel(embeddingProvider, similarityEngine);
var batchModel = new BatchIntentModel(model);

// Synchronous batch processing
var spaces = new[] { space1, space2, space3 };
var intents = batchModel.InferBatch(spaces);

// Async batch processing with cancellation support
var intentsAsync = await batchModel.InferBatchAsync(spaces, cancellationToken);
```

### Streaming: InferMany / InferManyAsync

For lazy or async streaming over many behavior spaces, use the `Intentum.AI` extension methods (no batch list in memory):

```csharp
using Intentum.AI;

// Lazy sync stream: yields one intent per space as you enumerate
foreach (var intent in model.InferMany(spaces))
    Process(intent);

// Async stream: yields intents as spaces are enumerated (e.g. from DB or queue)
await foreach (var intent in model.InferManyAsync(SpacesFromDbAsync(), cancellationToken))
    await ProcessAsync(intent);
```

### Behavior vector caching and pre-computed vector

- **ToVector() cache:** `BehaviorSpace.ToVector()` is computed once and cached until you call `Observe()` again, so repeated inference on the same space reuses the vector.
- **Pre-computed vector in Infer:** When you already have a `BehaviorVector` (e.g. from persistence or a snapshot), pass it to avoid recomputation: `model.Infer(space, precomputedVector)`.

---

## Persistence

Store behavior spaces and intent history for analytics and auditing. Implementations: **Entity Framework Core**, **Redis**, **MongoDB**.

### Entity Framework Core

```csharp
// Setup
builder.Services.AddIntentumPersistence(options =>
    options.UseSqlServer(connectionString));

// Or use in-memory for testing
builder.Services.AddIntentumPersistenceInMemory("TestDb");

// Usage
var repository = serviceProvider.GetRequiredService<IBehaviorSpaceRepository>();
var id = await repository.SaveAsync(behaviorSpace);
var retrieved = await repository.GetByIdAsync(id);

// Query by metadata
var spaces = await repository.GetByMetadataAsync("sector", "ESG");

// Query by time window
var recentSpaces = await repository.GetByTimeWindowAsync(
    DateTimeOffset.UtcNow.AddHours(-24),
    DateTimeOffset.UtcNow);
```

### Intent History

```csharp
var historyRepository = serviceProvider.GetRequiredService<IIntentHistoryRepository>();

// Save intent result
await historyRepository.SaveAsync(behaviorSpaceId, intent, decision);

// Query history
var history = await historyRepository.GetByBehaviorSpaceIdAsync(behaviorSpaceId);
var highConfidence = await historyRepository.GetByConfidenceLevelAsync("High");
var blocked = await historyRepository.GetByDecisionAsync(PolicyDecision.Block);
```

### Redis

Add `Intentum.Persistence.Redis` and register with a Redis connection:

```csharp
using Intentum.Persistence.Redis;
using StackExchange.Redis;

var redis = ConnectionMultiplexer.Connect("localhost");
builder.Services.AddIntentumPersistenceRedis(redis, keyPrefix: "intentum:");
```

### MongoDB

Add `Intentum.Persistence.MongoDB` and register with an `IMongoDatabase`:

```csharp
using Intentum.Persistence.MongoDB;
using MongoDB.Driver;

var client = new MongoClient(connectionString);
var database = client.GetDatabase("intentum");
builder.Services.AddIntentumPersistenceMongoDB(database,
    behaviorSpaceCollectionName: "behaviorspaces",
    intentHistoryCollectionName: "intenthistory");
```

---

## Webhook / Event System

**What it is:** **Intentum.Events** lets you send intent-related events (e.g. after inference or policy decision) to external systems via HTTP POST. It provides `IIntentEventHandler` and a built-in `WebhookIntentEventHandler` that posts a JSON payload to one or more webhook URLs with configurable retry.

**What it's for:** Use it when you need to notify another service when an intent is inferred or a decision is made (e.g. analytics, audit, downstream workflows). Typical use: after `intent = model.Infer(space)` and `decision = intent.Decide(policy)`, call `HandleAsync(payload, IntentumEventType.IntentInferred)` so your webhook endpoint receives the intent name, confidence, decision, and timestamp.

**How to use it:**

1. Add the package: `Intentum.Events`.
2. Register events and webhooks:

```csharp
builder.Services.AddIntentumEvents(options =>
{
    options.AddWebhook("https://api.example.com/webhooks/intent", events: new[] { "IntentInferred", "PolicyDecisionChanged" });
    options.RetryCount = 3;  // retries on HTTP failure (exponential backoff)
});
```

3. After inference, build a payload and call the handler:

```csharp
var intent = model.Infer(space);
var decision = intent.Decide(policy);
var payload = new IntentEventPayload(behaviorSpaceId: "id", intent, decision, DateTimeOffset.UtcNow);
await eventHandler.HandleAsync(payload, IntentumEventType.IntentInferred, cancellationToken);
```

The webhook receives a POST with a JSON body containing `BehaviorSpaceId`, `IntentName`, `ConfidenceLevel`, `ConfidenceScore`, `Decision`, `RecordedAt`, `EventType`. Failed POSTs are retried according to `RetryCount`.

---

## Intent Clustering

**What it is:** **Intentum.Clustering** groups intent history records for analysis. It provides `IIntentClusterer` with two strategies: **ClusterByPatternAsync** (group by confidence level + policy decision) and **ClusterByConfidenceScoreAsync** (split into k score buckets). Each cluster has an id, label, record ids, count, and a summary (average/min/max score).

**What it's for:** Use it when you store intent history (e.g. via `IIntentHistoryRepository`) and want to see patterns: how many intents ended in Allow vs Block, or how confidence is distributed (low/medium/high bands). Typical use: analytics dashboards, anomaly detection, or tuning policy thresholds.

**How to use it:**

1. Add the package: `Intentum.Clustering`. You need **Intentum.Persistence** (and a repository that implements `IIntentHistoryRepository`) so you have `IntentHistoryRecord` data.
2. Register the clusterer:

```csharp
builder.Services.AddIntentClustering();
```

3. Resolve `IIntentClusterer` and your `IIntentHistoryRepository`. Fetch records (e.g. by time window), then cluster:

```csharp
var clusterer = serviceProvider.GetRequiredService<IIntentClusterer>();
var historyRepo = serviceProvider.GetRequiredService<IIntentHistoryRepository>();
var records = await historyRepo.GetByTimeWindowAsync(start, end);

// Group by (ConfidenceLevel, Decision) — e.g. "High / Allow", "Medium / Observe"
var patternClusters = await clusterer.ClusterByPatternAsync(records);
foreach (var c in patternClusters)
    Console.WriteLine($"{c.Label}: {c.Count} intents (avg score {c.Summary?.AverageConfidenceScore:F2})");

// Split into k buckets by confidence score (e.g. low / medium / high)
var scoreClusters = await clusterer.ClusterByConfidenceScoreAsync(records, k: 3);
foreach (var c in scoreClusters)
    Console.WriteLine($"{c.Label}: {c.Count} intents");
```

---

## Intent Explainability

**What it is:** **Intentum.Explainability** explains how an intent was inferred: which signals (behaviors) contributed how much to the final confidence. It provides `IIntentExplainer` and `IntentExplainer`, which compute **signal contribution** (each signal’s weight as a percentage of the total) and a **human-readable explanation** string.

**What it's for:** Use it when you need to show users or auditors *why* a given intent/confidence was returned (e.g. “login and submit contributed 60% and 40%”). Typical use: debug UI, compliance, or support tools.

**How to use it:**

1. Add the package: `Intentum.Explainability`.
2. Create an explainer (or register it in DI) and call it after inference:

```csharp
var explainer = new IntentExplainer();

// Per-signal contribution (source, description, weight, percentage)
var contributions = explainer.GetSignalContributions(intent);
foreach (var c in contributions)
    Console.WriteLine($"{c.Description}: {c.ContributionPercent:F0}%");

// Single summary sentence
var text = explainer.GetExplanation(intent, maxSignals: 5);
// e.g. "Intent \"AI-Inferred-Intent\" inferred with confidence High (0.85). Top contributors: user:login (45%); user:submit (35%); ..."
```

No extra configuration; it works from the `Intent` and its `Signals` and `Confidence`.

---

## Intent Simulation

**What it is:** **Intentum.Simulation** generates **synthetic behavior spaces** for testing and demos. It provides `IBehaviorSpaceSimulator` and `BehaviorSpaceSimulator` with two methods: **FromSequence** (build a space from a fixed list of actor/action pairs) and **GenerateRandom** (build a space with random events from given actors and actions, with an optional seed for reproducibility).

**What it's for:** Use it when you need many behavior spaces in tests without hand-writing each `BehaviorSpace` (e.g. load tests, property-based tests, or demos). Typical use: unit tests that feed spaces into `model.Infer(space)` or `experiment.RunAsync(spaces)`.

**How to use it:**

1. Add the package: `Intentum.Simulation`.
2. Create a simulator and generate spaces:

```csharp
var simulator = new BehaviorSpaceSimulator();

// Fixed sequence — events get timestamps 1 second apart (or pass baseTime)
var space = simulator.FromSequence(new[] { ("user", "login"), ("user", "submit") });

// Random space — useful for stress tests or demos; use randomSeed for reproducible tests
var randomSpace = simulator.GenerateRandom(
    actors: new[] { "user", "system" },
    actions: new[] { "login", "submit", "retry", "cancel" },
    eventCount: 10,
    randomSeed: 42);
```

3. Use the returned `BehaviorSpace` with your `IIntentModel`, policy, or `IntentExperiment` as usual.

---

## A/B Experiments

**What it is:** **Intentum.Experiments** runs **A/B tests** over intent inference: you define multiple **variants** (each is a model + policy pair), set a **traffic split** (e.g. 50% control, 50% test), and run a list of behavior spaces through the experiment. Each space is assigned to one variant by the split; you get back one `ExperimentResult` per space (variant name, intent, decision).

**What it's for:** Use it when you want to compare two (or more) models or policies on the same traffic (e.g. “new policy vs current”). Typical use: rolling out a new policy or model and measuring Allow/Block/Observe distribution per variant.

**How to use it:**

1. Add the package: `Intentum.Experiments`.
2. Build an experiment with at least two variants and a traffic split (percentages must sum to 100):

```csharp
var experiment = new IntentExperiment()
    .AddVariant("control", controlModel, controlPolicy)
    .AddVariant("test", testModel, testPolicy)
    .SplitTraffic(50, 50);  // 50% control, 50% test; if omitted, split is even
```

3. Run the experiment with a list of behavior spaces (e.g. from production sampling or simulation):

```csharp
var results = await experiment.RunAsync(behaviorSpaces, cancellationToken);
foreach (var r in results)
    Console.WriteLine($"{r.VariantName}: {r.Intent.Confidence.Level} → {r.Decision}");
```

You can then aggregate by `VariantName` to compare metrics (e.g. Block rate, average confidence) between control and test.

---

## Multi-tenancy

**What it is:** **Intentum.MultiTenancy** provides a **tenant-scoped behavior space repository**. `TenantAwareBehaviorSpaceRepository` wraps any `IBehaviorSpaceRepository`: on save it injects the current tenant id (from `ITenantProvider`) into the behavior space metadata; on read/delete it returns only data belonging to the current tenant.

**What it's for:** Use it when your app serves multiple tenants (e.g. organizations or customers) and you must isolate behavior spaces and intent history per tenant. Typical use: SaaS backends where each request has a tenant context (e.g. from HTTP header or claims).

**How to use it:**

1. Add the package: `Intentum.MultiTenancy`.
2. Implement `ITenantProvider` to return the current tenant id (e.g. from `IHttpContextAccessor`, claims, or ambient context). Register it and the tenant-aware repository:

```csharp
builder.Services.AddSingleton<ITenantProvider, MyTenantProvider>();  // your implementation
builder.Services.AddTenantAwareBehaviorSpaceRepository();
```

3. Register an inner `IBehaviorSpaceRepository` (e.g. EF or MongoDB) as usual. When you need tenant isolation, inject `TenantAwareBehaviorSpaceRepository` instead of `IBehaviorSpaceRepository`:

```csharp
// In a request with tenant context (e.g. middleware sets tenant)
var repo = serviceProvider.GetRequiredService<TenantAwareBehaviorSpaceRepository>();
await repo.SaveAsync(space, cancellationToken);   // space gets metadata "TenantId" = current tenant
var list = await repo.GetByTimeWindowAsync(start, end, cancellationToken);  // only current tenant's spaces
```

Tenant id is stored in metadata under the key `TenantId`. If `GetCurrentTenantId()` returns null or empty, the wrapper does not filter (all data is visible).

---

## Policy Versioning

**What it is:** **Intentum.Versioning** tracks **policy versions** so you can roll back or roll forward. It provides `IVersionedPolicy` (a policy plus a version string), `VersionedPolicy` (record implementation), and `PolicyVersionTracker`, which holds a list of versioned policies and a “current” index. You can add versions, switch current, and call `Rollback()` / `Rollforward()` to move the current index.

**What it's for:** Use it when you deploy policy changes and want to quickly revert to a previous version without redeploying (e.g. a new rule causes too many Blocks). Typical use: admin API or feature flag that switches the active policy version.

**How to use it:**

1. Add the package: `Intentum.Versioning`.
2. Wrap policies with `VersionedPolicy` and add them to a tracker (e.g. in DI as singleton):

```csharp
var tracker = new PolicyVersionTracker();
tracker.Add(new VersionedPolicy("1.0", policyV1));
tracker.Add(new VersionedPolicy("2.0", policyV2));  // current is now 2.0
```

3. Use the tracker’s current policy when deciding:

```csharp
var versioned = tracker.Current;
var policy = versioned?.Policy ?? fallbackPolicy;
var decision = intent.Decide(policy);
```

4. Roll back or forward when needed:

```csharp
if (tracker.Rollback())   // current moves to previous (e.g. 2.0 → 1.0)
    logger.LogInformation("Rolled back to {Version}", tracker.Current?.Version);
if (tracker.Rollforward())  // current moves to next (e.g. 1.0 → 2.0)
    logger.LogInformation("Rolled forward to {Version}", tracker.Current?.Version);
```

You can also `SetCurrent(index)` to jump to a specific version by index. Version strings are arbitrary (e.g. `"1.0"`, `"2024-01-15"`); `CompareVersions(a, b)` is provided for ordering.

---

## See also

- [API Reference](api.md) — Full API documentation
- [Setup](setup.md) — Getting started guide
- [Scenarios](scenarios.md) — Usage examples
