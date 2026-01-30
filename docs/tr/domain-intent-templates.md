# Domain intent şablonları

Greenwashing dışında iki domain için **intent adları** ve **actor:action** (sinyal) setleri: **dolandırıcılık / güvenlik** ve **müşteri niyeti**. "Domain'im için intent'leri nasıl tanımlarım?" sorusuna tekrarlanabilir şekilde yanıt vermek için bu şablonları kullanın.

---

## Dolandırıcılık / güvenlik

**Önerilen intent adları:** `SuspiciousAccess`, `CredentialStuffing`, `AccountRecovery`, `NormalLogin`, `HighRiskSession`.

**Önerilen actor:action (sinyaller):**

| Actor   | Action (örnekler)   | Anlamı |
|--------|----------------------|--------|
| user   | login.failed         | Başarısız giriş denemesi |
| user   | login.retry          | Başarısızlık sonrası tekrar deneme |
| user   | ip.changed           | Oturumda IP değişimi |
| user   | password.reset       | Şifre sıfırlama akışı |
| user   | login.success        | Başarılı giriş |
| user   | device.verified      | Cihaz doğrulaması |
| user   | captcha.passed       | CAPTCHA çözüldü |
| system | velocity.high        | Kısa sürede çok istek |

**Politika fikri:** Güven yüksek ve sinyal sayısı ≥ N (örn. çok başarısız giriş + IP değişimi) ise Block; orta riskte Observe; düşük riskte Allow. Bkz. [examples/fraud-intent](../../examples/fraud-intent/).

---

## Müşteri niyeti

**Önerilen intent adları:** `PurchaseIntent`, `InfoGathering`, `SupportRequest`, `BrowsingOnly`.

**Önerilen actor:action (sinyaller):**

| Actor | Action (örnekler) | Anlamı |
|-------|--------------------|--------|
| user  | browse.category   | Kategori gezintisi |
| user  | cart.add          | Sepete ekleme |
| user  | checkout.start    | Ödemeye başlama |
| user  | payment.submit    | Ödeme gönderme |
| user  | search.product    | Ürün arama |
| user  | view.product      | Ürün görüntüleme |
| user  | compare.product   | Ürün karşılaştırma |
| user  | view.faq           | SSS görüntüleme |
| user  | contact.click      | İletişim tıklaması |
| user  | ticket.create      | Destek talebi oluşturma |
| user  | chat.start         | Sohbet başlatma |

**Politika fikri:** Yüksek güvende Allow; orta güvende Observe; intent'e göre yönlendirme (satın alma → checkout, destek → insan/chat). Bkz. [examples/customer-intent](../../examples/customer-intent/).

---

## Nasıl kullanılır

1. Bir domain seçin (dolandırıcılık, müşteri veya kendi domain'iniz).
2. Intent adlarını tanımlayıp güven eşikleri veya kurallarla eşleyin.
3. Önerilen aksiyonlarla (veya kendi aksiyonlarınızla) `space.Observe(actor, action)` ile olayları kaydedin.
4. Intent modelinizi (örn. `LlmIntentModel` veya kural tabanlı model) ve politikayı çalıştırın; yönlendirme veya engelleme için intent adı + güveni kullanın.

Greenwashing için [Greenwashing tespiti how-to](greenwashing-detection-howto.md) ve [Greenwashing metrikleri](../case-studies/greenwashing-metrics.md) sayfalarına bakın.
