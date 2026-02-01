# Test (TR)

**Bu sayfayı neden okuyorsunuz?** Bu sayfa Intentum testlerinin neyi kapsadığını, nasıl çalıştırılacağını ve kendi testinizi nasıl ekleyeceğinizi anlatır. CI'da API anahtarı olmadan güvenilir test istiyorsanız doğru yerdesiniz.

Intentum testleri **contract** (sağlayıcıların ve çekirdek tiplerin doğru parse ve davranışı) ile **çekirdek akışlara** (BehaviorSpace → Infer → Decide) odaklanır. Testler **mock HTTP** ve bellek içi sağlayıcı kullanır; API anahtarı veya ağ olmadan çalıştırılabilir.

Bu sayfa neyin test edildiğini, testlerin nasıl çalıştırılacağını ve kendi testini nasıl ekleyeceğini anlatır. Coverage üretimi ve raporlar için [Coverage](coverage.md).

---

## Neden bu şekilde test?

- **CI’da gerçek API çağrısı yok:** Mock HTTP ve sabit cevaplar testleri hızlı ve kararlı tutar; secret gerekmez.
- **Contract testleri:** Her sağlayıcının API cevap şeklini (embedding dizisi, skor) doğru şekilde `IntentEmbedding`’e parse ettiğini ve intent modeli ile policy engine’in beklenen şekilde davrandığını assert ediyoruz.
- **Çekirdek davranış:** BehaviorSpace vektörleştirme, intent çıkarımı ve policy kararları kapsanıyor; refactor’lar ana akışı bozmaz.

---

## Testleri nasıl çalıştırılır?

Reponun kökünden:

```bash
dotnet test tests/Intentum.Tests/Intentum.Tests.csproj
```

Ayrıntılı çıktı ile:

```bash
dotnet test tests/Intentum.Tests/Intentum.Tests.csproj -v n
```

---

## Neler kapsanıyor?

| Alan | Neyi test ediyoruz |
|------|---------------------|
| **BehaviorSpace** | `Observe(actor, action)` ile space oluşturma, olay sayısı, `ToVector()` boyutları; `IntentumCoreExtensions` (Observe, EvaluateIntent, IntentEvaluator.Normalize). |
| **Intent çıkarımı** | Mock sağlayıcı ve `SimpleAverageSimilarityEngine` ile `LlmIntentModel`: güven seviyesi ve skor, sinyaller; **IntentConfidence** (FromScore: Low/Medium/High/Certain). |
| **Policy kararları** | `IntentPolicy` ve **IntentPolicyEngine**: kural sırası, ilk eşleşen kural kazanır, eşleşen kural yok, boş policy; Allow, Observe, Warn, Block sonuçları; **PolicyDecisionTypes** (ToLocalizedString); **RuntimeExtensions** (DecideWithRateLimit / DecideWithRateLimitAsync, rate limit uygulanan/uygulanmayan). |
| **Yerelleştirme** | **DefaultLocalizer**: karar etiketleri (kültür, bilinen/bilinmeyen anahtarlar). |
| **Options doğrulama** | **OpenAIOptions**, **AzureOpenAIOptions**, **GeminiOptions**, **MistralOptions** `Validate()`: geçerli durum ve geçersiz (boş API key, embedding model, base URL) throw. |
| **Sağlayıcı cevap parse** | Her embedding sağlayıcısı (OpenAI, Gemini, Mistral, Azure OpenAI) için **mock HttpClient** ile sabit JSON cevabı; parse edilen embedding skorunu (veya non-200’de exception) assert ediyoruz. |
| **Sağlayıcı IntentModel'leri** | **OpenAIIntentModel**, **GeminiIntentModel**, **MistralIntentModel**, **AzureOpenAIIntentModel** ile **MockEmbeddingProvider**: infer beklenen confidence ve sinyalleri döndürür. |
| **Clustering** | **AddIntentClustering** kaydı; **IntentClusterer** (cluster Id, RecordIds, ClusterSummary ortalama/min/max). |
| **Test yardımcıları** | **IntentAssertions** (HasSignalCount, ContainsSignal), **PolicyDecisionAssertions** (IsAllow, IsBlock, IsNotBlock), **TestHelpers.CreateModel**. |
| **Redis embedding cache** | **RedisEmbeddingCache**: bellek içi `IDistributedCache` ile Get/Set/Remove; gerçek Redis yok. |
| **Webhook / Events** | **WebhookIntentEventHandler**: AddWebhook seçenekleri, HandleAsync mock HttpClient’a POST; **AddIntentumEvents** DI kaydı. |
| **Experiments** | **IntentExperiment**: AddVariant, SplitTraffic, RunAsync (mock model/policy ile). |
| **Simulation** | **BehaviorSpaceSimulator**: FromSequence, GenerateRandom (seed ile). |
| **Explainability** | **IntentExplainer**: GetSignalContributions, GetExplanation. **IntentTreeExplainer**: GetIntentTree (karar ağacı, eşleşen kural, sinyal düğümleri). |
| **Policy observability** | **DecideWithExecutionLog**: çalıştırma kaydı (eşleşen kural, intent adı, karar, süre, başarı, exception); **DecideWithMetrics** tutarlılığı. |
| **Multi-tenancy** | **TenantAwareBehaviorSpaceRepository**: SaveAsync TenantId ekler, GetByIdAsync tenant’a göre filtreler; bellek içi repo. |
| **Versioning** | **PolicyVersionTracker**: Add, Current, Rollback, Rollforward, SetCurrent; **VersionedPolicy**. |

