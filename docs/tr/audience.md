# Kitle, proje tipleri ve örnek kullanımlar (TR)

Bu sayfa Intentum’un **hangi tip projelerde** kullanıldığını, **kullanıcıların** kimler olduğunu (geliştirici profilleri) ve **düşük, orta ve yüksek** karmaşıklıkta **örnek test senaryolarını** — hem **AI tabanlı** hem **AI’sız (kural tabanlı)** kullanım için — anlatır. Ayrıca **sektör bazlı** örnekler (ESG, Carbon Accounting, Uyumluluk, güvenlik) verir; böylece Intentum’u kendi alanına uyarlayabilirsin.

**Çalıştırılabilir örnekler:** Tüm web tabanlı örnekler (eski Sample Web dahil) **Intentum.Sample.Blazor** projesinde toplanmıştır. Çekirdek akış için [ana sayfa](index.md) ve [API Referansı](api.md). Çalıştırılabilir senaryolar için [Sample (konsol)](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample), [Sample.Blazor](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample.Blazor) ve [Senaryolar](scenarios.md).

---

## Proje tipleri: Intentum nerede kullanılır

| Proje tipi | Tipik kullanım | Neden Intentum |
|------------|----------------|----------------|
| **Güvenlik & dolandırıcılık** | Finans dolandırıcılığı, hesap ele geçirme (ATO), şüpheli transfer | Olay zincirini (login yeni IP, e-posta değişikliği, yüksek transfer, hızlı kart talebi) Observe et; ChainedIntentModel + policy ile Block/Escalate. Blazor’da Demo 1 (Intent Stream) ve ATO Demo. |
| **İçeriden tehdit & anomali** | Veri sızıntısı hazırlığı, normal baz çizgiden sapma | Baz çizgi vs son 48 saat davranışını Observe et; RuleBased intent; RequireAuth/Warn. Blazor’da İçeriden Tehdit Demo. |
| **Sıfır-gün & API trafiği** | Bilinmeyen imza / bilinen davranış, rate limit, bot tespiti | Davranış modeli (PortScan → ExploitAttempt → LateralMove) veya yüksek istek hızı Observe; fallback LLM veya RuleBased; Block/RateLimit. Blazor’da Zero-Day Demo ve API Trafik Demo. |
| **ESG & Sürdürülebilirlik** | ESG raporlama, uyumluluk, greenwashing, karbon metrik | ESG/karbon olaylarını Observe et; intent çıkar; policy Allow / Block / Observe. Blazor’da Sustainability (Greenwashing, Carbon, Live). |
| **Carbon Accounting** | Karbon ayak izi hesaplama, doğrulama, denetim izleri | Karbon hesaplama ve doğrulama olaylarını Observe et; intent çıkar; uyumluluğa göre policy allow / flag / block. |
| **Uyumluluk ve denetim** | ICMA, LMA uyumluluk kontrolleri, denetim izleri, risk bayrakları | Uyumluluk olaylarını Observe et; risk seviyesi çıkar; policy allow / flag / block. |
| **E‑ticaret** | Sepete ekleme, checkout, ödeme doğrulama, tekrarlı denemeler | Sepet/checkout/ödeme olaylarını Observe et; intent çıkar; policy allow / observe / block (aşırı retry’da Block). Blazor’da Commerce sayfası. |
| **Finansal raporlama** | ESG rapor gönderimi, doğrulama retry’ları, çok aktörlü onaylar | Raporlama olayları ve retry’ları Observe et; intent çıkar; policy allow / observe / block. |
| **Düzenleyici iş akışları** | Çok paydaşlı onaylar, uyumluluk doğrulama, risk değerlendirme | İş akışı olaylarını (analist, uyumluluk, düzenleyici, yönetim) Observe et; intent çıkar; policy allow / observe / warn / block. |

---

## Kullanıcı profilleri: Intentum’u kimler kullanır

| Profil | Rol | Tipik ihtiyaç |
|--------|-----|----------------|
| **Backend / full-stack geliştirici** | Observe (olay yakalama), policy kuralları ve AI sağlayıcı entegrasyonunu uygular | Net API (BehaviorSpace, Infer, Decide), sağlayıcı seçenekleri, env config. Blazor sample’da Playground, Explain, Policy Lab, API endpoint’leri. |
| **Güvenlik / fraud** | Dolandırıcılık, ATO, içeriden tehdit, zero-day ve API anomali senaryolarını tanımlar | ChainedIntentModel, RuleBased + LLM fallback, policy Block/Escalate/RateLimit. Blazor’da Intent Stream, ATO Demo, İçeriden Tehdit, Zero-Day, API Trafik demoları. |
| **Ürün / platform** | “Hangi davranış önemli” ve “ne zaman allow / block / observe” tanımlar | Senaryolar ve policy örnekleri; düşük/orta/yüksek örnekler; sektör eşlemesi. |
| **Güvenlik / risk** | Block kuralları, eşikler ve denetim mesajlarını tanımlar | Policy sırası (Block önce), retry/rate limit, denetim mesajları için yerelleştirme. |
| **QA / test** | Ana akışlar için davranış → intent → karar doğrular | Test senaryoları (düşük/orta/yüksek), mock sağlayıcı, contract testler, sektör senaryoları. Blazor’da demolar tek tıkla çalıştırılabilir. |
| **DevOps / SRE** | API anahtarları, bölge, rate limit ile servisleri çalıştırır | Env var, sağlayıcı seçimi, prod’da ham log yok. |

