# Mimari (TR)

**Bu sayfayı neden okuyorsunuz?** Bu sayfa Intentum mimarisini anlatır: hangi paketleri neden kullanacağınızı ve Observe → Infer → Decide akışının teknik karşılığını gösterir. İlk projeyi kurmadan önce veya paket seçerken faydalıdır.

---

## Temel akış: Observe → Infer → Decide

Intentum, senaryo tabanlı BDD yerine üç adımlı akış kullanır: davranışı kaydet, intent çıkar, policy uygula.

```mermaid
flowchart LR
  subgraph observe [Observe]
    Events[Behavior Events]
    Space[BehaviorSpace]
    Events --> Space
  end
  subgraph infer [Infer]
    Model[IIntentModel]
    Intent[Intent]
    Space --> Model
    Model --> Intent
  end
  subgraph decide [Decide]
    Policy[IntentPolicy]
    Decision[PolicyDecision]
    Intent --> Policy
    Policy --> Decision
  end
  observe --> infer --> decide
```

| Adım | Sorumluluk |
|------|-------------|
| **Observe** | Ne olduğunu kaydet: `space.Observe(actor, action)` veya `BehaviorSpaceBuilder`. Olaylar **BehaviorSpace** oluşturur. İsteğe bağlı normalizasyon için **ToVector(options)** (Cap, L1, SoftCap) kullan. |
| **Infer** | **IIntentModel** (örn. **LlmIntentModel**, **RuleBasedIntentModel** veya **ChainedIntentModel**) **Intent** (ad, güven, sinyaller, opsiyonel **Reasoning**) üretir. LlmIntentModel embedding + similarity engine kullanır; dimension count ağırlık olarak geçer; **ITimeAwareSimilarityEngine** (örn. TimeDecay) kullanıldığında otomatik uygulanır. |
| **Decide** | **IntentPolicy** kuralları sırayla değerlendirir → **PolicyDecision** (Allow, Observe, Warn, Block, Escalate, RequireAuth, RateLimit). |

**Somut örnek (e-ticaret dolandırıcılık):**

- **Observe:** Ahmet kullanıcısı 1 dakika içinde: 1) 5 farklı kredi kartı ekledi, 2) 10 farklı adrese sipariş verdi, 3) Hesap şifresini değiştirdi. Sistem bu olayları **BehaviorSpace** ile kaydeder.
- **Infer:** **IIntentModel** bu davranıştan niyet çıkarır: Niyet = HesapEleGeçirme, güven %92. Sistem bu davranışı "hesap ele geçirme" niyeti olarak yorumlar.
- **Decide:** **IntentPolicy** kuralı: "%85 üstü güven → Block + bildirim". Sonuç: Sipariş bloke edilir, güvenlik ekibine uyarı gider.

---

## Paket mimarisi

Paketler sorumluluğa göre gruplanır: çekirdek tipler, runtime (policy + rate limiting), AI (model + embedding + cache), persistence, analytics ve opsiyonel uzantılar.

