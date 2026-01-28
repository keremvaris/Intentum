# Kullanım Senaryoları (TR)

Intentum Given/When/Then adımları kullanmaz. Bunun yerine olayları **Observe** edersin, **Infer** ile intent ve güven çıkarırsın, policy kurallarıyla **Decide** edersin. Bu sayfa hem **klasik** (ödeme, giriş, destek, e‑ticaret) hem **ESG / uyumluluk** senaryolarını gösterir: hangi davranışı kaydettiğin, policy’yi nasıl tanımladığın ve ne sonuç bekleyeceğin.

Given/When/Then’in yerine ne geldiği için [ana sayfa](index.md#givenwhenthen-yerine-ne-geldi). Tipler ve akış için [API Referansı](api.md).

---

## Klasik senaryolar

### 1) Ödeme akışı ve tekrarlar

**Ne:** Kullanıcı giriş yapar, ödeme denemeleri yapar (bazen retry), sonra işlemi gönderir. Bu normal “ödeme tamamlama” davranışı — şüpheli değil.

**Davranış (Observe):**  
`user:login` → `user:payment_attempt` → `user:retry` → `user:payment_attempt` → `user:submit` (veya benzeri).

**Policy fikri:** Güven High veya Certain ise → Allow. Medium ise → Observe. Retry sayısı çok yüksekse ayrı bir kural ile Block edebilirsin (aşağıdaki “Şüpheli tekrarlar” gibi).

**Kod (minimal):**

```csharp
var space = new BehaviorSpace()
    .Observe("user", "login")
    .Observe("user", "payment_attempt")
    .Observe("user", "retry")
    .Observe("user", "payment_attempt")
    .Observe("user", "submit");

var intent = model.Infer(space);
var policy = new IntentPolicy()
    .AddRule(new PolicyRule("AllowHigh", i => i.Confidence.Level is "High" or "Certain", PolicyDecision.Allow))
    .AddRule(new PolicyRule("ObserveMedium", i => i.Confidence.Level == "Medium", PolicyDecision.Observe));

var decision = intent.Decide(policy);
```

**Beklenen sonuç:** Genelde **Allow** veya **Observe**, çıkarılan güvene göre.

---

### 2) Şüpheli tekrarlar (giriş / ödeme)

**Ne:** Kullanıcı giriş veya ödeme denemelerinde çok fazla tekrar yapar, net bir “submit” veya başarı yok. Bu potansiyel kötüye kullanım veya takılmış akış göstergesi olabilir.

**Davranış (Observe):**  
`user:login` → `user:retry` → `user:retry` → `user:retry` (dört olay; üç retry).

**Policy fikri:** Retry sayısı (örn. “retry” içeren sinyaller) eşiği aştığında (örn. ≥ 3) Block et. Bu kuralı Allow’dan **önce** ekle.

**Kod (minimal):**

```csharp
var space = new BehaviorSpace()
    .Observe("user", "login")
    .Observe("user", "retry")
    .Observe("user", "retry")
    .Observe("user", "retry");

var intent = model.Infer(space);
var policy = new IntentPolicy()
    .AddRule(new PolicyRule(
        "ExcessiveRetryBlock",
        i => i.Signals.Count(s => s.Description.Contains("retry", StringComparison.OrdinalIgnoreCase)) >= 3,
        PolicyDecision.Block))
    .AddRule(new PolicyRule("AllowHigh", i => i.Confidence.Level is "High" or "Certain", PolicyDecision.Allow))
    .AddRule(new PolicyRule("ObserveMedium", i => i.Confidence.Level == "Medium", PolicyDecision.Observe));

var decision = intent.Decide(policy);
```

**Beklenen sonuç:** “Aşırı retry” kuralı eşleştiğinde **Block**; aksi halde güvene göre Allow veya Observe.

---

## E‑ticaret senaryoları

### E‑ticaret: Sepete ekleme ve checkout

**Ne:** Kullanıcı ürün görüntüler, sepete ekler; sonra checkout’a gider ve ödemeyi tamamlar (veya birkaç retry sonrası tamamlar). Bu normal “e‑ticaret alışverişi” davranışı.

**Davranış (Observe):**  
- **Sepete ekleme (düşük):** `user:view_product` → `user:add_to_cart`  
- **Checkout başarılı:** `user:cart` → `user:checkout` → `user:submit`  
- **Checkout tekrarlı:** `user:cart` → `user:checkout` → `user:retry` → `user:submit`  
- **Ödeme doğrulama:** `user:cart` → `user:checkout` → `user:payment_attempt` → `user:retry` → `system:payment_validate` → `user:submit`

**Policy fikri:** Güven High/Certain ise Allow; Medium ise Observe. Retry sayısı çok yüksekse (örn. ≥ 3) Block — ödeme akışındaki “Şüpheli tekrarlar” ile aynı mantık.

**Kod (checkout tekrarlı, minimal):**

```csharp
var space = new BehaviorSpace()
    .Observe("user", "cart")
    .Observe("user", "checkout")
    .Observe("user", "retry")
    .Observe("user", "submit");

var intent = model.Infer(space);
var policy = new IntentPolicy()
    .AddRule(new PolicyRule(
        "ExcessiveRetryBlock",
        i => i.Signals.Count(s => s.Description.Contains("retry", StringComparison.OrdinalIgnoreCase)) >= 3,
        PolicyDecision.Block))
    .AddRule(new PolicyRule("AllowHigh", i => i.Confidence.Level is "High" or "Certain", PolicyDecision.Allow))
    .AddRule(new PolicyRule("ObserveMedium", i => i.Confidence.Level == "Medium", PolicyDecision.Observe));

var decision = intent.Decide(policy);
```

**Beklenen sonuç:** Retry sayısı eşiği aşmıyorsa **Allow** veya **Observe**; aşarsa **Block**. Daha fazla örnek için [Kitle ve kullanım örnekleri](audience.md) sektör tablosundaki “Klasik (E‑ticaret)” satırlarına bak.

---

## ESG ve uyumluluk senaryoları

### 3) Tekrarlı ESG rapor gönderimi

**Ne:** Bir analist ESG raporu hazırlar, doğrulamayı birkaç kez tekrarlar, sonra gönderir. Bu normal “tekrarlı ESG raporlama” davranışı — şüpheli değil.

**Davranış (Observe):**  
`analyst:prepare_esg_report` → `analyst:retry_validation` → `analyst:retry_validation` → `system:report_submitted` (dört olay).

**Policy fikri:** Güven High veya Certain ise → Allow. Medium ise → Observe (izle). İstersen sadece retry sayısı çok yüksekken block eden bir kural da ekleyebilirsin (senaryo 2).

**Kod (minimal):**

```csharp
var space = new BehaviorSpace()
    .Observe("analyst", "prepare_esg_report")
    .Observe("analyst", "retry_validation")
    .Observe("analyst", "retry_validation")
    .Observe("system", "report_submitted");

var intent = model.Infer(space);
var policy = new IntentPolicy()
    .AddRule(new PolicyRule("AllowHigh", i => i.Confidence.Level is "High" or "Certain", PolicyDecision.Allow))
    .AddRule(new PolicyRule("ObserveMedium", i => i.Confidence.Level == "Medium", PolicyDecision.Observe));

var decision = intent.Decide(policy);
```

**Beklenen sonuç:** Genelde **Allow** veya **Observe**, çıkarılan güvene göre. Retry sayısına bağlı kural eklemediğin sürece Block olmaz.

---

### 4) Aşırı tekrarlı ESG raporu

**Ne:** Bir analist ESG raporu hazırlar, sonra net bir “submit” olmadan birçok kez doğrulama tekrarı yapar. Bu uyumluluk sorunları, veri kalitesi problemleri veya takılmış akış göstergesi olabilir.

**Davranış (Observe):**  
`analyst:prepare_esg_report` → `analyst:retry_validation` → `analyst:retry_validation` → `analyst:retry_validation` (dört olay; üç retry).

**Policy fikri:** Retry sayısı (örn. “retry” içeren sinyaller) eşiği aştığında (örn. ≥ 3) Block et. Bu kuralı Allow’dan **önce** ekle ki ikisi de eşleşebileceğinde Block kazansın.

**Kod (minimal):**

```csharp
var space = new BehaviorSpace()
    .Observe("analyst", "prepare_esg_report")
    .Observe("analyst", "retry_validation")
    .Observe("analyst", "retry_validation")
    .Observe("analyst", "retry_validation");

var intent = model.Infer(space);
var policy = new IntentPolicy()
    .AddRule(new PolicyRule(
        "ExcessiveRetryBlock",
        i => i.Signals.Count(s => s.Description.Contains("retry", StringComparison.OrdinalIgnoreCase)) >= 3,
        PolicyDecision.Block))
    .AddRule(new PolicyRule("AllowHigh", i => i.Confidence.Level is "High" or "Certain", PolicyDecision.Allow))
    .AddRule(new PolicyRule("ObserveMedium", i => i.Confidence.Level == "Medium", PolicyDecision.Observe));

var decision = intent.Decide(policy);
```

**Beklenen sonuç:** “Aşırı retry” kuralı eşleştiğinde (örn. ≥ 3 retry) **Block**. Aksi halde güvene göre Allow veya Observe.

---

### 5) ESG uyumluluk denetim izi (analist, uyumluluk ve sistem olayları birlikte)

**Ne:** Analist, uyumluluk ve sistem olayları karışık (örn. prepare_esg_report, compliance:review_esg, flag_discrepancy, retry_correction, approve, publish_esg). Hepsini yine Observe edersin; model tüm davranıştan intent çıkarır; policy karar verir.

**Davranış (Observe):**  
`analyst:prepare_esg_report`, `compliance:review_esg`, `compliance:flag_discrepancy`, `analyst:retry_correction`, `compliance:approve`, `system:publish_esg` (veya benzeri).

**Policy fikri:** Önce uyumluluk riskinde Block, sonra güvene göre Allow/Observe. **Kural sırası önemli:** ilk eşleşen kural kazanır.

**Kod (kural sırası örneği):**

```csharp
var policy = new IntentPolicy()
    .AddRule(new PolicyRule("ComplianceRiskBlock", i => i.Signals.Any(s => s.Description.Contains("compliance", StringComparison.OrdinalIgnoreCase) && i.Confidence.Level == "Low"), PolicyDecision.Block))
    .AddRule(new PolicyRule("AllowHigh", i => i.Confidence.Level is "High" or "Certain", PolicyDecision.Allow))
    .AddRule(new PolicyRule("ObserveMedium", i => i.Confidence.Level == "Medium", PolicyDecision.Observe))
    .AddRule(new PolicyRule("WarnLow", i => i.Confidence.Level == "Low", PolicyDecision.Warn));
```

**Beklenen sonuç:** Koşullarına bağlı. Intent’e (ve isteğe bağlı sinyal sayılarına) göre ilk eşleşen kural kazanır; sonrakiler değerlendirilmez.

---

## AI ile senaryolar nasıl yapılır?

Aynı **Observe** akışını hem **Mock** hem **gerçek AI sağlayıcı** (OpenAI, Gemini vb.) ile kullanabilirsin; senaryo yapın değişmez, sadece embedding kaynağı ve güven skoru değişir.

### Aynı senaryo, iki mod

| Mod | Ne zaman | Ne olur |
|-----|----------|---------|
| **Mock** | CI, birim testi, demo, API anahtarı yok | Davranış anahtarları (örn. `user:login`) hash ile skorlanır; deterministik, tekrarlanabilir. |
| **Gerçek AI** | Production, anlamsal benzerlik istiyorsan | Aynı anahtarlar embedding API ile vektöre çevrilir; anlamı benzer anahtarlar (örn. `login` / `sign_in`) benzer skor alabilir. |

Senaryo kodu aynı kalır: `space.Observe(...)` → `model.Infer(space)` → `intent.Decide(policy)`. Sample'da `OPENAI_API_KEY` **yoksa** Mock, **varsa** OpenAI kullanılır; aynı senaryolar çalışır, güven seviyesi (High/Medium/Low) sağlayıcıya göre değişebilir.

### AI ile senaryo yazma ipuçları

1. **Davranış anahtarlarını anlamlı seç** — `actor:action` formatında tutarlı isimler kullan (örn. `user:login`, `analyst:prepare_esg_report`). Gerçek embedding'ler bu metinlere göre anlamsal benzerlik üretir; tutarlı isimler daha öngörülebilir güven verir.
2. **Policy'yi hem güven hem sinyallere dayandır** — Güven seviyesi (High/Medium/Low) yanında `intent.Signals` ile retry sayısı, belirli anahtar kelimeler (örn. "compliance", "retry") üzerinden kural yazabilirsin; böylece hem AI çıkarımı hem sayılabilir eşikler birlikte kullanılır.
3. **Önce Mock ile doğrula, sonra gerçek AI ile dene** — CI ve testlerde Mock ile deterministik sonuç al; production'da veya "gerçek güven" demosunda aynı senaryoyu gerçek sağlayıcı ile çalıştır. Bkz. [Kurulum – gerçek sağlayıcı](setup.md#gerçek-sağlayıcı-kullanımı-örn-openai) ve [Sağlayıcılar](providers.md).

### Örnek: aynı davranış, Mock vs OpenAI

Aynı davranışı önce Mock, sonra (ortam değişkeniyle) OpenAI ile çalıştırmak için senaryo kodu değişmez; sadece uygulama başlarken kullanılan embedding sağlayıcı değişir (sample'da `OPENAI_API_KEY` ile otomatik). Örnek akış:

```csharp
// Aynı senaryo — sağlayıcı Mock veya OpenAI olabilir
var space = new BehaviorSpace()
    .Observe("user", "login")
    .Observe("user", "payment_attempt")
    .Observe("user", "submit");

var intent = model.Infer(space);  // Mock: deterministik skor; OpenAI: anlamsal skor
var decision = intent.Decide(policy);
```

Mock ile belirli bir güven (örn. Medium) alırken, aynı olaylarla OpenAI farklı bir güven (örn. High) verebilir; policy kuralları aynı kaldığı için karar (Allow/Observe/Warn/Block) buna göre değişir. Yeni senaryo eklemek için yukarıdaki klasik ve ESG örneklerindeki gibi `Observe` zincirini ve policy'yi genişletmen yeterli.

---

## Policy kural sıralaması

Intentum kuralları ekleme sırasına göre değerlendirir. **İlk eşleşen kural kazanır.** Yani:

- **Block** (ve katı kuralları) belirli davranışları güvenden bağımsız engellemek istiyorsan **önce** ekle.
- **Allow** (örn. yüksek güven) **Block’tan sonra** ekle ki normal akışlar sadece engellenmediğinde izin alsın.
- **Observe** / **Warn**’i araya veya sonraya ihtiyaca göre ekle.

**Örnek sıra:**

```csharp
var policy = new IntentPolicy()
    .AddRule(new PolicyRule("ExcessiveRetryBlock", i => RetryCount(i) >= 3, PolicyDecision.Block))
    .AddRule(new PolicyRule("AllowHigh", i => i.Confidence.Level is "High" or "Certain", PolicyDecision.Allow))
    .AddRule(new PolicyRule("ObserveMedium", i => i.Confidence.Level == "Medium", PolicyDecision.Observe))
    .AddRule(new PolicyRule("WarnLow", i => i.Confidence.Level == "Low", PolicyDecision.Warn));
```

Birden fazla senaryoyla tam çalışan örnek için [sample](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample).
