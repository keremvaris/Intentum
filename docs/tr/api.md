# API Referansı (TR)

Bu sayfa ana tipleri ve birbirleriyle nasıl uyumlu olduklarını açıklar. Tam metod imzaları ve otomatik üretilen doküman için [API sitesine](https://keremvaris.github.io/Intentum/api/) bakın.

---

## Intentum nasıl çalışır (tipik akış)

1. **Gözlemle** — Kullanıcı veya sistem olaylarını (örn. login, retry, submit) bir **BehaviorSpace** içine kaydedersin.
2. **Çıkar** — Bir **LlmIntentModel** (embedding sağlayıcı ve similarity engine ile) bu davranışı bir **Intent** ve güven seviyesine (High / Medium / Low / Certain) dönüştürür.
3. **Karar ver** — **Intent** ve bir **IntentPolicy** (kurallar) ile **Decide** çağırırsın; **Allow**, **Observe**, **Warn**, **Block**, **Escalate**, **RequireAuth** veya **RateLimit** alırsın.

Yani: *davranış → intent → policy kararı*. Sabit senaryo adımları yok; model gözlenen olaylardan intent’i çıkarır.

---

## Core (`Intentum.Core`)

| Tip | Ne işe yarar |
|-----|----------------|
| **BehaviorSpace** | Gözlenen olayların konteyneri. `.Observe(actor, action)` çağırırsın (örn. `"user"`, `"login"`). Çıkarım için `.ToVector()` veya `.ToVector(ToVectorOptions?)` ile behavior vektörü alırsın; sonuç bir sonraki `Observe` çağrısına kadar önbellekte tutulur. Normalizasyon için **ToVectorOptions** kullan. |
| **ToVectorOptions** | Behavior vektörü seçenekleri: `Normalization` (None, Cap, L1, SoftCap) ve opsiyonel `CapPerDimension`. `BehaviorSpace.ToVector(options)` veya `ToVector(start, end, options)` ile kullan. |
| **Intent** | Çıkarım sonucu: güven seviyesi, skor, sinyaller (ağırlıklı davranışlar) ve opsiyonel **Reasoning** (hangi kural eşleşti veya fallback kullanıldı). |
| **IntentConfidence** | Intent’in parçası: `Level` (string) ve `Score` (0–1). |
| **IntentSignal** | Intent'teki bir sinyal: `Source`, `Description`, `Weight`. |
| **IntentEvaluator** | Intent’i kriterlere göre değerlendirir; model tarafından dahili kullanılır. |
| **RuleBasedIntentModel** | Sadece kurallarla intent çıkarır (LLM yok). İlk eşleşen kural kazanır; **RuleMatch** (name, score, reasoning) döndürür. Hızlı, deterministik, açıklanabilir. |
| **ChainedIntentModel** | Önce birincil modeli dener; güven eşiğin altındaysa ikincil modele (örn. LlmIntentModel) düşer. RuleBasedIntentModel + LlmIntentModel ile kural-öncelikli + LLM fallback. |
| **RuleMatch** | Kural sonucu: `Name`, `Score`, opsiyonel `Reasoning`. **RuleBasedIntentModel** kuralları tarafından döndürülür. |

**Namespace:** `Intent`, `IntentConfidence` ve `IntentSignal` **`Intentum.Core.Intents`** ad alanındadır. Kullanmak için `using Intentum.Core.Intents;` ekleyin.

**Nereden başlanır:** Bir `BehaviorSpace` oluştur, her olay için `.Observe(...)` çağır, sonra space’i intent modelinin `Infer(space)` metoduna ver.

---

## Runtime (`Intentum.Runtime`)

| Tip | Ne işe yarar |
|-----|----------------|
| **IntentPolicy** | Sıralı kural listesi. `.AddRule(PolicyRule(...))` ile kural eklenir. İlk eşleşen kural kazanır. Inheritance için `.WithBase(basePolicy)`; birleştirmek için `IntentPolicy.Merge(policy1, policy2, ...)` kullanın. |
| **PolicyVariantSet** | A/B policy varyantları: seçici (`Func<Intent, string>`) ile birden fazla isimli policy. Seçilen policy ile değerlendirmek için `intent.Decide(variantSet)` kullanın. Varyant isimleri için `GetVariantNames()` çağrılır. |
| **IntentPolicyBuilder** | IntentPolicy oluşturmak için fluent builder. `.Allow(...)`, `.Block(...)`, `.Escalate(...)` vb. metodlar kullanılır. |
| **PolicyRule** | İsim + koşul (örn. `Intent` üzerinde lambda) + **PolicyDecision** (Allow, Observe, Warn, Block, Escalate, RequireAuth, RateLimit). |
| **PolicyDecision** | Karar enum'u: **Allow**, **Observe**, **Warn**, **Block**, **Escalate**, **RequireAuth**, **RateLimit**. |
| **IntentPolicyEngine** | Intent’i policy’ye göre değerlendirir; **PolicyDecision** döndürür. |
| **RuntimeExtensions.Decide** | Extension: `intent.Decide(policy)` — policy’yi çalıştırır ve kararı döndürür. |
| **RuntimeExtensions.DecideWithRateLimit** / **DecideWithRateLimitAsync** | Karar RateLimit olduğunda **IRateLimiter** ile kontrol eder; **RateLimitResult** döndürür. **RateLimitOptions** (Key, Limit, Window) geçirin. |
| **RateLimitOptions** | Key, Limit, Window — DecideWithRateLimit / DecideWithRateLimitAsync ile kullanın. |
| **IRateLimiter** / **MemoryRateLimiter** | PolicyDecision.RateLimit için rate limiting. **MemoryRateLimiter** = in-memory fixed window; çok node için dağıtık implementasyon kullanın. |
| **RateLimitResult** | Allowed, CurrentCount, Limit, RetryAfter. |
| **RuntimeExtensions.ToLocalizedString** | Extension: `decision.ToLocalizedString(localizer)` — insan tarafından okunabilir metin (örn. UI için). |
| **IIntentumLocalizer** / **DefaultLocalizer** | Karar etiketleri için yerelleştirme (örn. "Allow", "Block"). **DefaultLocalizer** culture alır (örn. `"tr"`). |

**Nereden başlanır:** `.AddRule(...)` ile bir `IntentPolicy` oluştur (örn. önce Block kuralları, sonra Allow). Çıkarımdan sonra `intent.Decide(policy)` çağır.

**Fluent API örneği:**
```csharp
var policy = new IntentPolicyBuilder()
    .Block("ExcessiveRetry", i => i.Signals.Count(s => s.Description.Contains("retry")) >= 3)
    .Escalate("LowConfidence", i => i.Confidence.Level == "Low")
    .RequireAuth("SensitiveAction", i => i.Signals.Any(s => s.Description.Contains("sensitive")))
    .Allow("HighConfidence", i => i.Confidence.Level is "High" or "Certain")
    .Build();
```

**Yeni karar tipleri:**
- **Escalate** — Daha yüksek seviyeye yükselt
- **RequireAuth** — Devam etmeden önce ek kimlik doğrulama gerektir
- **RateLimit** — Aksiyona hız sınırı uygula

---

## AI (`Intentum.AI`)

| Tip | Ne işe yarar |
|-----|----------------|
| **IIntentEmbeddingProvider** | Bir behavior anahtarını (örn. `"user:login"`) **IntentEmbedding** (vektör + skor) yapar. Her sağlayıcı (OpenAI, Gemini vb.) veya test için **MockEmbeddingProvider** tarafından uygulanır. |
| **IIntentSimilarityEngine** | Embedding’leri tek bir similarity skoruna birleştirir. **SimpleAverageSimilarityEngine** varsayılan seçenektir. |
| **WeightedAverageSimilarityEngine** | Kaynağa (actor:action) göre ağırlık uygular. **sourceWeights** verildiğinde (örn. vektör dimension'ları) kullanır. |
| **TimeDecaySimilarityEngine** | Zamana göre decay; daha yeni olaylar daha yüksek ağırlık. **ITimeAwareSimilarityEngine** implement eder; LlmIntentModel engine olarak verildiğinde otomatik kullanır. |
| **CosineSimilarityEngine** | Vektörler arası kosinüs benzerliği; vektör yoksa basit ortalama. |
| **CompositeSimilarityEngine** | Birden fazla similarity engine'i ağırlıklı birleştirir. |
| **LlmIntentModel** | Embedding sağlayıcı + similarity engine alır; **Infer(BehaviorSpace, BehaviorVector? precomputedVector = null)** bir **Intent** (güven ve sinyallerle) döndürür. Engine desteklediğinde dimension count ağırlık olarak kullanılır; engine **ITimeAwareSimilarityEngine** ise zaman decay uygulanır. |
| **IntentModelStreamingExtensions** | **InferMany(model, spaces)** — lazy `IEnumerable<Intent>`; **InferManyAsync(model, spaces, ct)** — async stream. |
| **IEmbeddingCache** / **MemoryEmbeddingCache** | Embedding sonuçları cache arayüzü ve bellek implementasyonu. **CachedEmbeddingProvider** ile her sağlayıcı cache'lenebilir. |
| **IBatchIntentModel** / **BatchIntentModel** | Birden fazla behavior space için toplu çıkarım; async ve iptal destekler. |

**Nereden başlanır:** Hızlı yerel çalıştırma için **MockEmbeddingProvider** ve **SimpleAverageSimilarityEngine** kullan; production için gerçek sağlayıcıya geç ([Sağlayıcılar](providers.md)).

**AI pipeline (özet):**  
1) **Embedding** — Her davranış anahtarı (`actor:action`) sağlayıcıya gider; vektör + skor döner. Mock = hash; gerçek sağlayıcı = anlamsal embedding.  
2) **Similarity** — Tüm embedding’ler tek skorda birleştirilir (örn. ortalama).  
3) **Confidence** — Skor High/Medium/Low/Certain seviyesine çevrilir.  
4) **Signals** — Her davranışın ağırlığı Intent sinyallerinde yer alır; policy kurallarında (örn. retry sayısı) kullanılabilir.

