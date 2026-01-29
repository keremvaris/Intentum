# Intentum

**Yapay Zeka Çağı için Niyet Odaklı Geliştirme**

[![CI](https://github.com/keremvaris/Intentum/actions/workflows/ci.yml/badge.svg)](https://github.com/keremvaris/Intentum/actions/workflows/ci.yml)
[![NuGet Intentum.Core](https://img.shields.io/nuget/v/Intentum.Core.svg)](https://www.nuget.org/packages/Intentum.Core)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=keremvaris_Intentum&metric=coverage)](https://sonarcloud.io/summary/new_code?id=keremvaris_Intentum)
[![SonarCloud](https://sonarcloud.io/api/project_badges/measure?project=keremvaris_Intentum&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=keremvaris_Intentum)

Çoğu yazılım çerçevesi şunu sorar:
> *Ne oldu?*

Intentum ise olaya böyle bakar:
> **Sistem ne yapmaya çalışıyordu?**

Modern sistemler artık deterministik değil.
Adapte olur, tekrar dener, çıkarım yapar, tahmin eder.
Yine de onları doğrusal senaryolarla test ediyoruz.

Intentum, senaryo tabanlı testi **niyet uzayları** ile değiştirir —
davranış sinyal olarak ele alınır,
doğruluk **güven** ile ölçülür, kesinlikle değil.

Sisteminiz şunları içeriyorsa:
- AI veya olasılıksal mantık
- kullanıcı belirsizliği
- adaptif iş akışları
- deterministik olmayan sonuçlar

Intentum sadece bir alternatif değil; bir sonraki adım.

---

> **Yazılım olaylara göre değil, niyete göre yargılanmalıdır.**

[English](README.md) | Türkçe

**Lisans:** [MIT](LICENSE) · **Katkıda bulunma** — [CONTRIBUTING.md](CONTRIBUTING.md) · [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) · [SECURITY.md](SECURITY.md)

---

## Intentum Manifestosu

Intentum sekiz ilkeye dayanıyor:

- Yazılım artık deterministik değil.
- Davranış niyet değil.
- Senaryolar kırılgan; niyet dayanıklı.
- Testler senaryo değil, uzay tanımlıyor.
- AI Given–When–Then'i bozuyor.
- Niyet yeni sözleşme.
- Başarısızlıklar sinyal.
- Kontrol değil, anlama için tasarlıyoruz.

**Tam metin:** [Intentum Manifestosu](docs/tr/manifesto.md) (8 ilke). Özet kurallar: [Intentum Canon](docs/tr/intentum-canon.md) (10 ilke).

---

## Intentum vs mevcut yaklaşımlar

### Kavramsal karşılaştırma

| Yaklaşım | Merkez | Varsayım | Uygun olduğu |
|----------|--------|----------|--------------|
| TDD | Doğruluk | Deterministik | Algoritmalar |
| BDD | Senaryo | Lineer akış | İş kuralları |
| DDD | Model | Stabil domain | Kurumsal sistemler |
| **Intentum** | **Niyet** | **Belirsizlik** | **AI ve adaptif sistemler** |

### Given–When–Then vs Intentum

| BDD | Intentum |
|-----|----------|
| Given (durum) | Gözlemlenen sinyaller |
| When (eylem) | Davranış evrimi |
| Then (assertion) | Niyet güveni |
| Boolean sonuç | Olasılıksal sonuç |
| Kırılgan senaryolar | Dayanıklı niyet uzayları |

**BDD “Bu oldu mu?” der.**  
**Intentum “Bu mantıklı mı?” der.**

### Test felsefesi

| Soru | BDD | Intentum |
|------|-----|----------|
| Test neyi temsil eder? | Bir hikâye | Bir uzay |
| Başarısızlık nedir? | Hata | Sinyal |
| Retry | Failure | Bağlam |
| Edge case | İstisna | Beklenen |
| Değişime dayanıklılık | Düşük | Yüksek |

---

## Intentum ne DEĞİLDİR / nedir

**Intentum şunlar DEĞİLDİR:**
- bir test çerçevesi yerine geçen
- bir BDD eklentisi
- bir kural motoru
- sihirli bir AI sarmalayıcı

**Intentum şunlar:**
- bir niyet modelleme çerçevesi
- davranış için bir akıl yürütme katmanı
- AI çağı doğruluğu için bir temel

---

## Başlarken (5 dakika)

### 1. Paketleri yükle

```bash
dotnet add package Intentum.Core
dotnet add package Intentum.Runtime
```

AI destekli çıkarım için (isteğe bağlı):

```bash
dotnet add package Intentum.AI
```

### 2. Gözlemlenen davranışı tanımla

Intentum’da test bir senaryo değildir. Gözlemlenen davranışların bir kümesidir.

```csharp
using Intentum.Core;
using Intentum.Core.Behavior;

var space = new BehaviorSpace()
    .Observe("user", "login.failed")
    .Observe("user", "login.retry")
    .Observe("user", "password.reset.requested")
    .Observe("user", "login.success");
```

Asıl soru “Giriş başarılı mı?” değil; “Kullanıcı ne yapmaya çalışıyordu?”

### 3. Niyeti çıkar

```csharp
using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;

var intentModel = new LlmIntentModel(
    new MockEmbeddingProvider(),
    new SimpleAverageSimilarityEngine());
var intent = intentModel.Infer(space);
```

Bu çağrı ne kurallara ne akışa ne sıraya bakar; sadece davranış sinyallerini yorumlar. (Mock ile API anahtarı gerekmez; canlı ortamda gerçek sağlayıcı kullanın.)

### 4. Adımlara değil, niyete göre doğrula

Güvene (Level: Low, Medium, High, Certain) göre doğrulayın; özel model kullanıyorsanız niyet adını da kontrol edebilirsiniz:

```csharp
// intent.Confidence.Level "High" veya "Certain"
// intent.Confidence.Score > 0.75
// Özel niyet modellerinde: intent.Name == "AccountAccess"
```

Bu da bir test — ama adım adım senaryo takip etmez, kırılgan değildir ve alternatif yolları da kabul eder.

### 5. Neden önemli?

Aynı niyet farklı davranışlarla yakalanabilir:

```csharp
var space1 = new BehaviorSpace()
    .Observe("user", "password.reset")
    .Observe("user", "email.confirmed")
    .Observe("user", "login.success");
// intent = intentModel.Infer(space1);
```

Veya:

```csharp
var space2 = new BehaviorSpace()
    .Observe("user", "login.failed")
    .Observe("user", "login.failed")
    .Observe("user", "account.locked");
// intent = intentModel.Infer(space2);
```

Senaryo değişse de niyet aynı kalır. BDD burada devre dışı kalır; Intentum tam burada devreye girer.

### 6. Zihinsel model

| Yaklaşım | Sonuç |
|----------|--------|
| Event'ler / Akışlar / Senaryolar | gürültü / varsayımlar / kırılganlık |
| **Niyet / Güven / Uzay** | anlam / doğruluk / dayanıklılık |

### 7. Intentum ne zaman kullanılmalı?

**Intentum’u kullan:** Sonuçlar değişkense, tekrar denemeler normalse, kararı AI veriyorsa, kullanıcılar hep aynı senaryoyu izlemiyorsa.

**Intentum’a gerek yok:** Mantık tamamen deterministikse, her adımın mutlaka atılması gerekiyorsa, her hata sistemi durdurmalıysa.

### 8. Sırada ne var?

- AI modellerini bağla ([Sağlayıcılar](docs/tr/providers.md))
- Özel niyet sınıflandırıcıları kur
- Intentum’u mevcut testlerle birlikte kullan

Intentum mevcut testlerinizin yerini almaz; onların anlatamadığını anlatır.

---

## Hızlı örnek (policy ile)

```csharp
using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Runtime;
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

**Örnek projeyi çalıştırmak için:**

```bash
dotnet run --project samples/Intentum.Sample
```

**Gelişmiş örnek (dolandırıcılık / kötüye kullanım niyeti):**

```bash
dotnet run --project examples/fraud-intent
```

API anahtarı gerekmez. Şüpheli vs. meşru davranışı çıkarır; policy Block / Observe / Allow kararı verir. Bkz. [Gerçek dünya senaryoları — Dolandırıcılık](docs/tr/real-world-scenarios.md).

---

## Dokümantasyon

- **GitHub Pages (EN/TR):** https://keremvaris.github.io/Intentum/
- [Neden Intentum](docs/tr/why-intentum.md) — isim, felsefe, konumlandırma
- [Intentum Manifestosu](docs/tr/manifesto.md) — sekiz ilke
- [Intentum Canon](docs/tr/intentum-canon.md) — Niyet Odaklı Geliştirme için on ilke
- [Yol haritası](docs/tr/roadmap.md) — v1.0 kriterleri, benimsenme ve derinlik
- [Mimari](docs/tr/architecture.md) — çekirdek akış, paketler, çıkarım pipeline’ı
- [Kurulum](docs/tr/setup.md) — yükleme, ilk proje, env değişkenleri
- [API Referansı](https://keremvaris.github.io/Intentum/api/)
- [CodeGen](docs/tr/codegen.md) — CQRS + Intentum iskeleti, dotnet new şablonu
- **Sample.Web:** `dotnet run --project samples/Intentum.Sample.Web` — UI ve `POST /api/intent/infer`, analytics, health
- **Dolandırıcılık niyeti (gelişmiş örnek):** `dotnet run --project examples/fraud-intent` — dolandırıcılık/kötüye kullanım niyet tespiti, policy Block/Observe/Allow, API anahtarı yok

---

## Sloganlar

- **Vizyon:** *Yazılım olaylara göre değil, niyete göre yargılanmalıdır.*
- **Geliştirici:** *Senaryolardan niyet uzaylarına.* / *Kullanıcıların ne yapmaya çalıştığını anla, sadece ne yaptıklarını değil.*
- **Kısa (NuGet/GitHub):** Intentum, davranışı deterministik senaryolar yerine niyet uzayları olarak modelleyen bir Niyet Odaklı Geliştirme çerçevesidir.

---

## Paketler

- **Çekirdek:** Intentum.Core, Intentum.Runtime, Intentum.AI
- **AI sağlayıcılar:** Intentum.AI.OpenAI, Intentum.AI.Gemini, Intentum.AI.Claude, Intentum.AI.Mistral, Intentum.AI.AzureOpenAI
- **Eklentiler:** Intentum.Testing, Intentum.AspNetCore, Intentum.Observability, Intentum.Logging
- **Persistence:** Intentum.Persistence, Intentum.Persistence.EntityFramework, Intentum.Analytics
- **Gelişmiş:** Intentum.AI.Caching.Redis, Intentum.Clustering, Intentum.Events, Intentum.Experiments, Intentum.MultiTenancy, Intentum.Explainability, Intentum.Simulation, Intentum.Versioning — bkz. [Gelişmiş Özellikler](docs/tr/advanced-features.md)

---

## Konfigürasyon (env değişkenleri)

OPENAI_API_KEY, GEMINI_API_KEY, MISTRAL_API_KEY, AZURE_OPENAI_* — ayrıntı için [Kurulum](docs/tr/setup.md) ve [Sağlayıcılar](docs/tr/providers.md).

---

## Güvenlik

API anahtarlarını asla repoya ekleme. Ortam değişkeni veya secret manager kullan. Canlıda sağlayıcıya giden/gelen ham veriyi loglama.

---

## Not

AI adapter’ları v1.0’da deterministik stub kullanır. Gerçek HTTP çağrıları v1.1’de planlanıyor.

---

## CI ve yayınlama

- **CI** `master`’a push/PR’da çalışır: build, test, coverage, SonarCloud. Analizi açmak için GitHub Secrets’ta `SONAR_TOKEN` tanımla.
- **Sürümleme** git tag’lerinden [MinVer](https://github.com/adamralph/minver) ile gelir. Tag `v1.0.1` → paket `1.0.1`.
- **Yayın:** `./release.sh` veya tag `v1.0.x` push et; `.github/workflows/` ve [CONTRIBUTING.md](CONTRIBUTING.md) sayfasına bak.

---

## Lisans

MIT
