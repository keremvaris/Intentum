# Intentum ile Başlarken

## Intentum Gerçekte Ne Yapar?

Intentum, **niyet odaklı geliştirme** için bir .NET çerçevesidir: kullanıcı/sistem olaylarını gözlemleyin, ne yapmak istediklerini çıkarın ve bu niyete göre karar verin.

```
BehaviorEvent → BehaviorSpace → BehaviorVector → Intent (Confidence ile) → PolicyDecision
```

## Kurulum

```bash
# Sadece çekirdek (kurallar, politikalar, pipeline)
dotnet add package Intentum.Core
dotnet add package Intentum.Runtime

# AI desteği ile (embedding'ler, LLM sınıflandırma)
dotnet add package Intentum.AI
dotnet add package Intentum.AI.OpenAI  # veya .Gemini, .Mistral, .AzureOpenAI, .Claude

# Hepsi tek pakette
dotnet add package Intentum.Providers
```

## Hızlı Başlangıç: Kural Tabanlı Niyet Tespiti

En basit ve güvenilir yaklaşım — AI gerekmez:

```csharp
using Intentum.Core.Behavior;
using Intentum.Core.Models;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

// 1. Davranış olaylarını gözlemle
var space = new BehaviorSpace();
space.Observe(new BehaviorEvent("user:123", "login.failed"));
space.Observe(new BehaviorEvent("user:123", "login.failed"));
space.Observe(new BehaviorEvent("user:123", "login.failed"));
space.Observe(new BehaviorEvent("user:123", "password.reset"));

// 2. Niyet çıkarmak için kurallar tanımla
var model = new RuleBasedIntentModel()
    .AddRule(space =>
    {
        var failedLogins = space.Events.Count(e => e.Action.Contains("login.failed"));
        var hasReset = space.Events.Any(e => e.Action.Contains("password.reset"));
        if (failedLogins >= 3 && hasReset)
            return new RuleMatch("AccountTakeover", 0.9, "Çoklu başarısız giriş + şifre sıfırlama");
        return null;
    });

// 3. Niyeti çıkar
var intent = model.Infer(space);
// intent.Name = "AccountTakeover", intent.Confidence.Score = 0.9

// 4. Politika tanımla ve karar ver
var policy = new IntentPolicyBuilder()
    .Block("HighRiskFraud", i => i.Confidence.Score >= 0.8)
    .Escalate("Suspicious", i => i.Confidence.Score >= 0.5)
    .Allow("Safe", _ => true)
    .Build();

var decision = intent.Decide(policy);
// decision = PolicyDecision.Block
```

## Maliyet Verimli AI: ChainedIntentModel

Önce kuralları kullan (hızlı + ücretsiz), kurallar karar veremezse yalnızca o zaman AI’ya düş:

```csharp
using Intentum.Core.Models;
using Intentum.AI.Models;
using Intentum.AI.Mock;
using Intentum.AI.Similarity;

var rules = new RuleBasedIntentModel()
    .AddRule(space => { /* kurallarınız */ return null; });

var llm = new LlmIntentModel(
    new MockEmbeddingProvider(),  // production için OpenAIEmbeddingProvider ile değiştir
    new SimpleAverageSimilarityEngine());

var chained = new ChainedIntentModel(rules, llm);
var intent = chained.Infer(space);
// Önce kurallar denenir; eşleşme yoksa LLM çağrılır
```

## Hazır Kural Kütüphaneleri

### Dolandırıcılık Tespiti
```csharp
using Intentum.Core.Fraud;
using Intentum.Runtime.Fraud;

var model = new RuleBasedIntentModel();
foreach (var rule in FraudRules.AllRules())
    model.AddRule(rule);

var policy = FraudPolicies.Standard();
```

### E-Ticaret Niyeti
```csharp
using Intentum.Core.Commerce;

var model = new RuleBasedIntentModel();
foreach (var rule in CommerceRules.AllRules())
    model.AddRule(rule);
```

### Kullanıcı Davranış Analitiği
```csharp
using Intentum.Core.UBA;

var model = new RuleBasedIntentModel();
foreach (var rule in UserBehaviorRules.AllRules())
    model.AddRule(rule);
```

## Katalog ile AI Destekli Sınıflandırma

Embedding kullanarak gerçek niyet sınıflandırması (sadece puanlama değil):

```csharp
using Intentum.AI.Catalog;
using Intentum.AI.Models;

var catalog = new IntentCatalog()
    .Define("PurchaseIntent", "Kullanıcı satın almak istiyor", "cart.add", "checkout.view", "payment.start")
    .Define("SupportIntent", "Kullanıcı yardım arıyor", "faq.view", "contact.click", "return.request")
    .Define("BrowsingIntent", "Kullanıcı sadece bakıyor", "product.view", "search", "category.browse");

// Embedding'leri çöz (bir kez, sonucu önbelleğe al)
await catalog.ResolveEmbeddingsAsync(embeddingProvider);

// Sınıflandırma için katalog kullan
var model = new CatalogIntentModel(embeddingProvider, catalog);
var intent = model.Infer(space);
// "AI-Inferred-Intent" değil, katalogdaki gerçek niyet adı döner
```

## ASP.NET Core Entegrasyonu

```csharp
// Program.cs
builder.Services.AddIntentumPersistenceInMemory();
builder.Services.AddSingleton<IIntentEmbeddingProvider, MockEmbeddingProvider>();

// Kimlik doğrulama (opsiyonel)
builder.Services.AddIntentumAuth(opts =>
{
    opts.JwtSecret = "your-secret-key-min-32-chars-long!!";
    opts.ApiKeys.Add(new() { Key = "your-api-key", Name = "MyApp", Roles = ["Admin"] });
});

app.UseAuthentication();
app.UseAuthorization();
```

## Paket Rehberi

| Paket | Kullanım Alanı |
|-------|----------------|
| `Intentum.Core` | Kurallar, behavior space'ler, niyet modelleri |
| `Intentum.Runtime` | Politikalar, kararlar, rate limiting |
| `Intentum.AI` | Embedding'ler, benzerlik motorları, AI çıkarımı |
| `Intentum.AI.OpenAI` | OpenAI embedding sağlayıcı |
| `Intentum.AspNetCore` | Middleware, health check'ler, auth |
| `Intentum.Analytics` | Anomali tespiti, güven eğilimleri |
| `Intentum.Experiments` | İstatistiksel anlamlılıkla A/B testi |
| `Intentum.Providers` | Meta-paket: yukarıdakilerin hepsi |

## Örnek uygulamayı çalıştırın

**Intentum.Sample.Blazor** uygulaması kütüphaneyle aynı kabiliyetleri gösterir: **infer** (kural tabanlı ve katalog/LLM), **politika** (Allow/Block/Warn/Escalate), **analitik** (Z-score/IQR anomali, sinyaller, export) ve **deneyler** (p-value ile A/B testi). Arayüzde gördüğünüz çıktılar yapıldı ve kullanılıyor; demolardaki olay kaynakları (örn. "Demo Başlat") örnek amaçlı simüle edilir. Bkz. [samples/Intentum.Sample.Blazor/README.md](../../samples/Intentum.Sample.Blazor/README.md) ve uygulamadaki Genel Bakış sayfasında "Gerçek / Simülasyon" kartı.