---

## Sağlayıcılar (opsiyonel paketler)

| Tip | Ne işe yarar |
|-----|----------------|
| **OpenAIEmbeddingProvider** | OpenAI embedding API kullanır; **OpenAIOptions** (örn. `FromEnvironment()`) ile yapılandırılır. |
| **GeminiEmbeddingProvider** | Google Gemini embedding API kullanır; **GeminiOptions**. |
| **MistralEmbeddingProvider** | Mistral embedding API kullanır; **MistralOptions**. |
| **AzureOpenAIEmbeddingProvider** | Azure OpenAI embedding deployment kullanır; **AzureOpenAIOptions**. |
| **ClaudeMessageIntentModel** | Claude tabanlı intent modeli (mesaj skoru); **ClaudeOptions**. |

Sağlayıcılar **AddIntentum\*** extension metodları ve options (env var) ile kaydedilir. Kurulum ve env var için [Sağlayıcılar](providers.md).

---

## Test (`Intentum.Testing`)

| Tip | Ne işe yarar |
|-----|----------------|
| **TestHelpers** | Test nesneleri: `CreateDefaultModel()`, `CreateDefaultPolicy()`, `CreateSimpleSpace()` vb. |
| **BehaviorSpaceAssertions** | BehaviorSpace assert: `ContainsEvent()`, `HasEventCount()`, `ContainsActor()` vb. |
| **IntentAssertions** | Intent assert: `HasConfidenceLevel()`, `HasConfidenceScore()`, `HasSignals()` vb. |
| **PolicyDecisionAssertions** | PolicyDecision assert: `IsOneOf()`, `IsAllow()`, `IsBlock()` vb. |

