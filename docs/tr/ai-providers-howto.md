# AI sağlayıcılarını kullanma

**Bu sayfayı neden okuyorsunuz?** Bu sayfa her Intentum AI sağlayıcısı (Mock, OpenAI, Gemini, Mistral, Azure, Claude) için kolay / orta / zor kullanım senaryolarını gösterir. Hangi sağlayıcıyı nasıl entegre edeceğinizi veya testten production'a nasıl geçeceğinizi merak ediyorsanız doğru yerdesiniz.

Her Intentum AI sağlayıcısı için **kolay**, **orta** ve **zor** kullanım senaryoları. Kullanım senaryoları sayfasındaki gibi: **Ne**, **İhtiyaç**, **Kod**, **Beklenen sonuç**.

---

## Seviyeler

| Seviye | İhtiyacın olan | Kullanım senaryosu |
|--------|----------------|--------------------|
| **Kolay** | API anahtarı yok | Yerel çalıştırma, test, demo — Mock sağlayıcı. |
| **Orta** | Bir API anahtarı + env var | Konsol veya basit uygulamada tek sağlayıcı. |
| **Zor** | DI, cache veya birden fazla sağlayıcı | ASP.NET Core, embedding cache, fallback. |

---

## Mock (Intentum.AI) — API anahtarı yok

### Kolay senaryo: Yerel test veya demo

**Ne:** Gerçek API çağırmadan niyet çıkarımı yapıyorsun; test, demo veya geliştirme ortamında.

**İhtiyaç:** Hiçbir API anahtarı gerekmez. Sadece `Intentum.AI` (Mock) paketi.

**Kod:**

```csharp
using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;

var provider = new MockEmbeddingProvider();
var model = new LlmIntentModel(provider, new SimpleAverageSimilarityEngine());

var space = new BehaviorSpace()
    .Observe("user", "login")
    .Observe("user", "submit");
var intent = model.Infer(space);
```

**Beklenen sonuç:** `intent.Confidence.Level`, `intent.Confidence.Score` ve `intent.Signals` dolu; gerçek ağ çağrısı yok.

---

### Orta senaryo: Mock ile tek uygulama

**Ne:** Aynı Mock’u tek bir konsol veya basit uygulamada kullanıyorsun; `OPENAI_API_KEY` (vb.) set değil veya test modundasın.

**İhtiyaç:** Kolayla aynı; Mock’un yapılandırması yok.

**Kod:** Kolay senaryodaki kodun aynısı.

**Beklenen sonuç:** Testlerde veya CI’da anahtar olmadan geçen niyet çıkarımı.

---

### Zor senaryo: DI ile sağlayıcı değiştirme

**Ne:** Başlangıçta Mock kullanıyorsun; production’da gerçek sağlayıcıya (OpenAI, Gemini vb.) DI ile geçmek istiyorsun.

**İhtiyaç:** DI konteyneri; gerçek sağlayıcı için env var’lar.

**Kod fikri:** `LlmIntentModel(provider, similarityEngine)` ve `Infer(space)` aynı kalır; sadece `IIntentEmbeddingProvider` kaydını Mock’tan gerçek sağlayıcıya değiştirirsin. Bkz. aşağıdaki Orta senaryolar (OpenAI, Gemini vb.) ve [Sağlayıcılar](providers.md).

**Beklenen sonuç:** Kod değişmeden farklı sağlayıcı kullanımı.

---

## OpenAI (Intentum.AI.OpenAI)

**Env var:** `OPENAI_API_KEY`, `OPENAI_EMBEDDING_MODEL` (opsiyonel), `OPENAI_BASE_URL` (örn. `https://api.openai.com/v1/`).

### Kolay senaryo

**Ne:** API anahtarı olmadan denemek istiyorsun.

**İhtiyaç:** OpenAI için kolay (anahtarsız) yok; bunun yerine Mock kullan (yukarıdaki Mock kolay senaryosu).

**Beklenen sonuç:** Mock ile aynı akış.

---

### Orta senaryo: Konsol veya minimal uygulama

**Ne:** Tek bir konsol uygulamasında veya basit bir serviste OpenAI embedding ile niyet çıkarıyorsun.

**İhtiyaç:** `OPENAI_API_KEY` ve `OPENAI_BASE_URL` ortam değişkenleri.

**Kod:**

```csharp
using Intentum.AI.OpenAI;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;

// OPENAI_API_KEY=sk-... ve OPENAI_BASE_URL=https://api.openai.com/v1/ ayarla
var options = OpenAIOptions.FromEnvironment();
options.Validate();

var httpClient = new HttpClient
{
    BaseAddress = new Uri(options.BaseUrl!)
};
httpClient.DefaultRequestHeaders.Authorization =
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.ApiKey);

var provider = new OpenAIEmbeddingProvider(options, httpClient);
var model = new LlmIntentModel(provider, new SimpleAverageSimilarityEngine());

var space = new BehaviorSpace()
    .Observe("user", "login")
    .Observe("user", "submit");
var intent = model.Infer(space);
```

