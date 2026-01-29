# Sağlayıcılar (TR)

Intentum, davranışı (örn. `"user:login"`) vektörlere çevirmek için **embedding sağlayıcıları** kullanır; böylece intent modeli güven skorunu çıkarabilir. Yerel çalıştırma ve testler için **mock sağlayıcı** (API anahtarı yok) veya production için **gerçek sağlayıcı** (OpenAI, Gemini, Mistral, Azure OpenAI, Claude) kullanabilirsin.

Bu sayfa her sağlayıcıyı açıklar: ne yapar, env var’lar, minimal kod ve DI kurulumu. **Kolay / orta / zor** kullanım örnekleri için [AI sağlayıcılarını kullanma](ai-providers-howto.md) sayfasına bak. Genel akış (Observe → Infer → Decide) için [API Referansı](api.md) ve [Kurulum](setup.md).

---

## Hangisini ne zaman kullanmalı

| Sağlayıcı | Ne zaman |
|-----------|----------|
| **MockEmbeddingProvider** (Intentum.AI) | Yerel çalıştırma, test, demo — API anahtarı yok. |
| **OpenAI** | Zaten OpenAI kullanıyorsan; iyi model seçimi ve doküman. |
| **Gemini** | Google tercih ediyorsan; genelde iyi gecikme ve fiyat. |
| **Mistral** | Avrupa odaklı veya Mistral modelleri istiyorsan. |
| **Azure OpenAI** | Azure kullanıyorsan veya kurumsal SLA gerekiyorsa. |
| **Claude** | Anthropic kullanıyorsan; mesaj tabanlı intent skoru destekler. |

Uygulama başına **tek** embedding sağlayıcı yeterli; stack ve bölgene uyanı seç. Hepsi aynı akışı kullanır: `LlmIntentModel(embeddingProvider, similarityEngine)` sonra `Infer(space)`.

---

## OpenAI

**Ne yapar:** Davranış anahtarlarını vektörlere çevirmek için OpenAI embedding API’sini (örn. `text-embedding-3-large`) çağırır.

**Env var:** `OPENAI_API_KEY`, `OPENAI_EMBEDDING_MODEL`, `OPENAI_BASE_URL` (opsiyonel, örn. proxy).

**Minimal kod (DI yok):**

```csharp
using Intentum.AI.OpenAI;
using Intentum.AI.Models;
using Intentum.AI.Similarity;

var options = OpenAIOptions.FromEnvironment();
options.Validate();

var httpClient = new HttpClient
{
    BaseAddress = new Uri(options.BaseUrl ?? "https://api.openai.com/v1/")
};
httpClient.DefaultRequestHeaders.Authorization =
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.ApiKey);

var provider = new OpenAIEmbeddingProvider(options, httpClient);
var model = new LlmIntentModel(provider, new SimpleAverageSimilarityEngine());

var space = new BehaviorSpace().Observe("user", "login").Observe("user", "submit");
var intent = model.Infer(space);
```

**DI (örn. ASP.NET Core):**

```csharp
using Intentum.AI.OpenAI;
using Microsoft.Extensions.DependencyInjection;

var options = OpenAIOptions.FromEnvironment();
services.AddIntentumOpenAI(options);

// Sonra IIntentEmbeddingProvider (veya OpenAIEmbeddingProvider) inject et ve LlmIntentModel kur.
```

---

## Gemini

**Ne yapar:** Davranış anahtarlarını vektörlere çevirmek için Google Gemini embedding API’sini (örn. `text-embedding-004`) çağırır.

**Env var:** `GEMINI_API_KEY`, `GEMINI_EMBEDDING_MODEL`, `GEMINI_BASE_URL` (opsiyonel).

**Minimal kod (DI yok):**

```csharp
using Intentum.AI.Gemini;
using Intentum.AI.Models;
using Intentum.AI.Similarity;

var options = GeminiOptions.FromEnvironment();
var httpClient = new HttpClient { BaseAddress = new Uri(options.BaseUrl ?? "https://generativelanguage.googleapis.com/v1beta/") };
var provider = new GeminiEmbeddingProvider(options, httpClient);
var model = new LlmIntentModel(provider, new SimpleAverageSimilarityEngine());
```