**Nereden başlanır:** Test projesine `Intentum.Testing` ekleyip bu yardımcıları kullanın.

---

## ASP.NET Core (`Intentum.AspNetCore`)

| Tip | Ne işe yarar |
|-----|----------------|
| **BehaviorObservationMiddleware** | HTTP istek davranışlarını otomatik BehaviorSpace'e kaydeder. |
| **IntentumAspNetCoreExtensions** | `AddIntentum()` (DI), `UseIntentumBehaviorObservation()` (middleware). |

**Nereden başlanır:** `Intentum.AspNetCore` ekleyin; `Program.cs`'de `services.AddIntentum()`, sonra `app.UseIntentumBehaviorObservation()`.

---

## Observability (`Intentum.Observability`)

| Tip | Ne işe yarar |
|-----|----------------|
| **IntentumMetrics** | OpenTelemetry metrikleri: intent çıkarım sayısı/süresi, güven skorları, policy kararları. |
| **ObservableIntentModel** | IIntentModel etrafında metrik saran wrapper. |
| **ObservablePolicyEngine** | `DecideWithMetrics()` — karar + metrik. |

---

## Logging (`Intentum.Logging`)

| Tip | Ne işe yarar |
|-----|----------------|
| **IntentumLogger** | Serilog ile yapılandırılmış loglama (intent, policy, behavior). |
| **LoggingExtensions** | `LogIntentInference()`, `LogPolicyDecision()`, `LogBehaviorSpace()`. |