**Beklenen sonuç:** Gerçek OpenAI embedding çağrısı; `intent` dolu döner.

---

### Zor senaryo: ASP.NET Core + DI + cache

**Ne:** ASP.NET Core’da OpenAI kullanıyorsun; isteğe bağlı embedding cache ile maliyet/gecikme azaltmak istiyorsun.

**İhtiyaç:** DI (`AddIntentumOpenAI`); isteğe bağlı `CachedEmbeddingProvider` + `MemoryEmbeddingCache` ([Gelişmiş Özellikler](advanced-features.md) — cache).

**Kod fikri:** [Sağlayıcılar](providers.md) içindeki `AddIntentumOpenAI(options)`; kayıtlı `IIntentEmbeddingProvider`’ı cache ile sar; `LlmIntentModel(provider, similarityEngine)` kaydet ve `Infer(space)` kullan.

**Beklenen sonuç:** Aynı davranış anahtarları için tekrarlayan embedding çağrıları cache’ten döner.

---

## Gemini (Intentum.AI.Gemini)

**Env var:** `GEMINI_API_KEY`, `GEMINI_EMBEDDING_MODEL` (opsiyonel), `GEMINI_BASE_URL` (örn. `https://generativelanguage.googleapis.com/v1beta/`).

### Kolay senaryo

**Ne:** Anahtarsız deneme.

**İhtiyaç:** Mock kullan (Mock kolay senaryosu).

**Beklenen sonuç:** Mock ile aynı akış.

---

### Orta senaryo: Tek seferlik uygulama

**Ne:** Konsol veya basit uygulamada Google Gemini embedding ile niyet çıkarıyorsun.

**İhtiyaç:** `GEMINI_API_KEY` ve `GEMINI_BASE_URL` (veya varsayılan).

**Kod:**

```csharp
using Intentum.AI.Gemini;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;

var options = GeminiOptions.FromEnvironment();
var httpClient = new HttpClient
{
    BaseAddress = new Uri(options.BaseUrl!)
};
var provider = new GeminiEmbeddingProvider(options, httpClient);
var model = new LlmIntentModel(provider, new SimpleAverageSimilarityEngine());

var space = new BehaviorSpace().Observe("user", "login").Observe("user", "submit");
var intent = model.Infer(space);
```

**Beklenen sonuç:** Gemini embedding çağrısı; `intent` dolu döner.

---

### Zor senaryo: DI + cache

**Ne:** ASP.NET Core’da Gemini; isteğe bağlı cache.

**İhtiyaç:** `AddIntentumGemini(options)`; isteğe bağlı `CachedEmbeddingProvider` + `MemoryEmbeddingCache`.

**Kod fikri:** OpenAI zor senaryosu ile aynı kalıp; sağlayıcı adı Gemini.

**Beklenen sonuç:** Cache ile tekrarlayan çağrılar azalır.

---

## Mistral (Intentum.AI.Mistral)

**Env var:** `MISTRAL_API_KEY`, `MISTRAL_EMBEDDING_MODEL` (opsiyonel), `MISTRAL_BASE_URL` (örn. `https://api.mistral.ai/v1/`).

### Kolay senaryo

**Ne:** Anahtarsız deneme.

**İhtiyaç:** Mock kullan.

**Beklenen sonuç:** Mock ile aynı akış.

---

### Orta senaryo: Tek sağlayıcı ile uygulama

**Ne:** Konsol veya basit uygulamada Mistral embedding ile niyet çıkarıyorsun.

**İhtiyaç:** `MISTRAL_API_KEY` ve `MISTRAL_BASE_URL` (veya varsayılan).

**Kod:**

```csharp
using Intentum.AI.Mistral;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;

var options = MistralOptions.FromEnvironment();
var httpClient = new HttpClient { BaseAddress = new Uri(options.BaseUrl!) };
httpClient.DefaultRequestHeaders.Authorization =
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.ApiKey);

var provider = new MistralEmbeddingProvider(options, httpClient);
var model = new LlmIntentModel(provider, new SimpleAverageSimilarityEngine());

var space = new BehaviorSpace().Observe("user", "login").Observe("user", "submit");
var intent = model.Infer(space);
```

**Beklenen sonuç:** Mistral embedding çağrısı; `intent` dolu döner.

---

### Zor senaryo: DI + cache

**Ne:** ASP.NET Core’da Mistral; isteğe bağlı cache.

**İhtiyaç:** `services.AddIntentumMistral(options)`; isteğe bağlı cache (Gelişmiş Özellikler).

**Beklenen sonuç:** OpenAI/Gemini zor senaryosu ile aynı mantık.

---

## Azure OpenAI (Intentum.AI.AzureOpenAI)

**Env var:** `AZURE_OPENAI_ENDPOINT`, `AZURE_OPENAI_API_KEY`, `AZURE_OPENAI_EMBEDDING_DEPLOYMENT` (opsiyonel), `AZURE_OPENAI_API_VERSION` (opsiyonel).