```mermaid
flowchart TB
  subgraph core [Core]
    CorePkg[Intentum.Core]
    CorePkg --> Behavior[BehaviorSpace, BehaviorEvent, BehaviorSpaceBuilder, ToVectorOptions]
    CorePkg --> Intent["Intent, IntentConfidence, IntentSignal, Reasoning (Intentum.Core.Intents)"]
    CorePkg --> Batch[IBatchIntentModel, BatchIntentModel]
    CorePkg --> Model[IIntentModel]
    CorePkg --> RuleBased[RuleBasedIntentModel, ChainedIntentModel]
  end

  subgraph runtime [Runtime]
    RuntimePkg[Intentum.Runtime]
    RuntimePkg --> Policy[IntentPolicy, IntentPolicyBuilder, PolicyDecision]
    RuntimePkg --> RateLimit[IRateLimiter, MemoryRateLimiter]
    RuntimePkg --> Localization[IIntentumLocalizer]
  end

  subgraph ai [AI]
    AIPkg[Intentum.AI]
    AIPkg --> Embeddings[IIntentEmbeddingProvider, LlmIntentModel]
    AIPkg --> Similarity[IIntentSimilarityEngine, ITimeAwareSimilarityEngine, SimpleAverage, TimeDecay, Cosine, Composite]
    AIPkg --> Cache[IEmbeddingCache, MemoryEmbeddingCache, CachedEmbeddingProvider]
  end

  subgraph providers [AI Sağlayıcılar]
    OpenAI[Intentum.AI.OpenAI]
    Gemini[Intentum.AI.Gemini]
    Mistral[Intentum.AI.Mistral]
    Azure[Intentum.AI.AzureOpenAI]
    Claude[Intentum.AI.Claude]
  end

  subgraph persistence [Persistence]
    Persist[Intentum.Persistence]
    PersistEF[Intentum.Persistence.EntityFramework]
    PersistMongo[Intentum.Persistence.MongoDB]
    PersistRedis[Intentum.Persistence.Redis]
    Persist --> Repos[IBehaviorSpaceRepository, IIntentHistoryRepository]
    PersistEF --> Repos
    PersistMongo --> Repos
    PersistRedis --> Repos
  end

  subgraph extensions [Uzantılar]
    AspNet[Intentum.AspNetCore]
    Analytics[Intentum.Analytics]
    Clustering[Intentum.Clustering]
    Events[Intentum.Events]
    Experiments[Intentum.Experiments]
    Explainability[Intentum.Explainability]
    Simulation[Intentum.Simulation]
    MultiTenancy[Intentum.MultiTenancy]
    Versioning[Intentum.Versioning]
    Redis[Intentum.AI.Caching.Redis]
  end

  core --> runtime
  core --> ai
  ai --> providers
  core --> persistence
  persistence --> Analytics
  core --> AspNet
  core --> Clustering
  core --> Events
  core --> Experiments
  core --> Explainability
  core --> Simulation
  core --> MultiTenancy
  core --> Versioning
  ai --> Redis
```

**Tüm paketler (özet):** Core, Runtime, AI, AI sağlayıcılar (OpenAI, Gemini, Mistral, Azure, Claude), Persistence (abstractions + EF, MongoDB, Redis), Analytics, AspNetCore, Clustering, Events, Experiments, Explainability, Simulation, MultiTenancy, Versioning, AI.Caching.Redis. Ayrıca Testing, Observability, Logging, CodeGen — bkz. [API Referansı](api.md) ve [Gelişmiş Özellikler](advanced-features.md).

**Hangi ihtiyacıma hangi paket?**

| İhtiyacınız / Yapmak istediğiniz | Başlamak için paketler | İleride ekleyebilecekleriniz |
|----------------------------------|-------------------------|------------------------------|
| Sadece niyet tespiti (kural veya basit AI) | Intentum.Core | — |
| Karar kuralları (policy) ve sınırlama (rate limit) | Intentum.Core + Intentum.Runtime | — |
| LLM ile niyet tahmini (OpenAI, Gemini vb.) | Intentum.Core + Intentum.AI + Intentum.AI.OpenAI (veya diğer sağlayıcı) | Intentum.AI.Caching.Redis (performans) |
| Verileri veritabanına kaydetmek | Intentum.Core + Intentum.Persistence.EntityFramework (veya MongoDB) | Intentum.Analytics |
| Web uygulamasında kullanmak (ASP.NET Core) | Yukarıdakiler + Intentum.AspNetCore | Intentum.MultiTenancy (çoklu müşteri) |

Detaylı kurulum için [Kurulum](setup.md).

---

## Inference pipeline (detay)

**BehaviorSpace**'ten **PolicyDecision**'a veri akışı: embedding, similarity ve policy değerlendirmesi.

```mermaid
sequenceDiagram
  participant App as Uygulama
  participant Space as BehaviorSpace
  participant Model as IIntentModel
  participant Embed as IIntentEmbeddingProvider
  participant Cache as IEmbeddingCache
  participant Sim as IIntentSimilarityEngine
  participant Policy as IntentPolicy
  participant Rate as IRateLimiter

  App->>Space: Observe(actor, action)
  App->>Model: Infer(space)
  Model->>Space: Events / ToVector (opsiyonel ToVectorOptions)
  Model->>Embed: Embed(behaviorKey)
  Embed->>Cache: Get(key)
  alt cache miss
    Embed->>Embed: Alt sağlayıcıyı çağır
    Embed->>Cache: Set(key, embedding)
  end
  Embed-->>Model: IntentEmbedding
  Model->>Sim: CalculateIntentScore(embeddings, opsiyonel sourceWeights) veya ITimeAwareSimilarityEngine ise CalculateIntentScoreWithTimeDecay(space, embeddings)
  Sim-->>Model: score
  Model-->>App: Intent (ad, güven, sinyaller, opsiyonel Reasoning)
  App->>Policy: intent.Decide(policy)
  Policy-->>App: PolicyDecision
  opt decision RateLimit ise
    App->>Rate: TryAcquireAsync(key, limit, window)
    Rate-->>App: RateLimitResult
  end
```

