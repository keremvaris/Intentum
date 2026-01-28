# Intentum Dokümantasyonu (TR)

Intentum dokümantasyonuna hoş geldin. Bu doküman projene kurulum, yapılandırma ve kullanım konusunda yardımcı olur.

---

## Intentum nedir?

Intentum, davranışın tam deterministik olmadığı sistemler için **intent odaklı** bir yaklaşımdır: klasik BDD’deki gibi sabit senaryo adımları yerine, ne olduğunu **gözlemlersin**, kullanıcının veya sistemin **intent’ini** (isteğe bağlı AI embedding’leriyle) **çıkarırsın** ve policy kurallarıyla ne yapacağına **karar verirsin** (Allow, Observe, Warn, Block).

- **Behavior Space** — Olayları kaydedersin (örn. login, retry, submit). Sabit “Given/When/Then” adımları yok.
- **Intent** — Model bu davranıştan intent ve güven (High / Medium / Low) çıkarır.
- **Policy** — Kurallar intent’i kararlara eşler: örn. “yüksek güven → Allow”, “çok fazla retry → Block”.

Yani: *gözlemle → çıkar → karar ver*. Akışlar değişkense, AI adapte oluyorsa veya adımlardan çok intent önemliyse kullanışlıdır.

---

## AI ile ne yapıyoruz?

Intentum’da **Infer** adımı isteğe bağlı **AI (embedding)** kullanır: davranış anahtarlarını (örn. `user:login`, `analyst:prepare_esg_report`) vektörlere çevirir, benzerlik skorundan güven seviyesi (High / Medium / Low / Certain) ve sinyalleri üretir.

| Adım | Ne olur |
|------|---------|
| **Embedding** | Her `actor:action` anahtarı bir **embedding sağlayıcı** ile vektör + skor alır. **Mock** (yerel/test) = hash tabanlı, deterministik; **gerçek sağlayıcı** (OpenAI, Gemini, Mistral, Azure, Claude) = anlamsal vektörler, aynı davranış farklı modellerde hafif farklı güven verebilir. |
| **Similarity** | **Similarity engine** tüm embedding’leri tek bir skora indirger (örn. ortalama). Bu skor güven seviyesine dönüştürülür. |
| **Intent** | **LlmIntentModel** bu skordan **Intent** (Confidence + Signals) üretir; policy bu intent’e göre Allow / Observe / Warn / Block verir. |