### Kolay senaryo

**Ne:** Anahtarsız deneme.

**İhtiyaç:** Mock kullan.

**Beklenen sonuç:** Mock ile aynı akış.

---

### Orta senaryo: Azure ile tek uygulama

**Ne:** Konsol veya basit uygulamada Azure OpenAI embedding deployment ile niyet çıkarıyorsun.

**İhtiyaç:** `AZURE_OPENAI_ENDPOINT` ve `AZURE_OPENAI_API_KEY`.

**Kod:**

```csharp
using Intentum.AI.AzureOpenAI;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;

var options = AzureOpenAIOptions.FromEnvironment();
var httpClient = new HttpClient();
var provider = new AzureOpenAIEmbeddingProvider(options, httpClient);
var model = new LlmIntentModel(provider, new SimpleAverageSimilarityEngine());

var space = new BehaviorSpace().Observe("user", "login").Observe("user", "submit");
var intent = model.Infer(space);
```

**Beklenen sonuç:** Azure OpenAI embedding çağrısı; `intent` dolu döner.

---

### Zor senaryo: DI + cache

**Ne:** ASP.NET Core’da Azure OpenAI; isteğe bağlı cache.

**İhtiyaç:** `services.AddIntentumAzureOpenAI(options)`; isteğe bağlı cache.

**Beklenen sonuç:** Diğer sağlayıcılarla aynı kalıp.

---

## Claude (Intentum.AI.Claude)

**Env var:** `CLAUDE_API_KEY`, `CLAUDE_MODEL`, `CLAUDE_BASE_URL`, `CLAUDE_API_VERSION`, `CLAUDE_USE_MESSAGES_SCORING` (opsiyonel). Claude hem embedding hem **mesaj tabanlı intent skoru** (ClaudeMessageIntentModel) destekler.

### Kolay senaryo

**Ne:** Anahtarsız deneme.

**İhtiyaç:** Mock kullan.

**Beklenen sonuç:** Mock ile aynı akış.

---

### Orta senaryo: DI ile Claude, IIntentModel kullanımı

**Ne:** Konsol veya basit uygulamada Claude ile niyet çıkarıyorsun; DI ile kayıt edip `IIntentModel` alıyorsun (embedding veya mesaj tabanlı skor).

**İhtiyaç:** `CLAUDE_*` env var’ları; `AddIntentumClaude(options)` ile DI.

**Kod:**

```csharp
using Intentum.AI.Claude;
using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Microsoft.Extensions.DependencyInjection;

var options = ClaudeOptions.FromEnvironment();
var services = new ServiceCollection();
services.AddIntentumClaude(options);
var sp = services.BuildServiceProvider();
var model = sp.GetRequiredService<IIntentModel>();
var space = new BehaviorSpace().Observe("user", "login").Observe("user", "submit");
var intent = model.Infer(space);
```

**Beklenen sonuç:** Claude (embedding veya mesaj tabanlı, `CLAUDE_USE_MESSAGES_SCORING` ile) ile `intent` dolu döner. Tam tipler için [Sağlayıcılar](providers.md).

---

### Zor senaryo: DI + cache

**Ne:** ASP.NET Core’da Claude; isteğe bağlı cache; mesaj veya embedding modu.

**İhtiyaç:** `AddIntentumClaude(options)`; isteğe bağlı cache; paket API’sine göre model seçimi.

**Beklenen sonuç:** Diğer sağlayıcılarla aynı kalıp; ClaudeMessageIntentModel veya ClaudeIntentModel.

---

## Özet tablo

| Sağlayıcı | Kolay | Orta | Zor |
|-----------|-------|------|-----|
| **Mock** | MockEmbeddingProvider, anahtar yok | Aynı | DI’da sağlayıcı değiştir |
| **OpenAI** | Mock kullan | FromEnvironment + HttpClient + LlmIntentModel | DI + opsiyonel cache |
| **Gemini** | Mock kullan | FromEnvironment + HttpClient + LlmIntentModel | DI + opsiyonel cache |
| **Mistral** | Mock kullan | FromEnvironment + HttpClient + LlmIntentModel | DI + opsiyonel cache |
| **Azure OpenAI** | Mock kullan | FromEnvironment + HttpClient + LlmIntentModel | DI + opsiyonel cache |
| **Claude** | Mock kullan | AddIntentumClaude + IIntentModel | DI + opsiyonel cache |

---

## Güvenlik

- API anahtarlarını asla commit etme; ortam değişkenleri veya secret manager kullan.
- Production’da ham istek/cevap gövdesini loglama.
- Tüm env var isimleri ve opsiyonel alanlar için her sağlayıcının `*Options` sınıfı ve repodaki `FromEnvironment()` kullanımına bak. Ayrıca [Sağlayıcılar](providers.md) ve [Kurulum](setup.md).

**Sonraki adım:** Bu sayfayı bitirdiyseniz → [Sağlayıcılar](providers.md) veya [Senaryolar](scenarios.md).