---

## Seviyeye göre örnek test senaryoları

Bunlar Intentum ile uygulayıp test edebileceğin **örnek kullanım senaryoları**. **Karmaşıklığa** (düşük, orta, yüksek) ve **kullanım türüne** (AI tabanlı vs kural tabanlı / normal) göre gruplanmıştır. [Sample (konsol)](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample) ve test projesi bunların bir kısmını çalıştırılabilir senaryo ve birim test olarak uygular; **Sample.Blazor** ise dolandırıcılık, ATO, içeriden tehdit, zero-day, API trafiği, sürdürülebilirlik ve e‑ticaret demolarını tek tıkla çalıştırılabilir sayfalar olarak sunar.

### Düşük karmaşıklık (3–4 örnek)

| # | Ad | Davranış (Observe) | Policy fikri | Beklenen | AI / normal |
|---|-----|--------------------|-------------|----------|-------------|
| 1 | **Karbon ayak izi hesaplama** | `analyst:calculate_carbon` → `system:report_generated` | Güven High ise Allow | Allow | İkisi |
| 2 | **ESG metrik görüntüleme** | `user:view_esg_metric` | Low confidence → Warn | Warn | İkisi |
| 3 | **Sepete ekleme / ürün görüntüleme** | `user:view_product` → `user:add_to_cart` | Güven High ise Allow | Allow veya Observe | İkisi |
| 4 | **Uyumluluk kontrolü** | `compliance:check_icma` → `system:validated` | İlk kural: Low → Warn | Warn | Normal (kural) |

### Orta karmaşıklık (3–4 örnek)

| # | Ad | Davranış (Observe) | Policy fikri | Beklenen | AI / normal |
|---|-----|--------------------|-------------|----------|-------------|
| 1 | **Tekrarlı ESG rapor gönderimi** | prepare_esg_report → retry_validation → retry_validation → report_submitted | High Allow; Medium Observe; retry ≥ 3 ise Block | Allow veya Observe | İkisi |
| 2 | **Karbon doğrulama süreci** | verify_carbon_data → request_correction → submit_correction → approve | Onayda Allow; düzeltmelerde Observe | Allow veya Observe | İkisi |
| 3 | **LMA kredi uyumluluk kontrolü** | check_lma_compliance → flag_issue → resolve → compliance_ok | Uyumluluk sorunlarında Block veya Warn | Block veya Warn | Normal (kural) |
| 4 | **E‑ticaret checkout (tekrarlı)** | cart → checkout → retry → submit | High Allow; aşırı retry’da Block | Allow veya Observe | İkisi |

### Yüksek karmaşıklık (3–4 örnek)

| # | Ad | Davranış (Observe) | Policy fikri | Beklenen | AI / normal |
|---|-----|--------------------|-------------|----------|-------------|
| 1 | **ESG uyumluluk denetim izi** | prepare_esg_report, compliance:review_esg, flag_discrepancy, retry_correction, approve, publish_esg | Uyumluluk riski + aşırı retry’da Block; yoksa High Allow | Block veya Allow | İkisi |
| 2 | **Çok doğrulayıcılı karbon muhasebesi** | calculate_carbon, internal_audit:review, external_verifier:verify, request_changes, update, certify | Embedding’den intent çıkar; güven ve sinyal sayısına göre policy | Allow / Observe / Warn | AI |
| 3 | **Çok paydaşlı ESG risk değerlendirmesi** | assess_esg_risk, risk_committee:review, request_details, provide_details, approve, board:final_approval | Uyumluluk riskinde Block; Medium’da Observe; High’da Allow | Block veya Observe veya Allow | İkisi |
| 4 | **E‑ticaret checkout + ödeme doğrulama** | cart, checkout, payment_attempt, retry, payment_validate, submit | Aşırı retry’da Block; High’da Allow | Block veya Allow veya Observe | İkisi |

---

## Sektör bazlı örnek kullanımlar

