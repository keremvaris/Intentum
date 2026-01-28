# Gelişmiş Özellikler (TR)

Bu sayfa son versiyonlarda eklenen gelişmiş özellikleri kapsar: similarity engine'ler, fluent API'ler, caching, test utilities ve daha fazlası.

---

## Similarity Engine'ler

Intentum, embedding'leri intent skorlarına dönüştürmek için birden fazla similarity engine sağlar.

### SimpleAverageSimilarityEngine (Varsayılan)

Tüm embedding skorlarını eşit şekilde ortalaması alan varsayılan engine.

```csharp
var engine = new SimpleAverageSimilarityEngine();
```

### WeightedAverageSimilarityEngine

Embedding'lere kaynaklarına (actor:action) göre özel ağırlıklar uygular. Belirli davranışların daha fazla etkisi olması gerektiğinde kullanışlıdır.

```csharp
var weights = new Dictionary<string, double>
{
    { "user:login", 2.0 },      // Login iki kat daha önemli
    { "user:submit", 1.5 },     // Submit 1.5 kat önemli
    { "user:retry", 0.5 }        // Retry daha az önemli
};
var engine = new WeightedAverageSimilarityEngine(weights, defaultWeight: 1.0);
```

### TimeDecaySimilarityEngine

Embedding'lere zaman bazlı decay uygular. Yakın zamandaki event'ler intent çıkarımında daha yüksek etkiye sahiptir.

```csharp
var engine = new TimeDecaySimilarityEngine(
    halfLife: TimeSpan.FromHours(1),
    referenceTime: DateTimeOffset.UtcNow);

// Timestamp'lere erişmek için BehaviorSpace ile kullan
var score = engine.CalculateIntentScoreWithTimeDecay(behaviorSpace, embeddings);
```

### CosineSimilarityEngine

Embedding vector'ları arasındaki cosine similarity kullanır. Vector'lar arasındaki açıyı ölçer.

```csharp
var engine = new CosineSimilarityEngine();

// Vector varsa otomatik kullanır, yoksa simple average'a düşer
var score = engine.CalculateIntentScore(embeddings);
```

**Not:** Vector verisi gerektirir. MockEmbeddingProvider test için otomatik vector üretir.

### CompositeSimilarityEngine

Birden fazla similarity engine'i weighted combination ile birleştirir. A/B testing veya farklı stratejileri birleştirmek için kullanışlıdır.

```csharp
var engine1 = new SimpleAverageSimilarityEngine();
var engine2 = new WeightedAverageSimilarityEngine(weights);
var engine3 = new CosineSimilarityEngine();

// Eşit ağırlıklar
var composite = new CompositeSimilarityEngine(new[] { engine1, engine2, engine3 });

// Özel ağırlıklar
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

Daha okunabilir fluent API ile behavior space'ler oluşturun.

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

Timestamp ve metadata ile:

```csharp
var space = new BehaviorSpaceBuilder()
    .WithActor("user")
        .Action("login", DateTimeOffset.UtcNow)
        .Action("submit", DateTimeOffset.UtcNow, new Dictionary<string, object> { { "sessionId", "abc123" } })
    .Build();
```

### IntentPolicyBuilder

Fluent API ile policy'ler oluşturun.

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

## Policy Decision Tipleri

Intentum birden fazla policy decision tipi destekler:

- **Allow** — Aksiyona izin ver
- **Observe** — Aksiyonu izle ama devam etmesine izin ver
- **Warn** — Aksiyon hakkında uyar ama devam etmesine izin ver
- **Block** — Aksiyonu engelle
- **Escalate** — Daha yüksek seviyeye yükselt
- **RequireAuth** — Devam etmeden önce ek kimlik doğrulama gerektir
- **RateLimit** — Aksiyona hız sınırı uygula

Tüm decision tipleri lokalizasyon destekler:

```csharp
var localizer = new DefaultLocalizer("tr");
var text = PolicyDecision.Escalate.ToLocalizedString(localizer); // "Yükselt"
```

---

## Rate Limiting

Policy **RateLimit** döndüğünde limiti uygulamak için **IRateLimiter** kullanın. **MemoryRateLimiter** in-memory fixed-window limit sağlar (tek node veya geliştirme).

### Temel kullanım

```csharp
var rateLimiter = new MemoryRateLimiter();

// intent.Decide(policy) RateLimit döndükten sonra:
var result = await rateLimiter.TryAcquireAsync(
    key: "user-123",
    limit: 100,
    window: TimeSpan.FromMinutes(1));

if (!result.Allowed)
{
    // 429 dön, Retry-After: result.RetryAfter
    return Results.Json(new { error = "Rate limit exceeded" }, statusCode: 429);
}
```

### Policy ile (DecideWithRateLimitAsync)

```csharp
var options = new RateLimitOptions("user-123", 100, TimeSpan.FromMinutes(1));
var (decision, rateLimitResult) = await intent.DecideWithRateLimitAsync(
    policy,
    rateLimiter,
    options);