---

## Katman görünümü

Katmanların birbirine bağımlılığının sadeleştirilmiş görünümü.

```mermaid
flowchart TB
  subgraph app [Uygulama / Örnekler]
    WebApp[Sample.Blazor]
    ConsoleApp[Sample.Console]
  end

  subgraph host [Hosting / HTTP]
    AspNetCore[Middleware, HealthChecks]
    Events[Webhook, IIntentEventHandler]
  end

  subgraph domain [Domain]
    Behavior[Behavior Space]
    Intent[Intent]
    Policy[Policy]
  end

  subgraph ai_layer [AI Katmanı]
    Embedding[Embedding Provider]
    Similarity[Similarity Engine]
    Cache[Embedding Cache]
    Model[Intent Model]
  end

  subgraph data [Veri]
    Repo[Repositories]
    Analytics[Analytics]
    Clustering[Clustering]
    Events[Events/Webhook]
    Experiments[Experiments]
  end

  app --> host
  app --> domain
  app --> ai_layer
  app --> data
  host --> domain
  ai_layer --> domain
  data --> domain
```

---

## Opsiyonel akışlar

### Persistence ve analytics

Intent geçmişi **IIntentHistoryRepository** ile saklanır; **IIntentAnalytics** trendler, karar dağılımı, anomali tespiti ve export için kullanır.

```mermaid
flowchart LR
  Infer[Infer + Decide]
  Infer --> Save[SaveAsync]
  Save --> History[(Intent History)]
  History --> Analytics[IIntentAnalytics]
  Analytics --> Trends[Trendler]
  Analytics --> Distribution[Karar Dağılımı]
  Analytics --> Anomaly[Anomali Tespiti]
  Analytics --> Export[JSON / CSV Export]
```

### Rate limiting

Policy **RateLimit** döndüğünde uygulama **IRateLimiter** (örn. **MemoryRateLimiter**) ile kontrol eder ve 429 + Retry-After dönebilir.

```mermaid
flowchart LR
  Intent[Intent]
  Policy[IntentPolicy]
  Intent --> Policy
  Policy --> Decision{Decision}
  Decision -->|RateLimit| Limiter[IRateLimiter.TryAcquireAsync]
  Decision -->|Diğer| Done[Karar dön]
  Limiter --> Allowed{Allowed?}
  Allowed -->|Yes| Done
  Allowed -->|No| RetryAfter[429 Retry-After]
```

### Multi-tenancy

**TenantAwareBehaviorSpaceRepository**, **IBehaviorSpaceRepository** ve **ITenantProvider** ile sarar: save'de metadata'ya `TenantId` ekler, read'de tenant'a göre filtreler.

```mermaid
flowchart TB
  App[Uygulama]
  App --> TenantRepo[TenantAwareBehaviorSpaceRepository]
  TenantRepo --> TenantProvider[ITenantProvider]
  TenantRepo --> InnerRepo[IBehaviorSpaceRepository]
  InnerRepo --> Db[(Veritabanı)]
  TenantProvider --> TenantId[Güncel TenantId]
  TenantRepo --> Metadata[Save'de TenantId metadata]
  TenantRepo --> Filter[Get'te TenantId filtre]
```

---

## Sonraki adım

Bu sayfayı bitirdiyseniz → [Kurulum](setup.md) veya [Senaryolar](scenarios.md).

---

## Ayrıca bakınız

- [Kurulum](setup.md) — Repo yapısı ve örnekler
- [API Referansı](api.md) — Ana tipler ve sözleşmeler
- [Gelişmiş Özellikler](advanced-features.md) — Similarity engine'ler (time decay, source weights), vektör normalizasyonu, kural tabanlı ve zincirli modeller, caching, clustering, events, analytics
