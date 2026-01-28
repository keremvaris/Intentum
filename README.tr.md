Intentum - Yapay Zeka Çağı için Intent-Driven Development

[![CI](https://github.com/keremvaris/Intentum/actions/workflows/ci.yml/badge.svg)](https://github.com/keremvaris/Intentum/actions/workflows/ci.yml)
[![NuGet Intentum.Core](https://img.shields.io/nuget/v/Intentum.Core.svg)](https://www.nuget.org/packages/Intentum.Core)
[![Coverage](https://keremvaris.github.io/Intentum/coverage/badge_linecoverage.svg)](https://keremvaris.github.io/Intentum/coverage/index.html)
[![SonarCloud](https://sonarcloud.io/api/project_badges/measure?project=keremvaris_Intentum&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=keremvaris_Intentum)

Intentum, senaryo tabanlı BDD yaklaşımını davranış uzayı çıkarımıyla değiştirir.
Odak noktası, kullanıcının ne yaptığı değil ne yapmaya çalıştığıdır.

[English](README.md) | Türkçe

**Lisans:** [MIT](LICENSE) · **Katkıda bulunma** — [CONTRIBUTING.md](CONTRIBUTING.md) · [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) · [SECURITY.md](SECURITY.md)

Neden Intentum?
- Deterministik olmayan akışlar artık yaygın.
- AI tabanlı sistemler sürekli adapte olur ve drift yaşar.
- Klasik assert/test yaklaşımı katı kalır; niyet daha gerçekçidir.

Ne zaman kullanılmamalı
- Tamamen deterministik, düşük varyanslı ve stabil gereksinimli sistemler.
- Davranış drift’i önem taşımayan küçük script veya tek seferlik işler.

BDD vs Intentum
- BDD: Senaryo odaklı, deterministik, pass/fail
- Intentum: Davranış odaklı, olasılıksal, policy kararları

Dokümantasyon
- GitHub Pages (EN/TR): https://keremvaris.github.io/Intentum/
- English docs: docs/en/index.md
- Türkçe docs: docs/tr/index.md
  - GitHub’da etkinleştirme: Settings -> Pages -> Source: GitHub Actions
- API referansı (otomatik): https://keremvaris.github.io/Intentum/api/

Çekirdek Kavramlar
Davranış Uzayı
Senaryo yazmak yerine davranış gözlemlenir.

Niyet Çıkarımı
Davranış uzayından niyet ve güven skoru çıkarılır.

Policy Kararları
Pass/fail yerine policy tabanlı kararlar.

Hızlı Örnek
```csharp
using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Runtime.Policy;

var space = new BehaviorSpace()
    .Observe("user", "login")
    .Observe("user", "retry")
    .Observe("user", "submit");

var intentModel = new LlmIntentModel(
    new MockEmbeddingProvider(),
    new SimpleAverageSimilarityEngine());

var intent = intentModel.Infer(space);

var policy = new IntentPolicy()
    .AddRule(new PolicyRule(
        "HighConfidenceAllow",
        i => i.Confidence.Level is "High" or "Certain",
        PolicyDecision.Allow));

var decision = intent.Decide(policy);
```

Sample çalıştırma
```bash
dotnet run --project samples/Intentum.Sample
```

Showcase çıktısı (kısaltılmış)
```
=== INTENTUM SCENARIO: PaymentHappyPath ===
Events            : 2
Intent Confidence : High
Decision          : Allow
Behavior Vector:
 - user:login = 1
 - user:submit = 1

=== INTENTUM SCENARIO: PaymentWithRetries ===
Events            : 4
Intent Confidence : Medium
Decision          : Observe
Behavior Vector:
 - user:login = 1
 - user:retry = 2
 - user:submit = 1

=== INTENTUM SCENARIO: SuspiciousRetries ===
Events            : 4
Intent Confidence : Medium
Decision          : Block
Behavior Vector:
 - user:login = 1
 - user:retry = 3
```

Paketler
- Intentum.Core
- Intentum.Runtime
- Intentum.AI
- Intentum.AI.OpenAI
- Intentum.AI.Gemini
- Intentum.AI.Claude
- Intentum.AI.Mistral
- Intentum.AI.AzureOpenAI

Konfigürasyon (env değişkenleri)
- OPENAI_API_KEY, OPENAI_EMBEDDING_MODEL, OPENAI_BASE_URL
- GEMINI_API_KEY, GEMINI_EMBEDDING_MODEL, GEMINI_BASE_URL
- MISTRAL_API_KEY, MISTRAL_EMBEDDING_MODEL, MISTRAL_BASE_URL
- AZURE_OPENAI_ENDPOINT, AZURE_OPENAI_API_KEY, AZURE_OPENAI_EMBEDDING_DEPLOYMENT, AZURE_OPENAI_API_VERSION

Güvenlik
- API anahtarlarını asla repoya koyma. Ortam değişkeni veya secret manager kullan.
- Üretimde sağlayıcı istek/cevaplarını ham olarak loglama.

Not
AI adapter’ları v1.0’da deterministik stub olarak geliyor. Gerçek HTTP çağrıları v1.1 hedefi.

Lisans
MIT
