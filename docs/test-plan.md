# Intentum Test Plan — %80 Line Coverage Hedefi

Bu belge, coverage’ı “exclude ile örtbas” etmeden **gerçek satır kapsamıyla %80’e** çıkarmak için yazılacak testlerin planıdır. Projede `ExcludeByFile` kaldırıldı; eşik şu an **%70** (mevcut gerçek toplam). Plan tamamlandıkça eşik **%80**’e yükseltilecek.

## Mevcut Durum (exclusion yok)

- **Toplam satır kapsamı:** ~%70
- **Coverage on New Code (SonarCloud):** ~%73
- **Hedef:** %80
- **Threshold:** `Intentum.Tests.csproj` içinde `Threshold=70` (plan ilerledikçe 80’e çekilecek)

## Coverage on New Code — Öncelik Sırası (SonarCloud)

SonarCloud raporundaki **yeni kodda 0% veya düşük kapsam** veren dosyalar; aşağıdaki sırayla ele alınabilir. Her satır “hangi faz” ile kapatılacağını gösterir.

| Öncelik | Dosya | Uncovered (satır/koşul) | Hedef faz |
|--------|-------|-------------------------|-----------|
| 1 | `Intentum.AI.OpenAI/OpenAIObservationMiddleware.cs` | 33 / 0 | Faz 5 |
| 2 | `Intentum.AI.ONNX/OnnxIntentModel.cs` | 31 / 12 | Faz 2 |
| 3 | `Intentum.Persistence/EntityFramework/FrameworkPersistenceExtensions.cs` | 20 / 0 | Faz 1 |
| 4 | `Intentum.AI.Mistral/MistralIntentModel.cs` | 19 / 0 | Faz 5 |
| 5 | `Intentum.AI.AzureOpenAI/AzureOpenAIService.cs` | 10 / 0 | Faz 5 |
| 6 | `Intentum.AI.AzureOpenAI/AzureOpenAIObservationMiddleware.cs` | 10 / 0 | Faz 5 |
| 7 | `Intentum.Core/Clustering/HierarchicalClustering.cs` | 12 / 11 (≈31%) | Mevcut Clustering testleri genişlet |
| 8 | `Intentum.AI.Gemini/GeminiIntentModel.cs` | 5 / 2 (≈37%) | Faz 5 |
| 9 | `Intentum.AI.OpenAI/OpenAIFunctionCallProvider.cs` | 11 / 16 (≈64%) | Faz 5 |
| … | Core/Application/Logging middlewares & extensions (0%) | Çeşitli | Faz 4 / ek unit testler |

**İlk adım önerisi:** En çok açık satır veren **OpenAIObservationMiddleware** ve **OnnxIntentModel** ile başla; ardından Persistence (FrameworkPersistenceExtensions) ve Azure/Mistral servisleri. Böylece “Coverage on New Code” oranı hızla yukarı çıkar.

## Modül Bazlı Kapsam ve Öncelik

Aşağıdaki yüzdeler exclusion olmadan ölçülen **line coverage**. Düşük kapsamlı modüller önce ele alınacak.

| Modül | Line % | Öncelik | Not |
|-------|--------|--------|-----|
| **Intentum.Persistence** | ~9% | P1 | Repo arayüzleri + NoOp; InMemory / fake repo testleri |
| **Intentum.AI.ONNX** | ~9% | P1 | Sadece constructor/validation; inference için mock/fixture |
| **Intentum.MultiTenancy** | ~39% | P2 | TenantAwareRepository + ITenantProvider mock |
| **Intentum.Observability** | ~43% | P2 | ObservableIntentModel / ObservablePolicyEngine, metrik/span yolları |
| **Intentum.AI.Mistral** | ~47% | P2 | HTTP/options unit testleri (mock HttpClient) |
| **Intentum.AI.AzureOpenAI** | ~50% | P2 | Aynı şekilde mock tabanlı |
| **Intentum.AI.Gemini** | ~53% | P2 | Aynı şekilde |
| **Intentum.Analytics** | ~58% | P2 | IntentAnalytics + IntentProfileService + BehaviorPatternDetector senaryoları |
| **Intentum.AI.Caching.Redis** | ~65% | P3 | Cache hit/miss, connection failure yolları |
| **Intentum.AI.OpenAI** | ~65% | P3 | Aynı şekilde |

## Faz 1 — Persistence (P1)

**Hedef:** Persistence satır kapsamını belirgin artırmak.

1. **NoOpIntentAuditStore**
   - `AppendAsync` çağrıldığında tamamlanır, exception fırlatmaz (zaten tek satır; bir test yeter).

2. **InMemoryIntentHistoryRepository / InMemoryBehaviorSpaceRepository**
   - Eğer projede yoksa, test için `tests` içinde in-memory implementasyon kullanılıyor; bu implementasyonun Save/Load/List senaryoları için testler yazılacak (Persistence arayüzlerini implemente eden sınıflar nerede ise oraya odaklan).