---

## Persistence (`Intentum.Persistence`)

| Tip | Ne işe yarar |
|-----|----------------|
| **IBehaviorSpaceRepository** | Behavior space kaydetme ve sorgulama arayüzü. |
| **IIntentHistoryRepository** | Intent çıkarım sonuçları ve policy kararlarını saklama arayüzü. |
| **Intentum.Persistence.EntityFramework** | EF Core implementasyonu; `AddIntentumPersistence()`. |
| **Intentum.Persistence.Redis** | Redis tabanlı repository'ler; `AddIntentumPersistenceRedis(IConnectionMultiplexer, keyPrefix?)`. |
| **Intentum.Persistence.MongoDB** | MongoDB tabanlı repository'ler; `AddIntentumPersistenceMongoDB(IMongoDatabase, ...)`. |

**Nereden başlanır:** EF, Redis veya MongoDB paketini ekleyip ilgili `AddIntentumPersistence*` ile kaydedin.

---

## Genişletme paketleri (API özeti)

Aşağıdaki paketler opsiyonel yetenek ekler. Detaylı kullanım (nedir, ne zaman, nasıl) için [Gelişmiş Özellikler](advanced-features.md).

### Redis embedding cache (`Intentum.AI.Caching.Redis`)

| Tip | Ne işe yarar |
|-----|----------------|
| **RedisEmbeddingCache** | Redis (`IDistributedCache`) ile `IEmbeddingCache` implementasyonu. |
| **RedisCachingExtensions.AddIntentumRedisCache** | Redis cache ve options (ConnectionString, InstanceName, DefaultExpiration) kaydı. |
| **IntentumRedisCacheOptions** | ConnectionString, InstanceName, DefaultExpiration. |

### Clustering (`Intentum.Clustering`)

| Tip | Ne işe yarar |
|-----|----------------|
| **IIntentClusterer** | Intent history kayıtlarını cluster'lar (pattern veya skor kovaları). |
| **IntentClusterer** | `ClusterByPatternAsync(records)` (ConfidenceLevel + Decision), `ClusterByConfidenceScoreAsync(records, k)`. |
| **IntentCluster** | Id, Label, RecordIds, Count, Summary (ClusterSummary). |
| **ClusterSummary** | AverageConfidenceScore, MinScore, MaxScore. |
| **ClusteringExtensions.AddIntentClustering** | DI'da `IIntentClusterer` kaydı. |

### Events / Webhook (`Intentum.Events`)

| Tip | Ne işe yarar |
|-----|----------------|
| **IIntentEventHandler** | `HandleAsync(payload, eventType)` — örn. webhook'a gönderim. |
| **WebhookIntentEventHandler** | IntentInferred, PolicyDecisionChanged ile JSON POST; retry. |
| **IntentEventPayload** | BehaviorSpaceId, Intent, Decision, RecordedAt. |
| **IntentumEventType** | IntentInferred, PolicyDecisionChanged. |
| **EventsExtensions.AddIntentumEvents** | Event handling kaydı; options üzerinde **AddWebhook(url, events?)**. |
| **IntentumEventsOptions** | Webhooks (Url + EventTypes listesi), RetryCount. |

