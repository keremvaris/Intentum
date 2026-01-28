# Advanced Features (EN)

This page covers advanced features added in recent versions: similarity engines, fluent APIs, caching, testing utilities, and more.

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

For multi-node production, use **Intentum.AI.Caching.Redis** to store embeddings in Redis:

```csharp
builder.Services.AddIntentumRedisCache(options =>
{
    options.ConnectionString = "localhost:6379";
    options.DefaultExpiration = TimeSpan.FromHours(24);
});
builder.Services.AddSingleton<IIntentEmbeddingProvider>(sp =>
{
    var provider = new MockEmbeddingProvider();
    var cache = sp.GetRequiredService<IEmbeddingCache>();
    return new CachedEmbeddingProvider(provider, cache);
});
```

Requires the `Intentum.AI.Caching.Redis` package and a Redis server.

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

---

## Persistence

Store behavior spaces and intent history for analytics and auditing.

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

---

## Webhook / Event System

The `Intentum.Events` package dispatches intent events (IntentInferred, PolicyDecisionChanged) to webhook URLs via HTTP POST with retry.

```csharp
builder.Services.AddIntentumEvents(options =>
{
    options.AddWebhook("https://api.example.com/webhooks/intent", events: new[] { "IntentInferred", "PolicyDecisionChanged" });
    options.RetryCount = 3;
});
// Then inject IIntentEventHandler and call HandleAsync(payload, IntentumEventType.IntentInferred) after inference.
```

---

## Intent Clustering

The `Intentum.Clustering` package groups intent history records for pattern detection.

```csharp
builder.Services.AddIntentClustering();
var clusterer = serviceProvider.GetRequiredService<IIntentClusterer>();

var records = await historyRepository.GetByTimeWindowAsync(start, end);
var clusters = await clusterer.ClusterByPatternAsync(records);
foreach (var c in clusters)
    Console.WriteLine($"{c.Label}: {c.Count} intents");

var scoreClusters = await clusterer.ClusterByConfidenceScoreAsync(records, k: 3);
```

---

## Intent Explainability

The `Intentum.Explainability` package explains how an intent was inferred (signal contributions, human-readable summary).

```csharp
var explainer = new IntentExplainer();
var contributions = explainer.GetSignalContributions(intent);
var text = explainer.GetExplanation(intent, maxSignals: 5);
```

---

## Intent Simulation

The `Intentum.Simulation` package generates synthetic behavior spaces for testing.

```csharp
var simulator = new BehaviorSpaceSimulator();
var space = simulator.FromSequence(new[] { ("user", "login"), ("user", "submit") });
var randomSpace = simulator.GenerateRandom(actors: new[] { "user", "system" }, actions: new[] { "a", "b" }, eventCount: 10, randomSeed: 42);
```

---

## A/B Experiments

The `Intentum.Experiments` package runs A/B tests with traffic splitting across model/policy variants.

```csharp
var experiment = new IntentExperiment()
    .AddVariant("control", controlModel, controlPolicy)
    .AddVariant("test", testModel, testPolicy)
    .SplitTraffic(50, 50);
var results = await experiment.RunAsync(behaviorSpaces);
```

---

## Multi-tenancy

The `Intentum.MultiTenancy` package provides tenant-scoped behavior space repository. Register `ITenantProvider` and `AddTenantAwareBehaviorSpaceRepository()`; inject `TenantAwareBehaviorSpaceRepository` when tenant scope is needed.

---

## Policy Versioning

The `Intentum.Versioning` package tracks policy versions for rollback. Use `VersionedPolicy(version, policy)`, `PolicyVersionTracker.Add()`, and `Rollback()` / `Rollforward()`.

---

## See also

- [API Reference](api.md) — Full API documentation
- [Setup](setup.md) — Getting started guide
- [Scenarios](scenarios.md) — Usage examples