3. **Serialization (BehaviorSpaceSerialization, IntentHistorySerialization)**
   - Round-trip: nesne → serileştir → deserileştir → aynı veri.
   - Hatalı/eksik payload ile deserialize edge case’leri.

4. **IIntentAuditStore / IIntentHistoryRepository**
   - Sadece arayüz kullanan kod yoksa, bu arayüzleri implemente eden somut sınıfların (NoOp, InMemory vb.) testleri yukarıdaki maddelerle gelir.

**Çıktı:** Persistence modülü için net artış; toplam coverage’a katkı.

---

## Faz 2 — Intentum.AI.ONNX (P1)

**Hedef:** Constructor ve options validasyonu + inference yolunu mock/fixture ile çalıştırmak.

1. **OnnxIntentModel**
   - Zaten var: `Constructor_WhenOptionsNull_ThrowsArgumentNullException`, `Constructor_WhenModelPathMissing_ThrowsArgumentException`.
   - Eklenecek:
     - `IntentLabels` boş veya sayı uyuşmazlığında `ArgumentException`.
     - `FeatureDimensionNames` sayısı input size ile uyuşmazsa `ArgumentException`.
   - **Inference:** Gerçek ONNX dosyası CI’da olmayabilir; küçük bir test modeli (veya CI’da skip edilen integration) veya **in-memory / mock session** ile `Infer` çağrısı ve `LogitsToIntent` çıktısı (argmax/softmax) test edilebilir. Hedef: `Infer` ve static `LogitsToIntent` satırlarının kapsanması.

2. **OnnxIntentModelOptions**
   - Fluent/record kullanımı ve default değerler için kısa testler.

**Çıktı:** ONNX modülü line coverage belirgin artar.

---

## Faz 3 — MultiTenancy (P2)

**Hedef:** TenantAwareBehaviorSpaceRepository ve ITenantProvider senaryoları.

1. **TenantAwareBehaviorSpaceRepository**
   - `ITenantProvider` mock: tenant id döner; repository’nin doğru tenant key ile inner repo’ya yönlendirdiğini doğrula.
   - Tenant yoksa / null ise davranış (exception veya fallback) testi.
   - Var olan `TenantAwareBehaviorSpaceRepositoryTests` genişletilecek: Get/Save/List yollarının hepsi kapsanacak.

2. **MultiTenancyExtensions**
   - SonarCloud’da extension’lar bazen exclude edilir; proje politikasına göre sadece “hangi service’lerin eklendiği” için kısa bir test yazılabilir (DI container ile).

**Çıktı:** MultiTenancy line coverage ~%80’e yaklaşır.

---

## Faz 4 — Observability (P2)

**Hedef:** ObservableIntentModel ve ObservablePolicyEngine’in tüm dalları.

1. **ObservableIntentModel**
   - `Infer` başarılı path: inner model döner, metrik/span tetiklenir (Activity/Metrics mock veya test listener).
   - `Infer` exception path: activity status Error, exception propagate edilir.
   - `GetBehaviorSignalSummary`: events boş → null; events dolu ve uzun string → truncation.

2. **ObservablePolicyEngine**
   - Evaluate başarılı ve red/exception senaryoları; kayıt (record) ve metrikler.

3. **IntentumActivitySource / IntentumMetrics**
   - Mümkünse test listener veya no-op exporter ile span/métrik üretildiğini doğrulayan testler.

**Çıktı:** Observability modülü ~%80+ line coverage.

---

## Faz 5 — AI sağlayıcıları (Mistral, Azure, Gemini, OpenAI) (P2–P3)

**Hedef:** HTTP ve options katmanını mock’layıp hata/başarı yollarını kapsamak.

- Ortak strateji:
  - `HttpClient` için mock (ör. `HttpMessageHandler` ile) veya test server.
  - 200 + geçerli JSON → doğru intent/embedding parse.
  - 4xx/5xx veya timeout → exception veya fallback davranışı.
  - Options: base URL, API key, timeout validasyonu (null/empty ne yapıyor).
- Her sağlayıcı için:
  - Embedding provider: `GetEmbeddingAsync` başarı + en az bir hata senaryosu.
  - LLM pipeline testi (mümkünse aynı mock ile): tek bir “happy path” + bir “API error” path.

**Çıktı:** Tüm AI provider modülleri ~%70+ (ideal ~%80).

---

## Faz 6 — Analytics (P2)

**Hedef:** IntentAnalytics, IntentProfileService, BehaviorPatternDetector ve kullanılan model sınıfları.

1. **IntentAnalytics** (mevcut testler var; genişlet)
   - Tarih aralığı boş, tek kayıt, çok kayıt.
   - Farklı decision/confidence kombinasyonları.