Örneklerde genelde **Mock** kullanılır (API anahtarı yok). Gerçek AI ile denemek için ortam değişkeniyle bir sağlayıcı seçip aynı akışı çalıştırabilirsin; bkz. [Sağlayıcılar](providers.md) ve [Kurulum – gerçek sağlayıcı](setup.md#gerçek-sağlayıcı-kullanımı-örneğin-openai).

---

## Given/When/Then yerine ne geldi?

Klasik BDD’de **Given** (ön koşul), **When** (aksiyon), **Then** (beklenti) yazarsın. Bu, sabit adımlar ve tek bir pass/fail sonucu varsayar. Intentum Given/When/Then kullanmaz; deterministik olmayan ve intent odaklı sistemlere uyan farklı bir akış kullanır.

| BDD (Given/When/Then) | Intentum (yerine gelen) |
|-----------------------|--------------------------|
| **Given** — sabit ön koşullar | **Observe** — Gerçekten ne olduğunu (login, retry, submit gibi olayları) bir **BehaviorSpace** içine kaydedersin. Sabit “given” durumu yok; gerçek davranışı yakalarsın. |
| **When** — tek aksiyon | Aynı **Observe** — olaylar “when”dir; `space.Observe(actor, action)` ile eklenir. Birden fazla olay, sıra korunur. |
| **Then** — tek assertion, pass/fail | **Infer** + **Decide** — model davranıştan **intent** ve güven (High/Medium/Low) **çıkarır**, sonra bir **policy** sonucu **belirler**: **Allow**, **Observe**, **Warn** veya **Block**. Yani “then X should be true” yerine “bu davranışa göre intent Y, karar Z” alırsın. |

Kısaca: **Given/When/Then kalktı; yerine Observe (olayları kaydet) → Infer (intent + güven) → Decide (policy sonucu) var.** Ne olduğunu tarif edersin, model intent’i yorumlar, kurallar kararı seçer. Tipler için [API Referansı](api.md), örnekler için [Senaryolar](scenarios.md).

---

## Kimler için?

**Intentum’u şu durumlarda kullan:**

- Akışlar deterministik değil veya AI tabanlı.
- Sabit pass/fail adımları yerine *intent* üzerinden düşünmek istiyorsun.
- Gözlenen davranıştan policy tabanlı kararlar (allow / observe / warn / block) alman gerekiyor.

**Intentum’u atla:**

- Sistem tam deterministik ve gereksinimler sabit.
- Sadece küçük script’ler veya tek seferlik araçlar var; davranış sapması önemsiz.

---

## Doküman içeriği

| Sayfa | Ne bulacaksın |
|-------|----------------|
| [Mimari](architecture.md) | Temel akış (Observe → Infer → Decide), paket yapısı, inference pipeline, persistence/analytics/rate-limiting/multi-tenancy akışları (Mermaid diyagramları). |
| [Kitle ve kullanım örnekleri](audience.md) | Proje tipleri, kullanıcı profilleri, düşük/orta/yüksek örnek test senaryoları (AI ve normal), sektör örnekleri. |
| [Kurulum](setup.md) | Gereksinimler, NuGet kurulumu, ilk proje adımları, env var. |
| [API Referansı](api.md) | Ana tipler (BehaviorSpace, Intent, Policy, sağlayıcılar) ve nasıl uyumlu oldukları. |
| [Sağlayıcılar](providers.md) | OpenAI, Gemini, Mistral, Azure OpenAI, Claude — env var ve DI kurulumu. |
| [Kullanım Senaryoları](scenarios.md) | Örnek akışlar (tekrarlı ödeme, şüpheli tekrarlar, policy sırası). |
| [CodeGen](codegen.md) | CQRS + Intentum proje iskeleti; test assembly veya YAML spec’ten Features üretme. |
| [Test](testing.md) | Birim testleri, coverage, hata senaryoları. |
| [Coverage](coverage.md) | Coverage üretme ve görüntüleme. |
| [Gelişmiş Özellikler](advanced-features.md) | Similarity engine'ler, fluent API'ler, caching, test utilities, rate limiting, analytics & reporting, middleware, observability, persistence. |

---

## Hızlı başlangıç (3 adım)

1. **Çekirdek paketleri yükle**
   ```bash
   dotnet add package Intentum.Core
   dotnet add package Intentum.Runtime
   dotnet add package Intentum.AI
   ```

2. **Sample’ı çalıştır** (API anahtarı gerekmez; mock sağlayıcı kullanır)
   ```bash
   dotnet run --project samples/Intentum.Sample
   ```

3. **Minimal “ilk proje” için [Kurulum](setup.md)** ve ana tipler ile akış için [API Referansı](api.md) oku.

---

## Nasıl yapılır?

- **İlk senaryoyu nasıl çalıştırırım?** — S“Konsol: dotnet run --project samples/Intentum.Sample; API + intent infer + analytics: dotnet run --project samples/Intentum.Sample.Web. Bkz. Kurulum.” mock sağlayıcı kullanır, API anahtarı gerekmez. Örnekler hem klasik (ödeme, giriş, destek, e‑ticaret: sepete ekleme, checkout, ödeme doğrulama) hem ESG (rapor gönderimi, uyumluluk) akışlarını gösterir.
- **Policy nasıl eklenir?** — `IntentPolicy` oluştur, `.AddRule(PolicyRule(...))` ile kuralları **sırayla** ekle (önce Block, sonra Allow). Çıkarımdan sonra `intent.Decide(policy)` çağır. Detay için [Senaryolar](scenarios.md) ve [API Referansı](api.md).
- **Klasik akışları (ödeme, login, destek) nasıl modellersin?** — Olayları `space.Observe(actor, action)` ile kaydet (örn. `"user"`, `"login"`; `"user"`, `"retry"`; `"user"`, `"submit"`). Model davranıştan intent çıkarır; policy Allow/Observe/Warn/Block verir. [Kullanım Senaryoları](scenarios.md) içinde hem klasik (ödeme, e‑ticaret) hem ESG örnekleri var.
- **AI ile senaryo nasıl yazılır?** — Aynı `Observe` akışı Mock veya gerçek sağlayıcı (OpenAI, Gemini vb.) ile çalışır; davranış anahtarlarını anlamlı seç, policy'yi güven + sinyallere dayandır. Detay ve ipuçları: [Senaryolar – AI ile senaryolar](scenarios.md#ai-ile-senaryolar-nasıl-yapılır).

Daha fazla örnek ve kural sıralaması için [Senaryolar](scenarios.md) ve [Kitle ve kullanım örnekleri](audience.md).

---

## Global kullanım notları

- **API anahtarları** — Ortam değişkenleri veya secret manager kullan; anahtarları asla commit etme.
- **Bölge ve gecikme** — Sağlayıcı endpoint konumu ve rate limit’leri dikkate al.
- **Production** — Ham sağlayıcı istek/cevaplarını loglama.

Tam API metod imzaları için [otomatik üretilen API sitesine](https://keremvaris.github.io/Intentum/api/) bak.