if (decision == PolicyDecision.RateLimit && rateLimitResult is { Allowed: false })
{
    // Uygula: 429 dön, Retry-After header'ı rateLimitResult.RetryAfter ile set et
}
```

### Reset

```csharp
rateLimiter.Reset("user-123"); // Sayacı temizle (örn. admin override sonrası)
```

**Not:** **MemoryRateLimiter** process bazlıdır. Çok node'lu uygulamalarda **IRateLimiter** implemente eden dağıtık bir rate limiter kullanın.

---

## Embedding Caching

Performansı artırmak ve API çağrılarını azaltmak için embedding sonuçlarını cache'leyin.

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

### Redis (Dağıtık) Cache

Çok node'lu production için **Intentum.AI.Caching.Redis** ile embedding'leri Redis'te saklayın:

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

`Intentum.AI.Caching.Redis` paketi ve Redis sunucusu gerekir.

---

## Behavior Space Metadata ve Zaman Pencereleri

### Metadata

Behavior space'lerle metadata ilişkilendirin:

```csharp
var space = new BehaviorSpace();
space.SetMetadata("sector", "ESG");
space.SetMetadata("sessionId", "abc123");
space.SetMetadata("userId", "user456");

var sector = space.GetMetadata<string>("sector");
```

### Zaman Pencereleri

Belirli zaman pencereleri içindeki event'leri analiz edin:

```csharp
// Son bir saatteki event'leri al
var recentEvents = space.GetEventsInWindow(TimeSpan.FromHours(1));

// Belirli bir aralıktaki event'leri al
var eventsInRange = space.GetEventsInWindow(startTime, endTime);

// Tüm event'lerin zaman aralığını al
var span = space.GetTimeSpan();

// Zaman penceresinden vector oluştur
var vector = space.ToVector(startTime, endTime);
```

---

## Testing Utilities

`Intentum.Testing` paketi test yazmak için helper'lar sağlar.

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

`Intentum.Analytics` paketi intent history üzerinde analytics ve raporlama sağlar (`IIntentHistoryRepository` gerekir).

### Kurulum

```csharp
// Intentum.Persistence (örn. EF Core) ve Intentum.Analytics eklendikten sonra
builder.Services.AddIntentumPersistence(options => options.UseSqlServer(connectionString));
builder.Services.AddIntentAnalytics();
```

### Confidence trendleri

```csharp
var analytics = serviceProvider.GetRequiredService<IIntentAnalytics>();

var trends = await analytics.GetConfidenceTrendsAsync(
    start: DateTimeOffset.UtcNow.AddDays(-30),
    end: DateTimeOffset.UtcNow,
    bucketSize: TimeSpan.FromDays(1));

foreach (var point in trends)
    Console.WriteLine($"{point.BucketStart:yyyy-MM-dd} {point.ConfidenceLevel}: {point.Count} (ortalama skor {point.AverageScore:F2})");
```

### Decision dağılımı

```csharp
var report = await analytics.GetDecisionDistributionAsync(start, end);
Console.WriteLine($"Toplam: {report.TotalCount}");
foreach (var (decision, count) in report.CountByDecision)
    Console.WriteLine($"  {decision}: {count}");
```

### Anomali tespiti

```csharp
var anomalies = await analytics.DetectAnomaliesAsync(start, end, TimeSpan.FromHours(1));
foreach (var a in anomalies)
    Console.WriteLine($"{a.Type}: {a.Description} (şiddet {a.Severity:F2})");
```

### Dashboard özeti

```csharp
var summary = await analytics.GetSummaryAsync(start, end, TimeSpan.FromDays(1));
// summary.TotalInferences, summary.UniqueBehaviorSpaces, summary.ConfidenceTrend,
// summary.DecisionDistribution, summary.Anomalies
```

### JSON / CSV export

```csharp
var json = await analytics.ExportToJsonAsync(start, end);
var csv = await analytics.ExportToCsvAsync(start, end);
```

---

## ASP.NET Core Middleware

HTTP request davranışlarını otomatik olarak gözlemleyin.

### Kurulum

```csharp
// Program.cs
builder.Services.AddIntentum();
// veya özel BehaviorSpace ile
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

`Intentum.Observability` paketi OpenTelemetry metrikleri sağlar.

### Kurulum

```csharp
var model = new ObservableIntentModel(
    new LlmIntentModel(embeddingProvider, similarityEngine));

// Metrikler otomatik olarak kaydedilir
var intent = model.Infer(space);

// Veya policy decision'lar için extension method kullan
var decision = intent.DecideWithMetrics(policy);
```

### Metrikler

- `intentum.intent.inference.count` — Intent çıkarım sayısı
- `intentum.intent.inference.duration` — Çıkarım işlemlerinin süresi (ms)
- `intentum.intent.confidence.score` — Güven skorları
- `intentum.policy.decision.count` — Policy karar sayısı
- `intentum.behavior.space.size` — Behavior space boyutları

---

## Batch Processing

Birden fazla behavior space'i verimli şekilde batch olarak işleyin.

### BatchIntentModel