### Experiments (`Intentum.Experiments`)

| Tip | Ne işe yarar |
|-----|----------------|
| **IntentExperiment** | A/B test: `.AddVariant(name, model, policy)`, `.SplitTraffic(yüzdeler)`, `.RunAsync(behaviorSpaces)` / `.Run(behaviorSpaces)`. |
| **ExperimentResult** | VariantName, Intent, Decision (her behavior space için). |

### Explainability (`Intentum.Explainability`)

| Tip | Ne işe yarar |
|-----|----------------|
| **IIntentExplainer** | Intent'in nasıl çıkarıldığını açıklar (sinyal katkıları, metin özeti). |
| **IntentExplainer** | `GetSignalContributions(intent)` → **SignalContribution** listesi; `GetExplanation(intent, maxSignals?)` → string. |
| **SignalContribution** | Source, Description, Weight, ContributionPercent. |

### Simulation (`Intentum.Simulation`)

| Tip | Ne işe yarar |
|-----|----------------|
| **IBehaviorSpaceSimulator** | Sentetik behavior space üretir. |
| **BehaviorSpaceSimulator** | `FromSequence((actor, action)[])` — sabit sıra; `GenerateRandom(actors, actions, eventCount, randomSeed?)` — rastgele. |

### Multi-tenancy (`Intentum.MultiTenancy`)

| Tip | Ne işe yarar |
|-----|----------------|
| **ITenantProvider** | `GetCurrentTenantId()` — örn. HTTP context veya claims'ten. |
| **TenantAwareBehaviorSpaceRepository** | `IBehaviorSpaceRepository` saran: Save'de TenantId ekler, Get/Delete'de tenant'a göre filtreler. |
| **MultiTenancyExtensions.AddTenantAwareBehaviorSpaceRepository** | Tenant-aware repo DI kaydı (iç repo + ITenantProvider gerekir). |

### Versioning (`Intentum.Versioning`)

| Tip | Ne işe yarar |
|-----|----------------|
| **IVersionedPolicy** | Version (string) + Policy (IntentPolicy). |
| **VersionedPolicy** | Record: `new VersionedPolicy(version, policy)`. |
| **PolicyVersionTracker** | `Add(versionedPolicy)`, `Current`, `Versions`, `Rollback()`, `Rollforward()`, `SetCurrent(index)`, `CompareVersions(a, b)`. |

---

## Batch Processing (`Intentum.Core.Batch`)

| Tip | Ne işe yarar |
|-----|----------------|
| **IBatchIntentModel** | Toplu intent çıkarım arayüzü. |
| **BatchIntentModel** | Birden fazla behavior space'i toplu işler; async ve iptal destekler. |

**Nereden başlanır:** `IIntentModel`'i `BatchIntentModel` ile sarıp `InferBatch()` / `InferBatchAsync()` kullanın.

---

## Analytics (`Intentum.Analytics`)

| Tip | Ne işe yarar |
|-----|----------------|
| **IIntentAnalytics** | Intent history üzerinde analytics: confidence trendleri, decision dağılımı, anomali tespiti, export. |
| **IntentAnalytics** | IIntentHistoryRepository kullanan varsayılan implementasyon. |
| **ConfidenceTrendPoint** | Confidence trendinde bir bucket (BucketStart, BucketEnd, ConfidenceLevel, Count, AverageScore). |
| **DecisionDistributionReport** | Zaman penceresinde PolicyDecision başına sayı. |
| **AnomalyReport** | Tespit edilen anomali (Type, Description, Severity, Details). |
| **AnalyticsSummary** | Dashboard için özet (trends, distribution, anomalies). |