**DI:**

```csharp
using Intentum.AI.Gemini;
using Microsoft.Extensions.DependencyInjection;

var options = GeminiOptions.FromEnvironment();
services.AddIntentumGemini(options);
```

---

## Mistral

**Ne yapar:** Davranış anahtarlarını vektörlere çevirmek için Mistral embedding API’sini (örn. `mistral-embed`) çağırır.

**Env var:** `MISTRAL_API_KEY`, `MISTRAL_EMBEDDING_MODEL`, `MISTRAL_BASE_URL` (opsiyonel).

**Minimal kod (DI yok):**

```csharp
using Intentum.AI.Mistral;
using Intentum.AI.Models;
using Intentum.AI.Similarity;

var options = MistralOptions.FromEnvironment();
var httpClient = new HttpClient { BaseAddress = new Uri(options.BaseUrl ?? "https://api.mistral.ai/v1/") };
httpClient.DefaultRequestHeaders.Authorization =
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.ApiKey);
var provider = new MistralEmbeddingProvider(options, httpClient);
var model = new LlmIntentModel(provider, new SimpleAverageSimilarityEngine());
```

**DI:**

```csharp
using Intentum.AI.Mistral;
using Microsoft.Extensions.DependencyInjection;

var options = MistralOptions.FromEnvironment();
services.AddIntentumMistral(options);
```

---

## Azure OpenAI

**Ne yapar:** Davranış anahtarlarını vektörlere çevirmek için Azure OpenAI embedding deployment’ını çağırır. Endpoint + API key + deployment adı kullanır.

**Env var:** `AZURE_OPENAI_ENDPOINT`, `AZURE_OPENAI_API_KEY`, `AZURE_OPENAI_EMBEDDING_DEPLOYMENT`, `AZURE_OPENAI_API_VERSION` (opsiyonel).

**Minimal kod (DI yok):**

```csharp
using Intentum.AI.AzureOpenAI;
using Intentum.AI.Models;
using Intentum.AI.Similarity;

var options = AzureOpenAIOptions.FromEnvironment();
var httpClient = new HttpClient();
// Azure header’da ApiKey kullanır; extension genelde ayarlar.
var provider = new AzureOpenAIEmbeddingProvider(options, httpClient);
var model = new LlmIntentModel(provider, new SimpleAverageSimilarityEngine());
```

**DI:**

```csharp
using Intentum.AI.AzureOpenAI;
using Microsoft.Extensions.DependencyInjection;

var options = AzureOpenAIOptions.FromEnvironment();
services.AddIntentumAzureOpenAI(options);
```

---

## Claude

**Ne yapar:** Anthropic Claude **mesaj tabanlı intent skoru** (ClaudeMessageIntentModel) için kullanılabilir; sadece embedding değil. Varsayılan olarak Claude paketi embedding için stub kullanabilir; tam intent çıkarımı için mesaj skorunu kullan.

**Env var:** `CLAUDE_API_KEY`, `CLAUDE_MODEL`, `CLAUDE_BASE_URL`, `CLAUDE_API_VERSION`, `CLAUDE_USE_MESSAGES_SCORING` (opsiyonel).

**DI (önerilen):**

```csharp
using Intentum.AI.Claude;
using Microsoft.Extensions.DependencyInjection;

var options = ClaudeOptions.FromEnvironment();
services.AddIntentumClaude(options);
```

Sonra intent modelini (örn. ClaudeMessageIntentModel) ihtiyaca göre inject et. Mesaj tabanlı skor kullanımı için paket dokümanına bak.

---

## Güvenlik ve yapılandırma

- **API anahtarlarını asla commit etme.** Ortam değişkenleri veya secret manager kullan.
- **Production’da** ham istek/cevap gövdesini loglama.
- **Bölge ve gecikme:** Gecikme önemliyse kullanıcılara yakın sağlayıcı ve endpoint seç.
- **Rate limit:** Her sağlayıcının limitine uy; production için retry ve backoff (veya ayrı middleware) düşün.

Tam env var isimleri ve opsiyonel alanlar için her sağlayıcının `*Options` sınıfı ve repodaki `FromEnvironment()` kullanımına bak.