Yani: gerçek API çağırmıyoruz; sahte JSON veya mock sağlayıcı kullanıyoruz ve sağlayıcı ile modelin beklenen `Intent` ve `PolicyDecision` ürettiğini kontrol ediyoruz.

---

## Henüz kapsanmayanlar

| Alan | Durum |
|------|--------|
| **Intentum.Persistence.MongoDB** | Test projesinde referans yok; `MongoBehaviorSpaceRepository` / `MongoIntentHistoryRepository` için unit veya integration test yok. |
| **Intentum.Persistence.EntityFramework** | Test projesinde referans yok; EF repository’ler veya PostgreSQL/SQL Server için test yok. |
| **Intentum.Persistence.Redis** | Test projesinde referans yok; Redis tabanlı behavior space veya intent history repository testi yok. |
| **Gerçek Redis / MongoDB / PostgreSQL** | Tüm mevcut testler bellek içi veya mock kullanıyor. Gerçek veritabanına karşı integration testler (örn. Testcontainers) yok. |

Persistence ekliyorsanız veya gerçek bir store’a karşı çalıştırıyorsanız, integration test (örn. MongoDB/PostgreSQL/Redis için Testcontainers) veya en azından aynı interface’i implement eden sahte repository ile contract test eklemeyi düşünün.

---

## Hata senaryoları

- **HTTP non-200:** Sağlayıcılar HTTP cevabı başarılı değilse (örn. 401, 500) exception fırlatır. Testler 401/500 dönen mock client ile bunu simüle edip exception bekleyebilir.
- **Boş embedding’ler:** API boş embedding dizisi (veya embedding yok) döndürürse sağlayıcı skor 0 (veya eşdeğeri) döndürür; testler bunu kapsar, davranış öngörülebilir olur.

---

## Test nasıl eklenir (mock HTTP)

Gerçek API çağırmadan bir sağlayıcıyı test etmek için:

1. Sabit cevap döndüren bir `HttpClient` oluştur (örn. `new HttpClient(new MockHttpMessageHandler(json))` veya test sunucusu).
2. Sağlayıcıyı options (örn. sahte API key) ve bu client ile oluştur.
3. `provider.Embed("user:login")` (veya benzeri) çağır ve `result.Score` veya beklenen exception üzerinde assert et.

Örnek kalıp (kavramsal):

```csharp
var json = """{ "data": [ { "embedding": [0.5, -0.5] } ] }""";
var client = CreateMockClient(json);
var provider = new OpenAIEmbeddingProvider(options, client);
var result = provider.Embed("user:login");
Assert.InRange(result.Score, 0.49, 0.51);
```

Gerçek örnekler için repodaki `ProviderHttpTests` dosyasına bak. Coverage seçenekleri (CollectCoverage, OpenCover vb.) için [Coverage](coverage.md).

**Sonraki adım:** Bu sayfayı bitirdiyseniz → [Testler özeti](tests-overview.md) veya [Coverage](coverage.md).
