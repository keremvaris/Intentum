# Gelişmiş Özellikler (TR)

Bu sayfa son versiyonlarda eklenen gelişmiş özellikleri kapsar: similarity engine'ler, fluent API'ler, caching, test utilities ve daha fazlası.

---

## Paketler ve özellikler özeti

Core, Runtime, AI ve sağlayıcıların ötesindeki genişletme paketleri ve bu sayfada nerede anlatıldıkları:

| Paket | Nedir | Ne işe yarar | Bölüm |
|-------|-------|--------------|--------|
| **Intentum.AI.Caching.Redis** | Redis tabanlı embedding cache | Çok node'lu production için `IEmbeddingCache`; embedding'leri Redis'te saklar | [Embedding Caching](#embedding-caching) (Redis alt bölümü) |
| **Intentum.Clustering** | Intent gruplama | Intent history'yi pattern tespiti için gruplar; `IIntentClusterer`, `AddIntentClustering` | [Intent Clustering](#intent-clustering) |
| **Intentum.Events** | Webhook / event sistemi | Intent event'lerini (IntentInferred, PolicyDecisionChanged) HTTP POST ile gönderir; `IIntentEventHandler`, `WebhookIntentEventHandler` | [Webhook / Event Sistemi](#webhook--event-sistemi) |
| **Intentum.Experiments** | A/B test | Model/policy varyantları arasında traffic split; `IntentExperiment`, `ExperimentResult`, `AddVariant` | [A/B Experiments](#ab-experiments) |
| **Intentum.MultiTenancy** | Multi-tenancy | Tenant-scoped behavior space repository; `ITenantProvider`, `TenantAwareBehaviorSpaceRepository` | [Multi-tenancy](#multi-tenancy) |
| **Intentum.Explainability** | Niyet açıklanabilirliği | Sinyal katkı skorları, özet metin; `IIntentExplainer`, `IntentExplainer` | [Intent Explainability](#intent-explainability) |
| **Intentum.Simulation** | Niyet simülasyonu | Test için sentetik behavior space üretir; `IBehaviorSpaceSimulator`, `BehaviorSpaceSimulator` | [Intent Simulation](#intent-simulation) |
| **Intentum.Versioning** | Policy versiyonlama | Rollback için policy/model versiyon takibi; `IVersionedPolicy`, `PolicyVersionTracker` | [Policy Versioning](#policy-versioning) |
| **Intentum.Runtime.PolicyStore** | Deklaratif policy store | JSON/dosyadan policy yükleme, hot-reload; `IPolicyStore`, `FilePolicyStore`, `SafeConditionBuilder` | [Policy Store](#policy-store) |
| **Intentum.Explainability** (genişletme) | Intent karar ağacı | Policy yolunu ağaç olarak açıklar; `IIntentTreeExplainer`, `IntentTreeExplainer` | [Intent Tree](#intent-tree) |
| **Intentum.Analytics** (genişletme) | Intent timeline, pattern detector | Entity timeline, davranış pattern’leri, anomaliler; `GetIntentTimelineAsync`, `IBehaviorPatternDetector` | [Intent Timeline](#intent-timeline), [Behavior Pattern Detector](#behavior-pattern-detector) |
| **Intentum.Simulation** (genişletme) | Scenario runner | Tanımlı senaryoları model + policy ile çalıştırır; `IScenarioRunner`, `IntentScenarioRunner` | [Scenario Runner](#scenario-runner) |
| **Intentum.Core** (genişletme) | Multi-stage model, context-aware policy | Eşiklerle model zinciri; context’li policy; `MultiStageIntentModel`, `ContextAwarePolicyEngine` | [Multi-Stage Intent](#multi-stage-intent-model), [Context-Aware Policy](#context-aware-policy-engine) |
| **Intentum.Core.Streaming** | Gerçek zamanlı intent stream | Behavior event batch’lerini tüketir; `IBehaviorStreamConsumer`, `MemoryBehaviorStreamConsumer` | [Stream Processing](#real-time-intent-stream-processing) |
| **Intentum.Observability** (genişletme) | OpenTelemetry tracing | infer ve policy.evaluate span’leri; `IntentumActivitySource` | [Observability](observability.md#opentelemetry-tracing) |

**Şablonlar:** `dotnet new intentum-webapi`, `intentum-backgroundservice`, `intentum-function` — bkz. [Kurulum – Şablondan oluştur](setup.md#create-from-template-dotnet-new). **Sample Web** ayrıca Playground (model karşılaştırma): `POST /api/intent/playground/compare`, ve Intent Tree: `POST /api/intent/explain-tree` sunar.

Çekirdek paketler (Intentum.Core, Intentum.Runtime, Intentum.AI, sağlayıcılar, Testing, AspNetCore, Persistence, Analytics) [Mimari](architecture.md) ve [README](../../README.tr.md) içinde listelenir.

---

## Similarity Engine'ler

Intentum, embedding'leri intent skorlarına dönüştürmek için birden fazla similarity engine sağlar.

### Kaynak ağırlıkları (dimension count)

Similarity engine desteklediğinde **LlmIntentModel**, **dimension count** (actor:action başına event sayısı) ağırlık olarak geçirir. Böylece "user:login.failed" 5 kez olunca 1 kezden daha ağır sayılır. `CalculateIntentScore(embeddings, sourceWeights)` overload'ını implement eden engine'ler bu ağırlıkları kullanır; diğerleri yok sayar (örn. simple average).

### SimpleAverageSimilarityEngine (Varsayılan)

Embedding skorlarını ortalaması alan varsayılan engine; **sourceWeights** (örn. BehaviorVector.Dimensions) verildiğinde weighted average kullanır.

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

**LlmIntentModel** ile kullanıldığında zaman decay otomatik uygulanır: model `ITimeAwareSimilarityEngine` tespit eder ve `CalculateIntentScoreWithTimeDecay(behaviorSpace, embeddings)` çağırır; ekstra bağlama gerek yok.

```csharp
var engine = new TimeDecaySimilarityEngine(
    halfLife: TimeSpan.FromHours(1),
    referenceTime: DateTimeOffset.UtcNow);

var intentModel = new LlmIntentModel(embeddingProvider, engine);
var intent = intentModel.Infer(space); // zaman decay otomatik uygulanır
```

Doğrudan kullanım (örn. özel pipeline) için:

```csharp
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

## Behavior vektör normalizasyonu

Tekrarlayan event'lerin baskın olmaması için behavior vektörlerini normalize edebilirsin (örn. dimension başına cap, L1 norm, soft cap).

**ToVectorOptions** — `Normalization` (None, Cap, L1, SoftCap) ve opsiyonel `CapPerDimension`.

```csharp
// Ham (varsayılan): actor:action → count
var raw = space.ToVector();

// Her dimension 3'te cap
var capped = space.ToVector(new ToVectorOptions(VectorNormalization.Cap, CapPerDimension: 3));

// L1 norm: dimension değerleri toplamı 1 olsun
var l1 = space.ToVector(new ToVectorOptions(VectorNormalization.L1));

// SoftCap: value / cap, min 1
var soft = space.ToVector(new ToVectorOptions(VectorNormalization.SoftCap, CapPerDimension: 3));
```

Zaman pencereli vektör + normalizasyon:

```csharp
var windowed = space.ToVector(start, end, new ToVectorOptions(VectorNormalization.L1));
```

Çalışan örnek: [examples/vector-normalization](https://github.com/keremvaris/Intentum/tree/master/examples/vector-normalization).

**LlmIntentModel ile ToVectorOptions:** Extension `model.Infer(space, toVectorOptions)` (örn. `new ToVectorOptions(CapPerDimension: 20)`) ile vektörü cap’li hesaplayıp tek çağrıda infer edebilirsin — daha az boyut = daha az embedding çağrısı ve bellek. Bkz. [Benchmark'lar — İyileştirme fırsatları](benchmarks.md#iyileştirme-fırsatları-ve-önerilen-çözümler).

---

## Kural tabanlı ve zincirli intent modelleri

**RuleBasedIntentModel** — Sadece kurallardan intent çıkarır (LLM yok). Hızlı, deterministik, açıklanabilir. İlk eşleşen kural kazanır; her kural **RuleMatch** (name, score, opsiyonel reasoning) döndürür.

**ChainedIntentModel** — Önce birincil modeli (örn. RuleBasedIntentModel) dener; güven eşiğin altındaysa ikincil modele (örn. LlmIntentModel) düşer. Yüksek güvende ucuz path kullanarak maliyet ve gecikmeyi azaltır.

```csharp
var rules = new List<Func<BehaviorSpace, RuleMatch?>>
{
    space =>
    {
        var loginFails = space.Events.Count(e => e.Action == "login.failed");
        var hasReset = space.Events.Any(e => e.Action == "password.reset");
        if (loginFails >= 2 && hasReset)
            return new RuleMatch("AccountRecovery", 0.85, "login.failed>=2 and password.reset");
        return null;
    }
};

var primary = new RuleBasedIntentModel(rules);
var fallback = new LlmIntentModel(embeddingProvider, new SimpleAverageSimilarityEngine());
var chained = new ChainedIntentModel(primary, fallback, confidenceThreshold: 0.7);

var intent = chained.Infer(space);
// intent.Reasoning: "Primary: login.failed>=2 and password.reset" veya "Fallback: LLM (primary confidence below 0.7)"
```

Çalışan örnek: [examples/chained-intent](https://github.com/keremvaris/Intentum/tree/master/examples/chained-intent).

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

## Policy composition

Kuralları tekrarlamadan policy'leri birleştirin veya genişletin.

### Inheritance (WithBase)

Türetilmiş policy'yi base policy'den sonra değerlendirin: önce base kuralları, sonra türetilmiş. İlk eşleşen kural kazanır.

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

### Merge (birden fazla policy)

Birden fazla policy'yi tek policy'de birleştirin; ilk policy'nin kuralları önce, sonra ikincinin vb. değerlendirilir.

```csharp
var merged = IntentPolicy.Merge(policyA, policyB, policyC);
var decision = intent.Decide(merged);
```

### A/B policy varyantları (PolicyVariantSet)

Intent'a göre farklı policy kullanın (örn. deney veya güvene göre). Seçici hangi varyant adının kullanılacağını döndürür.

```csharp
var variants = new PolicyVariantSet(
    new Dictionary<string, IntentPolicy> { ["control"] = controlPolicy, ["treatment"] = treatmentPolicy },
    intent => intent.Confidence.Score > 0.8 ? "treatment" : "control");
var decision = intent.Decide(variants);
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

**Nedir:** **Intentum.AI.Caching.Redis**, Redis (ve `IDistributedCache`) kullanarak `IEmbeddingCache` uygular. Embedding sonuçları (davranış anahtarı → vektör/skor) Redis’te saklanır; böylece birden fazla uygulama örneği aynı cache’i paylaşır.

**Ne işe yarar:** Birden fazla node (örn. birden fazla web sunucusu veya worker) çalıştırdığınızda kullanın. Bellek cache’i (`MemoryEmbeddingCache`) process bazlıdır; Redis cache paylaşımlıdır, bu yüzden aynı davranış anahtarı için tekrarlayan embedding API çağrıları yapılmaz; maliyet ve gecikme azalır. Tipik kullanım: OpenAI/Gemini/Mistral ile production’da farklı instance’lardan aynı anahtarların istendiği senaryolar.

**Nasıl kullanılır:**

1. Paketi ekleyin: `Intentum.AI.Caching.Redis`.
2. Redis sunucusunun erişilebilir olduğundan emin olun (yerel veya yönetilen, örn. Azure Cache for Redis).
3. Cache’i kaydedin ve embedding sağlayıcınızı `CachedEmbeddingProvider` ile sarın:

```csharp
builder.Services.AddIntentumRedisCache(options =>
{
    options.ConnectionString = "localhost:6379";  // veya Redis connection string
    options.InstanceName = "Intentum:";            // anahtar öneki (varsayılan)
    options.DefaultExpiration = TimeSpan.FromHours(24);
});
builder.Services.AddSingleton<IIntentEmbeddingProvider>(sp =>
{
    var provider = new OpenAIEmbeddingProvider(/* ... */);  // veya herhangi IIntentEmbeddingProvider
    var cache = sp.GetRequiredService<IEmbeddingCache>();
    return new CachedEmbeddingProvider(provider, cache);
});
```

4. `IIntentEmbeddingProvider` enjekte edip `LlmIntentModel` ile normal şekilde kullanın. Cache hit’ler Redis’ten döner; miss’ler alttaki sağlayıcıyı çağırır ve sonucu cache’e yazar.

**Seçenekler:** `ConnectionString`, `InstanceName` (anahtar öneki), `DefaultExpiration` (cache TTL).

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

### Streaming: InferMany / InferManyAsync

Birçok behavior space üzerinde lazy veya async stream için `Intentum.AI` extension metodlarını kullanın (bellekte toplu liste tutmadan):

```csharp
using Intentum.AI;

// Lazy senkron stream: enumerate ederken her space için bir intent üretir
foreach (var intent in model.InferMany(spaces))
    Process(intent);

// Async stream: space'ler enumerate edilirken intent'leri üretir (örn. DB veya kuyruktan)
await foreach (var intent in model.InferManyAsync(SpacesFromDbAsync(), cancellationToken))
    await ProcessAsync(intent);
```

### Behavior vektör önbellekleme ve önceden hesaplanmış vektör

- **ToVector() önbelleği:** `BehaviorSpace.ToVector()` bir kez hesaplanır ve tekrar `Observe()` çağrılana kadar önbellekte tutulur; aynı space üzerinde tekrarlayan çıkarım vektörü yeniden kullanır.
- **Infer'da önceden hesaplanmış vektör:** Zaten bir `BehaviorVector` varsa (örn. persistence veya snapshot'tan), yeniden hesaplamayı önlemek için geçirin: `model.Infer(space, precomputedVector)`.

---

## Persistence

Analytics ve auditing için behavior space'leri ve intent history'yi saklayın. Implementasyonlar: **Entity Framework Core**, **Redis**, **MongoDB**.

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

### Redis

`Intentum.Persistence.Redis` ekleyin ve Redis bağlantısı ile kaydedin:

```csharp
using Intentum.Persistence.Redis;
using StackExchange.Redis;

var redis = ConnectionMultiplexer.Connect("localhost");
builder.Services.AddIntentumPersistenceRedis(redis, keyPrefix: "intentum:");
```

### MongoDB

`Intentum.Persistence.MongoDB` ekleyin ve `IMongoDatabase` ile kaydedin:

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

## Webhook / Event Sistemi

**Nedir:** **Intentum.Events**, niyet çıkarımı veya policy kararı sonrası intent ile ilgili event’leri dış sistemlere HTTP POST ile göndermenizi sağlar. `IIntentEventHandler` ve yerleşik `WebhookIntentEventHandler` sunar; bu handler, JSON payload’ı bir veya birden fazla webhook URL’ine, yapılandırılabilir retry ile POST eder.

**Ne işe yarar:** Bir niyet çıkarıldığında veya karar verildiğinde başka bir servise bildirim göndermeniz gerektiğinde kullanın (analytics, audit, downstream iş akışları). Tipik kullanım: `intent = model.Infer(space)` ve `decision = intent.Decide(policy)` sonrası `HandleAsync(payload, IntentumEventType.IntentInferred)` çağrısı; webhook endpoint’iniz niyet adı, güven, karar ve zaman damgasını alır.

**Nasıl kullanılır:**

1. Paketi ekleyin: `Intentum.Events`.
2. Event’leri ve webhook’ları kaydedin:

```csharp
builder.Services.AddIntentumEvents(options =>
{
    options.AddWebhook("https://api.example.com/webhooks/intent", events: new[] { "IntentInferred", "PolicyDecisionChanged" });
    options.RetryCount = 3;  // HTTP hata durumunda yeniden deneme (exponential backoff)
});
```

3. Inference sonrası payload oluşturup handler’ı çağırın:

```csharp
var intent = model.Infer(space);
var decision = intent.Decide(policy);
var payload = new IntentEventPayload(behaviorSpaceId: "id", intent, decision, DateTimeOffset.UtcNow);
await eventHandler.HandleAsync(payload, IntentumEventType.IntentInferred, cancellationToken);
```

Webhook, POST ile `BehaviorSpaceId`, `IntentName`, `ConfidenceLevel`, `ConfidenceScore`, `Decision`, `RecordedAt`, `EventType` içeren JSON gövdesini alır. Başarısız POST’lar `RetryCount`’a göre yeniden denenir.

---

## Intent Clustering

**Nedir:** **Intentum.Clustering**, intent history kayıtlarını analiz için gruplar. `IIntentClusterer` ile iki strateji sunar: **ClusterByPatternAsync** (güven seviyesi + policy kararına göre gruplama) ve **ClusterByConfidenceScoreAsync** (skora göre k adet kovana bölme). Her cluster’da id, label, kayıt id’leri, sayı ve özet (ortalama/min/max skor) bulunur.

**Ne işe yarar:** Intent history’yi (örn. `IIntentHistoryRepository` ile) sakladığınızda ve pattern görmek istediğinizde kullanın: kaç niyet Allow vs Block ile bitti, güven dağılımı (düşük/orta/yüksek bantlar) nasıl. Tipik kullanım: analytics panoları, anomali tespiti veya policy eşiklerini ayarlama.

**Nasıl kullanılır:**

1. Paketi ekleyin: `Intentum.Clustering`. `IntentHistoryRecord` verisi için **Intentum.Persistence** ve `IIntentHistoryRepository` implementasyonu gerekir.
2. Clusterer’ı kaydedin:

```csharp
builder.Services.AddIntentClustering();
```

3. `IIntentClusterer` ve `IIntentHistoryRepository` resolve edin. Kayıtları (örn. zaman penceresine göre) alıp cluster’layın:

```csharp
var clusterer = serviceProvider.GetRequiredService<IIntentClusterer>();
var historyRepo = serviceProvider.GetRequiredService<IIntentHistoryRepository>();
var records = await historyRepo.GetByTimeWindowAsync(start, end);

// (ConfidenceLevel, Decision) ile grupla — örn. "High / Allow", "Medium / Observe"
var patternClusters = await clusterer.ClusterByPatternAsync(records);
foreach (var c in patternClusters)
    Console.WriteLine($"{c.Label}: {c.Count} niyet (ortalama skor {c.Summary?.AverageConfidenceScore:F2})");

// Güven skoruna göre k kovana böl (örn. düşük / orta / yüksek)
var scoreClusters = await clusterer.ClusterByConfidenceScoreAsync(records, k: 3);
foreach (var c in scoreClusters)
    Console.WriteLine($"{c.Label}: {c.Count} niyet");
```

---

## Intent Explainability

**Nedir:** **Intentum.Explainability**, bir niyetin nasıl çıkarıldığını açıklar: hangi sinyallerin (davranışların) toplam güvene ne kadar katkı yaptığı. `IIntentExplainer` ve `IntentExplainer` sunar; sinyal katkısı (her sinyalin ağırlığının toplam içindeki yüzdesi) ve okunabilir bir özet metin üretir.

**Ne işe yarar:** Kullanıcılara veya denetçilere *neden* bu niyet/güven döndüğünü göstermeniz gerektiğinde kullanın (örn. “login %60, submit %40 katkı yaptı”). Tipik kullanım: debug UI, uyumluluk veya destek araçları.

**Nasıl kullanılır:**

1. Paketi ekleyin: `Intentum.Explainability`.
2. Explainer oluşturun (veya DI’da kaydedin); inference sonrası çağırın:

```csharp
var explainer = new IntentExplainer();

// Sinyal bazlı katkı (kaynak, açıklama, ağırlık, yüzde)
var contributions = explainer.GetSignalContributions(intent);
foreach (var c in contributions)
    Console.WriteLine($"{c.Description}: {c.ContributionPercent:F0}%");

// Tek cümle özet
var text = explainer.GetExplanation(intent, maxSignals: 5);
// örn. "Intent \"AI-Inferred-Intent\" güven High (0.85) ile çıkarıldı. En çok katkı: user:login (%45); user:submit (%35); ..."
```

Ek yapılandırma gerekmez; `Intent` ve `Signals` / `Confidence` üzerinden çalışır.

---

## Intent Simulation

**Nedir:** **Intentum.Simulation**, test ve demolar için **sentetik behavior space** üretir. `IBehaviorSpaceSimulator` ve `BehaviorSpaceSimulator` ile iki metot sunar: **FromSequence** (sabit bir actor/action listesinden space oluşturur) ve **GenerateRandom** (verilen actor ve action’lardan rastgele event’lerle space oluşturur; tekrarlanabilirlik için seed verilebilir).

**Ne işe yarar:** Testlerde her `BehaviorSpace`’i elle yazmadan çok sayıda space gerektiğinde kullanın (yük testi, property-based test, demo). Tipik kullanım: `model.Infer(space)` veya `experiment.RunAsync(spaces)` besleyen unit testler.

**Nasıl kullanılır:**

1. Paketi ekleyin: `Intentum.Simulation`.
2. Simulator oluşturup space üretin:

```csharp
var simulator = new BehaviorSpaceSimulator();

// Sabit sıra — event’lere 1 saniye arayla timestamp atanır (veya baseTime verin)
var space = simulator.FromSequence(new[] { ("user", "login"), ("user", "submit") });

// Rastgele space — yük testi veya demo; tekrarlanabilir test için randomSeed kullanın
var randomSpace = simulator.GenerateRandom(
    actors: new[] { "user", "system" },
    actions: new[] { "login", "submit", "retry", "cancel" },
    eventCount: 10,
    randomSeed: 42);
```

3. Dönen `BehaviorSpace`’i `IIntentModel`, policy veya `IntentExperiment` ile normal şekilde kullanın.

---

## A/B Experiments

**Nedir:** **Intentum.Experiments**, niyet çıkarımı üzerinde **A/B test** çalıştırır: birden fazla **varyant** (her biri model + policy çifti) tanımlarsınız, **traffic split** (örn. %50 control, %50 test) verirsiniz; bir behavior space listesini deneyden geçirirsiniz. Her space split’e göre bir varyanta atanır; her space için bir `ExperimentResult` (varyant adı, intent, decision) döner.

**Ne işe yarar:** Aynı trafikte iki (veya daha fazla) model veya policy’yi karşılaştırmak istediğinizde kullanın (yeni policy vs mevcut). Tipik kullanım: yeni policy veya modeli yayınlarken varyant bazında Allow/Block/Observe dağılımını ölçmek.

**Nasıl kullanılır:**

1. Paketi ekleyin: `Intentum.Experiments`.
2. En az iki varyant ve traffic split (yüzdeler toplamı 100 olmalı) ile deneyi kurun:

```csharp
var experiment = new IntentExperiment()
    .AddVariant("control", controlModel, controlPolicy)
    .AddVariant("test", testModel, testPolicy)
    .SplitTraffic(50, 50);  // %50 control, %50 test; verilmezse eşit bölünür
```

3. Deneyi bir behavior space listesiyle (örn. production örneklemesi veya simülasyon) çalıştırın:

```csharp
var results = await experiment.RunAsync(behaviorSpaces, cancellationToken);
foreach (var r in results)
    Console.WriteLine($"{r.VariantName}: {r.Intent.Confidence.Level} → {r.Decision}");
```

Sonuçları `VariantName`’e göre toplayıp control ile test arasında metrikleri (örn. Block oranı, ortalama güven) karşılaştırabilirsiniz.

---

## Multi-tenancy

**Nedir:** **Intentum.MultiTenancy**, **tenant-scoped behavior space repository** sağlar. `TenantAwareBehaviorSpaceRepository`, herhangi bir `IBehaviorSpaceRepository`’yi sarar: kayıtta mevcut tenant id’yi (`ITenantProvider`’dan) behavior space metadata’sına ekler; okuma/silmede yalnızca o tenant’a ait veriyi döndürür veya siler.

**Ne işe yarar:** Uygulamanız birden fazla tenant’a (örn. organizasyon veya müşteri) hizmet veriyorsa ve behavior space / intent history’yi tenant bazında ayırmak istiyorsanız kullanın. Tipik kullanım: Her istekte tenant context’i olan SaaS backend’leri (HTTP header veya claims’ten).

**Nasıl kullanılır:**

1. Paketi ekleyin: `Intentum.MultiTenancy`.
2. `ITenantProvider` implemente edin (mevcut tenant id’yi döndürsün; örn. `IHttpContextAccessor`, claims veya ambient context’ten). Kaydedin ve tenant-aware repository’yi ekleyin:

```csharp
builder.Services.AddSingleton<ITenantProvider, MyTenantProvider>();  // kendi implementasyonunuz
builder.Services.AddTenantAwareBehaviorSpaceRepository();
```

3. İç repository’yi (`IBehaviorSpaceRepository`, örn. EF veya MongoDB) normal şekilde kaydedin. Tenant izolasyonu gerektiğinde `IBehaviorSpaceRepository` yerine `TenantAwareBehaviorSpaceRepository` enjekte edin:

```csharp
// Tenant context’li bir istekte (örn. middleware tenant set eder)
var repo = serviceProvider.GetRequiredService<TenantAwareBehaviorSpaceRepository>();
await repo.SaveAsync(space, cancellationToken);   // space metadata’sına "TenantId" = mevcut tenant eklenir
var list = await repo.GetByTimeWindowAsync(start, end, cancellationToken);  // yalnızca mevcut tenant’ın space’leri
```

Tenant id metadata’da `TenantId` anahtarıyla saklanır. `GetCurrentTenantId()` null veya boş dönerse wrapper filtre uygulamaz (tüm veri görünür).

---

## Policy Versioning

**Nedir:** **Intentum.Versioning**, **policy versiyonlarını** takip eder; böylece geri alıp ileri alabilirsiniz. `IVersionedPolicy` (policy + versiyon string’i), `VersionedPolicy` (record implementasyonu) ve `PolicyVersionTracker` sunar; tracker versiyonlu policy listesini ve “mevcut” indeksi tutar. Versiyon ekleyebilir, mevcut’u değiştirebilir, `Rollback()` / `Rollforward()` ile indeksi hareket ettirebilirsiniz.

**Ne işe yarar:** Policy değişikliklerini yayınlayıp hızlıca önceki versiyona dönmek istediğinizde kullanın (yeni kural çok fazla Block üretiyorsa). Tipik kullanım: aktif policy versiyonunu değiştiren admin API veya feature flag.

**Nasıl kullanılır:**

1. Paketi ekleyin: `Intentum.Versioning`.
2. Policy’leri `VersionedPolicy` ile sarıp tracker’a ekleyin (örn. DI’da singleton):

```csharp
var tracker = new PolicyVersionTracker();
tracker.Add(new VersionedPolicy("1.0", policyV1));
tracker.Add(new VersionedPolicy("2.0", policyV2));  // mevcut artık 2.0
```

3. Karar verirken tracker’ın mevcut policy’sini kullanın:

```csharp
var versioned = tracker.Current;
var policy = versioned?.Policy ?? fallbackPolicy;
var decision = intent.Decide(policy);
```

4. Gerektiğinde geri veya ileri alın:

```csharp
if (tracker.Rollback())   // mevcut bir öncekine iner (örn. 2.0 → 1.0)
    logger.LogInformation("Geri alındı: {Version}", tracker.Current?.Version);
if (tracker.Rollforward())  // mevcut bir sonrakine çıkar (örn. 1.0 → 2.0)
    logger.LogInformation("İleri alındı: {Version}", tracker.Current?.Version);
```

İsterseniz `SetCurrent(index)` ile belirli bir indekse atlayabilirsiniz. Versiyon string’leri serbesttir (örn. `"1.0"`, `"2024-01-15"`); sıralama için `CompareVersions(a, b)` kullanılabilir.

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

## Intent Timeline

**Nedir:** **Intentum.Analytics**, intent geçmişine **entity-scoped timeline** ekler: `IIntentHistoryRepository.GetByEntityIdAsync(entityId, start, end)` ve `IIntentAnalytics.GetIntentTimelineAsync(entityId, start, end)` belirli bir entity için zaman sıralı noktalar (intent adı, güven, karar) döner.

**Ne işe yarar:** Intent history kayıtlarına isteğe bağlı `EntityId` eklendiğinde "bu kullanıcı/oturumun intent’i zamanla nasıl evrildi?" sorusuna cevap vermek — dashboard, destek araçları veya denetim için.

**Kullanım:** Intent history’yi `EntityId` ile persist edin. `AddIntentAnalytics()` ile `IIntentAnalytics` kaydedin. `GetIntentTimelineAsync(entityId, start, end)` çağırın. Sample Web: `GET /api/intent/analytics/timeline/{entityId}`.

---

## Intent Tree

**Nedir:** **Intentum.Explainability** **IIntentTreeExplainer** sunar: çıkarılmış intent ve policy verildiğinde **karar ağacı** (hangi kural eşleşti, sinyal düğümleri, intent özeti) oluşturur. `GetIntentTree(intent, policy, behaviorSpace?)` ile `IntentDecisionTree` alırsınız.

**Ne işe yarar:** Policy’nin neden Allow/Block döndüğünü açıklamak: kural adı, koşul, sinyaller ağaç formunda (UI veya denetim).

**Kullanım:** **Intentum.Explainability** ekleyin, `AddIntentTreeExplainer()` ile kaydedin. Inference ve policy değerlendirmesinden sonra `treeExplainer.GetIntentTree(intent, policy, space)` çağırın. Sample Web: `POST /api/intent/explain-tree` (infer ile aynı body).

---

## Context-Aware Policy Engine

**Nedir:** **ContextAwarePolicyEngine** ve **ContextAwareIntentPolicy**, **PolicyContext** (intent, system load, region, recent intents, custom key-value) ile kuralları değerlendirir. Kurallar `Func<Intent, PolicyContext, bool>`.

**Ne işe yarar:** Sadece mevcut intent’ten fazlasına bağlı kararlar (örn. "load > 0.8 ise block" veya "aynı intent 3 kez tekrarlanırsa escalate").

**Kullanım:** Context-aware kurallarla `ContextAwareIntentPolicy` oluşturun. `ContextAwarePolicyEngine` oluşturup `Evaluate(intent, context, policy)` çağırın. Extension: `intent.Decide(context, policy)` (RuntimeExtensions).

---

## Policy Store

**Nedir:** **Intentum.Runtime.PolicyStore**, **IPolicyStore** (örn. **FilePolicyStore**) ile JSON’dan **deklaratif policy** yükler: kurallar property/operator/value ile (örn. `intent.confidence.level` eq `"High"`). **SafeConditionBuilder** bunları `Func<Intent, bool>` yapar. Dosyadan hot-reload destekler.

**Ne işe yarar:** Geliştirici olmayanlar policy kurallarını JSON’da düzenleyebilir; kural değişikliği için kod deploy gerekmez.

**Kullanım:** **Intentum.Runtime.PolicyStore** ekleyin, `AddFilePolicyStore(path)` ile kaydedin. `await policyStore.LoadAsync()` ile policy yükleyin. JSON şeması için repoya bakın (PolicyDocument, PolicyRuleDocument, PolicyConditionDocument).

---

## Behavior Pattern Detector

**Nedir:** **Intentum.Analytics** **IBehaviorPatternDetector** sunar: intent geçmişinde **davranış pattern’leri**, **pattern anomalileri** ve **şablon eşlemesi** (`GetBehaviorPatternsAsync`, `GetPatternAnomaliesAsync`, `MatchTemplates`).

**Ne işe yarar:** Tekrarlayan intent kümelerini keşfetmek ve anomalileri işaretlemek (örn. Block oranında ani artış veya alışılmadık güven dağılımı).

**Kullanım:** `AddBehaviorPatternDetector()` ile kaydedin. `IBehaviorPatternDetector` inject edip zaman penceresi ve isteğe bağlı şablon intent’lerle metodları çağırın.

---

## Multi-Stage Intent Model

**Nedir:** **MultiStageIntentModel** (**Intentum.Core**) birden fazla **IIntentModel** örneğini güven eşikleriyle zincirler. Eşiğin üzerinde güven dönen ilk model kazanır; aksi halde son modelin sonucu döner.

**Ne işe yarar:** Örn. rule-based → hafif LLM → ağır LLM; böylece pahalı inference’a sadece güven düşükken ödersiniz.

**Kullanım:** Aşamalar (model + eşik) oluşturun. `new MultiStageIntentModel(stages)`. `Infer(space)` ile her zamanki gibi çağırın.

---

## Scenario Runner

**Nedir:** **Intentum.Simulation** **IScenarioRunner** (**IntentScenarioRunner**) sunar: **BehaviorScenario** listesini (sabit sıralar veya rastgele üretim parametreleri) bir intent modeli ve policy üzerinden çalıştırır. Senaryo başına **ScenarioRunResult** döner.

**Ne işe yarar:** Tekrarlanabilir senaryo testi, demolar veya regresyon setleri (örn. "10 senaryo çalıştır, beklenmeyen Block olmasın").

**Kullanım:** **Intentum.Simulation** ekleyin, `AddIntentScenarioRunner()` ile kaydedin. Senaryoları (sıra veya rastgele config) oluşturup `runner.RunAsync(scenarios, model, policy)` çağırın.

---

## Real-time Intent Stream Processing

**Nedir:** **Intentum.Core.Streaming** **IBehaviorStreamConsumer** tanımlar; `ReadAllAsync()` `IAsyncEnumerable<BehaviorEventBatch>` döner. **MemoryBehaviorStreamConsumer** test veya tek işlem pipeline’ları için **System.Threading.Channels** kullanan in-memory implementasyon.

**Ne işe yarar:** Behavior event’lerini stream olarak işlemek (örn. mesaj kuyruğu veya event hub’dan); tüm event’leri belleğe almadan batch başına intent çıkarmak.

**Kullanım:** **MemoryBehaviorStreamConsumer** kullanın veya implement edin. Worker veya Azure Function’da `await foreach (var batch in consumer.ReadAllAsync(cancellationToken))` ile her batch için model/policy çalıştırın.

---

## Ayrıca bakınız

- [API Referansı](api.md) — Tam API dokümantasyonu
- [Kurulum](setup.md) — Başlangıç rehberi
- [Senaryolar](scenarios.md) — Kullanım örnekleri