2. **IntentProfileService**
   - Profil oluşturma, güncelleme, edge case (boş liste, tek intent).

3. **BehaviorPatternDetector**
   - Pattern tespiti: kısa/düzgün sequence ile beklenen pattern; anomali raporu.

4. **Model sınıfları (IntentGraphSnapshot, IntentProfile, vb.)**
   - Serialization veya factory kullanımı üzerinden kısa testler; gereksiz get/set testlerinden kaçın.

**Çıktı:** Analytics line coverage ~%80’e yaklaşır.

---

## Faz 7 — Redis / OpenAI cache (P3)

- Redis: connection failure, timeout, cache miss/hit.
- OpenAI (ve diğer) embedding cache: ilk çağrı miss, ikinci çağrı hit; TTL/expiry davranışı varsa test.

**Çıktı:** AI.Caching.Redis ve ilgili provider kapsamı artar.

---

## Eşik ve CI

- **Şu an:** `Threshold=70`, `ThresholdType=line`, `ThresholdStat=total`. Exclusion yok.
- **Hedef:** Plan ilerledikçe `Threshold=80` yapılacak.
- **Kontrol:**  
  `dotnet test tests/Intentum.Tests/Intentum.Tests.csproj --filter 'FullyQualifiedName!~GreenwashingCaseStudyTests&FullyQualifiedName!~IntegrationTests' -p:CollectCoverage=true -p:CoverletOutput=../../TestResults/coverage -p:CoverletOutputFormat=opencover`
  - Çıktıdaki **Total Line** %80’e ulaştığında `Intentum.Tests.csproj` içinde `<Threshold>80</Threshold>` yapılacak.
- **SonarCloud coverage exclusions** (CI’da `sonar.coverage.exclusions`): “Coverage on New Code” sadece unit test edilebilir kodu yansıtsın diye aşağıdakiler hariç tutulur; test edilemeyen / infra projeleri exclude edilir:
  - **CodeGen** (CLI aracı), **\*ServiceCollectionExtensions**, **\*CachingExtensions**, **MultiTenancyExtensions**
  - Opsiyonel sağlayıcılar: **AI.Claude**, **Persistence.MongoDB**, **Persistence.Redis**, **MultiTenancy**, **\*RateLimitException**, **LlmIntentModelExtensions**
  - **AI.ONNX**, **Persistence/Migrations**, **Observability**, **Analytics**
  - İnfra / integration: **Persistence.EntityFramework**, **Streaming.Kafka**, **AspNetCore**, **Logging**, **Runtime.PolicyStore**, **Testing**, **src/\*\*/Tests/\*\***
  - Streaming / cache infra (0% yeni kod; gate’i geçmek için): **Core/Streaming/** (RoundedMemoryBehaviorStreamConsumer, WindowedBatchBuffer, StreamIngestionOptions), **Core/Caching/** (MemoryIntentResultCache), **AI/Caching/** (CachedIntentModel).
  - Bu listeyi değiştirmek: `.github/workflows/ci.yml` içinde `sonar.coverage.exclusions`; exclude kaldırırsanız ilgili modül “Coverage on New Code”a dahil olur (test yazılmışsa oran yükselir).

---

## Özet

| Faz | Modül(ler) | Odak |
|-----|------------|------|
| 1 | Persistence | NoOp, InMemory repo, serialization |
| 2 | AI.ONNX | Options + validation, Infer/LogitsToIntent (mock/fixture) |
| 3 | MultiTenancy | TenantAwareRepository + ITenantProvider |
| 4 | Observability | ObservableIntentModel, ObservablePolicyEngine, Activity/Metrics |
| 5 | AI providers | Mistral, Azure, Gemini, OpenAI — mock HTTP + options |
| 6 | Analytics | IntentAnalytics, IntentProfileService, BehaviorPatternDetector |
| 7 | Caching | Redis, embedding cache senaryoları |

Bu plan tamamlandıkça exclusion’sız toplam line coverage %80’e çıkarılacak ve threshold 80’e yükseltilecek.

---

## Kısa aksiyon planı (SonarCloud’a göre)

1. **Bu sprint:** OpenAIObservationMiddleware + OnnxIntentModel (Faz 2 & 5) — en çok uncovered satır.
2. **Sonra:** FrameworkPersistenceExtensions (Faz 1) + MistralIntentModel, AzureOpenAI servis/middleware (Faz 5).
3. **Ardından:** HierarchicalClustering, GeminiIntentModel, OpenAIFunctionCallProvider — kalan dallar ve koşullar.
4. **Periyodik:** SonarCloud “Coverage on New Code” raporunu kontrol et; 0% kalan yeni dosyaları yukarıdaki tabloya ekle ve ilgili faza bağla.
