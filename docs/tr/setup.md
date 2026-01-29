# Kurulum (TR)

Bu sayfa gereksinimler, paket kurulumu ve Intentum’u uçtan uca çalıştırabilmen için minimal bir ilk proje adımlarını anlatır.

---

## Gereksinimler

- **.NET SDK 10.x** (veya projenin hedeflediği sürüm).

---

## Paket kurulumu (NuGet)

**Çekirdek** (behavior space, intent ve policy için gerekli):

```bash
dotnet add package Intentum.Core
dotnet add package Intentum.Runtime
dotnet add package Intentum.AI
```

**Sağlayıcılar** (opsiyonel; gerçek embedding API’leri için bir veya daha fazlasını seç):

```bash
dotnet add package Intentum.AI.OpenAI
dotnet add package Intentum.AI.Gemini
dotnet add package Intentum.AI.Mistral
dotnet add package Intentum.AI.AzureOpenAI
dotnet add package Intentum.AI.Claude
```

İstersen **Intentum.Providers** ekleyerek Core, Runtime, AI ve tüm sağlayıcı paketlerini tek seferde alabilirsin: `dotnet add package Intentum.Providers`.

Sağlayıcı eklemezsen yerel çalıştırma için **MockEmbeddingProvider** (Intentum.AI içinde) kullan — API anahtarı gerekmez.

---

## İlk proje: minimal konsol uygulaması

1. **Konsol uygulaması oluştur** (yoksa):
   ```bash
   dotnet new console -n MyIntentumApp -o MyIntentumApp
   cd MyIntentumApp
   ```

2. **Çekirdek paketleri ekle** (yukarıdaki gibi).

3. **`Program.cs` içeriğini** minimal akışla değiştir:

```csharp
using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Runtime.Policy;

// 1) Davranış: ne oldu?
var space = new BehaviorSpace()
    .Observe("user", "login")
    .Observe("user", "retry")
    .Observe("user", "submit");

// 2) Intent çıkar (mock sağlayıcı = API anahtarı yok)
var model = new LlmIntentModel(
    new MockEmbeddingProvider(),
    new SimpleAverageSimilarityEngine());
var intent = model.Infer(space);

// 3) Basit bir policy ile karar ver
var policy = new IntentPolicy()
    .AddRule(new PolicyRule(
        "AllowHigh",
        i => i.Confidence.Level is "High" or "Certain",
        PolicyDecision.Allow))
    .AddRule(new PolicyRule(
        "ObserveMedium",
        i => i.Confidence.Level == "Medium",
        PolicyDecision.Observe));

var decision = intent.Decide(policy);

Console.WriteLine($"Güven: {intent.Confidence.Level}, Karar: {decision}");
```

4. **Çalıştır**
   ```bash
   dotnet run
   ```

Bir güven seviyesi ve karar (örn. Allow veya Observe) görmelisin. Sonra: [Senaryolar](scenarios.md) ile daha fazla kural, [Sağlayıcılar](providers.md) ile gerçek sağlayıcı, [API Referansı](api.md) ile tüm tipler.

---

## Gerçek sağlayıcı kullanımı (örn. OpenAI)

1. Sağlayıcı paketini ekle: `dotnet add package Intentum.AI.OpenAI`.
2. Ortam değişkenlerini ayarla (örn. `OPENAI_API_KEY`, `OPENAI_EMBEDDING_MODEL`). Bkz. [Sağlayıcılar](providers.md).
3. Mock’u gerçek sağlayıcı ve options ile değiştir:

```csharp
using Intentum.AI.OpenAI;

var options = OpenAIOptions.FromEnvironment();
var httpClient = new HttpClient { BaseAddress = new Uri(options.BaseUrl ?? "https://api.openai.com/v1/") };
// Auth header ekle, sonra:
var embeddingProvider = new OpenAIEmbeddingProvider(options, httpClient);
var model = new LlmIntentModel(embeddingProvider, new SimpleAverageSimilarityEngine());
```

DI (örn. ASP.NET Core) için `services.AddIntentumOpenAI(options)` kullan ve sağlayıcıyı inject et. Bkz. [Sağlayıcılar](providers.md).

---

## Ortam değişkenleri (özet)

Sadece gerçek HTTP adapter kullanırken ayarla:

| Sağlayıcı | Ana değişkenler |
|-----------|------------------|
| OpenAI | `OPENAI_API_KEY`, `OPENAI_EMBEDDING_MODEL`, `OPENAI_BASE_URL` |
| Gemini | `GEMINI_API_KEY`, `GEMINI_EMBEDDING_MODEL`, `GEMINI_BASE_URL` |
| Mistral | `MISTRAL_API_KEY`, `MISTRAL_EMBEDDING_MODEL`, `MISTRAL_BASE_URL` |
| Azure OpenAI | `AZURE_OPENAI_ENDPOINT`, `AZURE_OPENAI_API_KEY`, `AZURE_OPENAI_EMBEDDING_DEPLOYMENT`, `AZURE_OPENAI_API_VERSION` |
| Claude | `CLAUDE_API_KEY`, `CLAUDE_MODEL`, `CLAUDE_BASE_URL` vb. |

Detay ve örnekler: [Sağlayıcılar](providers.md).

---

## Repo sample’ını derle ve çalıştır

Çözüm birçok paket ve iki örnek uygulama içerir: **Çekirdek:** Intentum.Core, Intentum.Runtime, Intentum.AI. **Sağlayıcılar:** Intentum.AI.OpenAI, Gemini, Mistral, AzureOpenAI, Claude. **Uzantılar:** Intentum.AspNetCore, Testing, Observability, Logging, Persistence, Persistence.EntityFramework, Persistence.Redis, Persistence.MongoDB, Analytics, CodeGen. **Örnekler:** samples/Intentum.Sample (konsol), samples/Intentum.Sample.Web (API + UI, intent infer, analytics, rate limiting).

Reponun kökünden:

```bash
dotnet build Intentum.slnx
```

**Konsol örneği:** `dotnet run --project samples/Intentum.Sample`

**Web örneği:** `dotnet run --project samples/Intentum.Sample.Web` — UI ve Scalar docs (http://localhost:5150/), endpoint'ler: `/api/carbon/calculate`, `/api/orders`, `POST /api/intent/infer`, `GET /api/intent/analytics/summary`, `/api/intent/analytics/export/json`, `/api/intent/analytics/export/csv`, `/health`.

Konsol örneği ESG, Carbon, EU Green Bond, workflow ve klasik (ödeme, destek, e‑ticaret) senaryolarını çalıştırır; varsayılan **mock** embedding kullanır (API anahtarı gerekmez). **Gerçek AI:** `OPENAI_API_KEY` ayarla; bkz. [Sağlayıcılar](providers.md).

---

## Yerel NuGet’ten kurulum (geliştirme)

Intentum’u kaynaktan derleyip yerel paket referansı vermek istersen:

```bash
dotnet pack Intentum.slnx -c Release
dotnet nuget add source /path/to/Intentum/src/Intentum.Core/bin/Release -n IntentumLocal
dotnet add package Intentum.Core --source IntentumLocal
```

Diğer projeler (Intentum.Runtime, Intentum.AI vb.) için gerekirse tekrarla.