| Sektör | Örnek akış | Observe | Infer | Decide |
|--------|------------|--------|-------|--------|
| **ESG** | ESG rapor başarılı akış | prepare_esg_report, compliance:approve, publish_esg | Intent + güven | Allow / Observe |
| **ESG** | Uyumluluk sorunlu ESG raporu | prepare_esg_report, flag_issue, retry_correction×2, approve | Intent + sinyaller | Aşırı retry’da Block; yoksa Allow |
| **Carbon** | Karbon hesaplama başarı | calculate_carbon, validate, record | Intent + güven | Allow |
| **Carbon** | Düzeltmeli karbon doğrulama | calculate_carbon, verify, request_correction, correct, approve | Intent | Allow veya Observe |
| **Uyumluluk** | Uyumluluk kontrolü | check_icma, validated | Risk intent | Warn veya Allow |
| **EU Green Bond** | Draft → InProgress → UnderReview → Approved → Completed | process:Draft, InProgress, UnderReview, Approved, Completed | Intent + sinyaller | Completed’da Allow |
| **EU Green Bond** | Red yolu | process:Draft, InProgress, UnderReview, Rejected | Intent + sinyaller | Rejected’da Block |
| **Klasik (Fintech)** | Ödeme başarılı / tekrarlı | login, retry, submit | Intent + güven | Allow / Observe; aşırı retry’da Block |
| **Klasik (Destek)** | Eskalasyon | user:ask, user:ask, system:escalate | Intent | Warn veya Allow |
| **Klasik (E‑ticaret)** | Sepete ekleme / ürün görüntüleme | view_product, add_to_cart | Intent + güven | Allow veya Observe |
| **Klasik (E‑ticaret)** | Checkout başarılı | cart, checkout, submit | Intent | Allow veya Observe |
| **Klasik (E‑ticaret)** | Checkout tekrarlı / ödeme doğrulama | cart, checkout, retry, payment_validate, submit | Intent + sinyaller | Allow / Observe; aşırı retry’da Block |

---

## İş akışı process status (Draft → InProgress → UnderReview → Approved / Rejected → Completed)

Birçok karmaşık iş akışı (LMA, EU Green Bond, ESG raporlama) **process status** yaşam döngüsü kullanır. Status geçişlerini Observe edersin; model intent çıkarır; policy Allow / Block / Observe verir.

| Status / geçiş | Observe | Policy fikri | Beklenen |
|----------------|--------|--------------|----------|
| **Draft → InProgress** | `process:Draft`, `process:InProgress` | Allow veya Observe (iş devam ediyor) | Allow veya Observe |
| **Draft → InProgress → UnderReview → Approved** | Draft, InProgress, UnderReview, Approved | Approved sinyali varken Allow | Allow |
| **Draft → InProgress → UnderReview → Approved → Completed** | Tam yaşam döngüsü | Completed varken Allow | Allow |
| **Draft → InProgress → UnderReview → Rejected** | Draft, InProgress, UnderReview, Rejected | Rejected varken Block | Block |
| **Draft / InProgress’te takılı** | Sadece Draft, InProgress | Observe (Approved/Rejected/Completed yok) | Observe veya Allow |

Sample ve **WorkflowStatusTests** bu geçişleri ESG raporu, uyumluluk, LMA kredi ve EU Green Bond tarzı iş akışları için içerir.

---

## Sample ve testler buna nasıl karşılık geliyor

Tüm **web tabanlı** örnekler (eski Sample Web’deki sayfalar dahil) **Intentum.Sample.Blazor** projesinde toplanmıştır; ayrı bir “Sample Web” projesi yoktur.

- **Intentum.Sample** (konsol) şunları çalıştırır: ESG/Carbon/EU Green Bond senaryoları; **iş akışı process status** (Draft, InProgress, UnderReview, Approved, Rejected, Completed) EU Green Bond, ESG, uyumluluk, LMA için; ve **klasik** örnekler (ödeme başarılı, tekrarlı ödeme, şüpheli tekrarlar, destek eskalasyonu, e‑ticaret: sepete ekleme, checkout başarılı, checkout tekrarlı, ödeme doğrulama).
- **Intentum.Sample.Blazor** (tek web örneği — Blazor UI + API):
  - **Infer & Decide:** Playground (Sandbox), Explain (sinyal katkıları + politika ağacı), Policy Lab.
  - **Analiz:** Overview, Graph, Timeline, Heatmap, Signals.
  - **Use cases / demolar:** **Intent Stream** (canlı niyet akışı + Demo 1: Finans Dolandırıcılığı, SSE), **ATO Demo** (hesap ele geçirme), **İçeriden Tehdit Demo**, **Zero-Day Demo**, **API Trafik Demo**, **Sustainability** (Greenwashing, Carbon, Live simülasyonu), **Commerce** (e‑ticaret sipariş).
  - **Config:** Settings (dashboard eşikleri vb.).
  - API: intent infer/explain/explain-tree, greenwashing, dolandırıcılık/ATO/zero-day/insider-threat/api-traffic endpoint’leri, SSE inference, Dashboard stream.
  - Çalıştırma: `dotnet run --project samples/Intentum.Sample.Blazor`. Detaylı demo akışları için [Blazor beş demo spesifikasyonu](../case-studies/blazor-five-demo-specs.md).
- **Test projesi** şunlara sahiptir: **LowLevelScenarioTests**, **MediumLevelScenarioTests**, **HighLevelScenarioTests**, **SectorScenarioTests** (ESG, Carbon, uyumluluk + klasik: Fintech, Destek, E‑ticaret), **WorkflowStatusTests** (process status geçişleri), **GreenwashingCaseStudyTests** vb.

Sample’ı çalıştırmak için [Kurulum](setup.md), testleri çalıştırmak için [Test](testing.md). **Sonraki adım:** Bu sayfayı bitirdiyseniz → [Senaryolar](scenarios.md) veya [Örnekler rehberi](examples-overview.md).