**Persistence:** `IBehaviorSpaceRepository` ve `IIntentHistoryRepository` için EF Core (`AddIntentumPersistence`), Redis (`Intentum.Persistence.Redis`, `AddIntentumPersistenceRedis`) veya MongoDB (`Intentum.Persistence.MongoDB`, `AddIntentumPersistenceMongoDB`) kullanılabilir. Detay: [Gelişmiş özellikler](advanced-features.md#persistence).

**Nereden başlanır:** `IIntentHistoryRepository` kaydedin (örn. `AddIntentumPersistence`), sonra `AddIntentAnalytics()` ekleyin ve `IIntentAnalytics` inject edin. `GetSummaryAsync()`, `GetConfidenceTrendsAsync()`, `GetDecisionDistributionAsync()`, `DetectAnomaliesAsync()`, `ExportToJsonAsync()`, `ExportToCsvAsync()` kullanın.

---

## Örnek Web HTTP API (`Intentum.Sample.Web`)

Web örneği intent çıkarımı, açıklanabilirlik, greenwashing tespiti ve analytics için HTTP endpoint’leri sunar. `dotnet run --project samples/Intentum.Sample.Web` ile çalıştırın; UI http://localhost:5150/, API dokümanları http://localhost:5150/scalar.

| Method | Path | Açıklama |
|--------|------|----------|
| POST | `/api/intent/infer` | Olaylardan intent çıkarır. Body: `{ "events": [ { "actor": "user", "action": "login" }, ... ] }`. Intent adı, güven, karar, sinyaller döner. |
| POST | `/api/intent/explain` | Infer ile aynı body; sinyal katkıları (kaynak, açıklama, ağırlık, yüzde) ve metin açıklaması döner. |
| GET | `/api/intent/history` | Sayfalanmış intent geçmişi (örnekte in-memory). Sorgu: `skip`, `take`. |
| GET | `/api/intent/analytics/summary` | Dashboard özeti: güven trendleri, karar dağılımı, anomali listesi. |
| GET | `/api/intent/analytics/export/json` | Analytics’i JSON olarak dışa aktarır. |
| GET | `/api/intent/analytics/export/csv` | Analytics’i CSV olarak dışa aktarır. |
| POST | `/api/greenwashing/analyze` | Raporu greenwashing için analiz eder. Body: `{ "report": "...", "sourceType": "Report", "language": "tr", "imageBase64": null }`. Intent, karar, sinyaller, önerilen aksiyonlar, `sourceMetadata`, `visualResult` (görsel gönderildiyse) döner. |
| GET | `/api/greenwashing/recent?limit=15` | Son greenwashing analizleri (in-memory; Dashboard’da kullanılır). |
| POST | `/api/carbon/calculate` | Karbon ayak izi hesaplama (CQRS örneği). |
| GET | `/api/carbon/report/{reportId}` | Rapor id’ye göre karbon raporu. |
| POST | `/api/orders` | Sipariş ver (CQRS örneği). |
| GET | `/health` | Sağlık kontrolü. |

Detay için [Kurulum](setup.md#repo-sampleını-derle-ve-çalıştır) ve [Greenwashing tespiti (how-to)](greenwashing-detection-howto.md#6-örnek-uygulama-intentumsampleweb).

---

## Minimal kod özeti

```csharp
// 1) Davranış oluştur
var space = new BehaviorSpace()
    .Observe("user", "login")
    .Observe("user", "retry")
    .Observe("user", "submit");

// 2) Intent çıkar (Mock = API anahtarı gerekmez)
var model = new LlmIntentModel(
    new MockEmbeddingProvider(),
    new SimpleAverageSimilarityEngine());
var intent = model.Infer(space);

// 3) Karar ver (fluent API ile)
var policy = new IntentPolicyBuilder()
    .Block("ExcessiveRetry", i => i.Signals.Count(s => s.Description.Contains("retry")) >= 3)
    .Escalate("LowConfidence", i => i.Confidence.Level == "Low")
    .RequireAuth("SensitiveAction", i => i.Signals.Any(s => s.Description.Contains("sensitive")))
    .Allow("HighConfidence", i => i.Confidence.Level is "High" or "Certain")
    .Build();
var decision = intent.Decide(policy);
```

Tam çalışan örnek için [sample](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample) ve [Kurulum](setup.md).
