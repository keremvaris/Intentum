# Test (TR)

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

Yani: gerçek API çağırmıyoruz; sahte JSON veya mock sağlayıcı kullanıyoruz ve sağlayıcı ile modelin beklenen `Intent` ve `PolicyDecision` ürettiğini kontrol ediyoruz.

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