```csharp
var model = new LlmIntentModel(embeddingProvider, similarityEngine);
var batchModel = new BatchIntentModel(model);

// Senkron batch processing
var spaces = new[] { space1, space2, space3 };
var intents = batchModel.InferBatch(spaces);

// Async batch processing, cancellation desteği ile
var intentsAsync = await batchModel.InferBatchAsync(spaces, cancellationToken);
```

---

## Persistence

Analytics ve auditing için behavior space'leri ve intent history'yi saklayın.

### Entity Framework Core

```csharp
// Kurulum
builder.Services.AddIntentumPersistence(options =>
    options.UseSqlServer(connectionString));

// Veya test için in-memory kullan
builder.Services.AddIntentumPersistenceInMemory("TestDb");

// Kullanım
var repository = serviceProvider.GetRequiredService<IBehaviorSpaceRepository>();
var id = await repository.SaveAsync(behaviorSpace);
var retrieved = await repository.GetByIdAsync(id);

// Metadata ile sorgula
var spaces = await repository.GetByMetadataAsync("sector", "ESG");

// Zaman penceresi ile sorgula
var recentSpaces = await repository.GetByTimeWindowAsync(
    DateTimeOffset.UtcNow.AddHours(-24),
    DateTimeOffset.UtcNow);
```

### Intent History

```csharp
var historyRepository = serviceProvider.GetRequiredService<IIntentHistoryRepository>();

// Intent sonucunu kaydet
await historyRepository.SaveAsync(behaviorSpaceId, intent, decision);

// History sorgula
var history = await historyRepository.GetByBehaviorSpaceIdAsync(behaviorSpaceId);
var highConfidence = await historyRepository.GetByConfidenceLevelAsync("High");
var blocked = await historyRepository.GetByDecisionAsync(PolicyDecision.Block);
```

---

## Webhook / Event Sistemi

`Intentum.Events` paketi intent event'lerini (IntentInferred, PolicyDecisionChanged) HTTP POST ile webhook URL'lerine retry ile gönderir.

```csharp
builder.Services.AddIntentumEvents(options =>
{
    options.AddWebhook("https://api.example.com/webhooks/intent", events: new[] { "IntentInferred", "PolicyDecisionChanged" });
    options.RetryCount = 3;
});
// IIntentEventHandler enjekte edip inference sonrası HandleAsync(payload, IntentumEventType.IntentInferred) çağırın.
```

---

## Intent Clustering

`Intentum.Clustering` paketi intent history kayıtlarını pattern tespiti için gruplar.

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

`Intentum.Explainability` paketi intent'in nasıl çıkarıldığını açıklar (sinyal katkıları, özet metin).

```csharp
var explainer = new IntentExplainer();
var contributions = explainer.GetSignalContributions(intent);
var text = explainer.GetExplanation(intent, maxSignals: 5);
```

---

## Intent Simulation

`Intentum.Simulation` paketi test için sentetik behavior space üretir.

```csharp
var simulator = new BehaviorSpaceSimulator();
var space = simulator.FromSequence(new[] { ("user", "login"), ("user", "submit") });
var randomSpace = simulator.GenerateRandom(actors: new[] { "user", "system" }, actions: new[] { "a", "b" }, eventCount: 10, randomSeed: 42);
```

---

## A/B Experiments

`Intentum.Experiments` paketi model/policy varyantları arasında traffic split ile A/B testi çalıştırır.

```csharp
var experiment = new IntentExperiment()
    .AddVariant("control", controlModel, controlPolicy)
    .AddVariant("test", testModel, testPolicy)
    .SplitTraffic(50, 50);
var results = await experiment.RunAsync(behaviorSpaces);
```

---

## Multi-tenancy

`Intentum.MultiTenancy` paketi tenant-scoped behavior space repository sağlar. `ITenantProvider` ve `AddTenantAwareBehaviorSpaceRepository()` kaydedin; tenant kapsamı gerektiğinde `TenantAwareBehaviorSpaceRepository` enjekte edin.

---

## Policy Versioning

`Intentum.Versioning` paketi policy versiyonlarını rollback için takip eder. `VersionedPolicy(version, policy)`, `PolicyVersionTracker.Add()`, `Rollback()` / `Rollforward()` kullanın.

---

## Structured Logging

Serilog ile structured logging.

```csharp
// Intent inference logging
intent.LogIntentInference(logger, behaviorSpace, duration);

// Policy decision logging
decision.LogPolicyDecision(logger, intent, policy);

// Behavior space logging
space.LogBehaviorSpace(logger, LogLevel.Information);
```

---

## Health Checks

ASP.NET Core health checks.

```csharp
// Program.cs
builder.Services.AddIntentum();
builder.Services.AddIntentumHealthChecks();

// Health check endpoint: /health
app.MapHealthChecks("/health");
```

---

## Ayrıca bakınız

- [API Referansı](api.md) — Tam API dokümantasyonu
- [Kurulum](setup.md) — Başlangıç rehberi
- [Senaryolar](scenarios.md) — Kullanım örnekleri
