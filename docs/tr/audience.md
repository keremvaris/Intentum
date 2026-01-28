# Kitle, proje tipleri ve örnek kullanımlar (TR)

Bu sayfa Intentum’un **hangi tip projelerde** kullanıldığını, **kullanıcıların** kimler olduğunu (geliştirici profilleri) ve **düşük, orta ve yüksek** karmaşıklıkta **örnek test senaryolarını** — hem **AI tabanlı** hem **AI’sız (kural tabanlı)** kullanım için — anlatır. Ayrıca **sektör bazlı** örnekler (ESG, Carbon Accounting, Sukuk & İslami Finans, Uyumluluk) verir; böylece Intentum’u kendi alanına uyarlayabilirsin.

Çekirdek akış (Observe → Infer → Decide) için [ana sayfa](index.md) ve [API Referansı](api.md). Çalıştırılabilir senaryolar için [sample](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample) ve [Senaryolar](scenarios.md).

---

## Proje tipleri: Intentum nerede kullanılır

| Proje tipi | Tipik kullanım | Neden Intentum |
|------------|----------------|----------------|
| **ESG & Sürdürülebilirlik** | ESG raporlama, uyumluluk, metrik takibi, risk değerlendirme | ESG olaylarını (rapor hazırlama, uyumluluk incelemesi, doğrulama) Observe et; intent çıkar; policy Allow / Block / Observe ver. Deterministik değil (çok paydaşlı, uyumluluk kontrolleri). |
| **Carbon Accounting** | Karbon ayak izi hesaplama, doğrulama, denetim izleri | Karbon hesaplama ve doğrulama olaylarını Observe et; intent çıkar; uyumluluğa göre policy allow / flag / block. |
| **Sukuk & İslami Finans** | Sukuk ihracı, şeriat incelemesi, ICMA uyumluluğu, düzenleyici onay | İhrac akışını (şeriat incelemesi, düzenleyici kontroller, ICMA uyumluluğu) Observe et; intent çıkar; policy allow / block / observe. |
| **Uyumluluk ve denetim** | ICMA, LMA uyumluluk kontrolleri, denetim izleri, risk bayrakları | Uyumluluk olaylarını Observe et; risk seviyesi çıkar; policy allow / flag / block. |
| **Finansal raporlama** | ESG rapor gönderimi, doğrulama retry’ları, çok aktörlü onaylar | Raporlama olayları ve retry’ları Observe et; intent çıkar; policy allow / observe / block. |
| **Düzenleyici iş akışları** | Çok paydaşlı onaylar, uyumluluk doğrulama, risk değerlendirme | İş akışı olaylarını (analist, uyumluluk, düzenleyici, yönetim) Observe et; intent çıkar; policy allow / observe / warn / block. |

---

## Kullanıcı profilleri: Intentum’u kimler kullanır

| Profil | Rol | Tipik ihtiyaç |
|--------|-----|----------------|
| **Backend / full-stack geliştirici** | Observe (olay yakalama), policy kuralları ve AI sağlayıcı entegrasyonunu uygular | Net API (BehaviorSpace, Infer, Decide), sağlayıcı seçenekleri, env config. |
| **Ürün / platform** | “Hangi davranış önemli” ve “ne zaman allow / block / observe” tanımlar | Senaryolar ve policy örnekleri; düşük/orta/yüksek örnekler; sektör eşlemesi. |
| **Güvenlik / risk** | Block kuralları, eşikler ve denetim mesajlarını tanımlar | Policy sırası (Block önce), retry/rate limit, denetim mesajları için yerelleştirme. |
| **QA / test** | Ana akışlar için davranış → intent → karar doğrular | Test senaryoları (düşük/orta/yüksek), mock sağlayıcı, contract testler, sektör senaryoları. |
| **DevOps / SRE** | API anahtarları, bölge, rate limit ile servisleri çalıştırır | Env var, sağlayıcı seçimi, prod’da ham log yok. |

---

## Seviyeye göre örnek test senaryoları

Bunlar Intentum ile uygulayıp test edebileceğin **örnek kullanım senaryoları**. **Karmaşıklığa** (düşük, orta, yüksek) ve **kullanım türüne** (AI tabanlı vs kural tabanlı / normal) göre gruplanmıştır. [Sample](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample) ve test projesi bunların bir kısmını çalıştırılabilir senaryo ve birim test olarak uygular.

### Düşük karmaşıklık (3–4 örnek)

| # | Ad | Davranış (Observe) | Policy fikri | Beklenen | AI / normal |
|---|-----|--------------------|-------------|----------|-------------|
| 1 | **Karbon ayak izi hesaplama** | `analyst:calculate_carbon` → `system:report_generated` | Güven High ise Allow | Allow | İkisi |
| 2 | **ESG metrik görüntüleme** | `user:view_esg_metric` | Low confidence → Warn | Warn | İkisi |
| 3 | **Sukuk ihrac sorgusu** | `investor:inquire_sukuk` → `system:provide_details` | High Allow; Medium Observe | Allow veya Observe | İkisi |
| 4 | **ICMA uyumluluk kontrolü** | `compliance:check_icma` → `system:validated` | İlk kural: Low → Warn | Warn | Normal (kural) |

### Orta karmaşıklık (3–4 örnek)

| # | Ad | Davranış (Observe) | Policy fikri | Beklenen | AI / normal |
|---|-----|--------------------|-------------|----------|-------------|
| 1 | **Tekrarlı ESG rapor gönderimi** | prepare_esg_report → retry_validation → retry_validation → report_submitted | High Allow; Medium Observe; retry ≥ 3 ise Block | Allow veya Observe | İkisi |
| 2 | **Karbon doğrulama süreci** | verify_carbon_data → request_correction → submit_correction → approve | Onayda Allow; düzeltmelerde Observe | Allow veya Observe | İkisi |
| 3 | **Onaylı sukuk ihracı** | initiate_sukuk → sharia:review → regulator:approve → issue_sukuk | High Allow; Medium Observe; uyumluluk riskinde Block | Allow veya Observe | İkisi |
| 4 | **LMA kredi uyumluluk kontrolü** | check_lma_compliance → flag_issue → resolve → compliance_ok | Uyumluluk sorunlarında Block veya Warn | Block veya Warn | Normal (kural) |

### Yüksek karmaşıklık (3–4 örnek)

| # | Ad | Davranış (Observe) | Policy fikri | Beklenen | AI / normal |
|---|-----|--------------------|-------------|----------|-------------|
| 1 | **ESG uyumluluk denetim izi** | prepare_esg_report, compliance:review_esg, flag_discrepancy, retry_correction, approve, publish_esg | Uyumluluk riski + aşırı retry’da Block; yoksa High Allow | Block veya Allow | İkisi |
| 2 | **Çok doğrulayıcılı karbon muhasebesi** | calculate_carbon, internal_audit:review, external_verifier:verify, request_changes, update, certify | Embedding’den intent çıkar; güven ve sinyal sayısına göre policy | Allow / Observe / Warn | AI |
| 3 | **Şeriat ve düzenleyici incelemeli sukuk ihracı** | initiate_sukuk, sharia:review, request_amendment, amend, regulator:review, approve, issue_sukuk | Uyumluluk riskinde Block; High güvende Allow | Block veya Allow | İkisi |
| 4 | **Çok paydaşlı ESG risk değerlendirmesi** | assess_esg_risk, risk_committee:review, request_details, provide_details, approve, board:final_approval | Uyumluluk riskinde Block; Medium’da Observe; High’da Allow | Block veya Observe veya Allow | İkisi |

---

## Sektör bazlı örnek kullanımlar

| Sektör | Örnek akış | Observe | Infer | Decide |
|--------|------------|--------|-------|--------|
| **ESG** | ESG rapor başarılı akış | prepare_esg_report, compliance:approve, publish_esg | Intent + güven | Allow / Observe |
| **ESG** | Uyumluluk sorunlu ESG raporu | prepare_esg_report, flag_issue, retry_correction×2, approve | Intent + sinyaller | Aşırı retry’da Block; yoksa Allow |
| **Carbon** | Karbon hesaplama başarı | calculate_carbon, validate, record | Intent + güven | Allow |
| **Carbon** | Düzeltmeli karbon doğrulama | calculate_carbon, verify, request_correction, correct, approve | Intent | Allow veya Observe |
| **Sukuk** | Sukuk ihrac sorgusu | inquire_sukuk, provide_details | Intent | Allow |
| **Sukuk** | Şeriat incelemeli sukuk | initiate_sukuk, sharia:review, approve, issue_sukuk | Intent + güven | Allow veya Observe |
| **Sukuk** | ICMA uyumluluğu ile sukuk | initiate_sukuk, sharia:review, icma:check_compliance, request_adjustment, adjust, approve, issue_sukuk | Intent + sinyaller | Uyumluluk riskinde Block; yoksa Allow |
| **Uyumluluk** | ICMA uyumluluk kontrolü | check_icma, validated | Risk intent | Warn veya Allow |
| **EU Green Bond** | Draft → InProgress → UnderReview → Approved → Completed | process:Draft, InProgress, UnderReview, Approved, Completed | Intent + sinyaller | Completed’da Allow |
| **EU Green Bond** | Red yolu | process:Draft, InProgress, UnderReview, Rejected | Intent + sinyaller | Rejected’da Block |
| **Klasik (Fintech)** | Ödeme başarılı / tekrarlı | login, retry, submit | Intent + güven | Allow / Observe; aşırı retry’da Block |
| **Klasik (Destek)** | Eskalasyon | user:ask, user:ask, system:escalate | Intent | Warn veya Allow |
| **Klasik (E‑ticaret)** | Checkout başarılı / tekrarlı | cart, checkout, retry, submit | Intent | Allow / Observe; aşırı retry’da Block |

---

## İş akışı process status (Draft → InProgress → UnderReview → Approved / Rejected → Completed)

Birçok karmaşık iş akışı (ICMA, LMA, Sukuk, EU Green Bond, ESG raporlama) **process status** yaşam döngüsü kullanır. Status geçişlerini Observe edersin; model intent çıkarır; policy Allow / Block / Observe verir.

| Status / geçiş | Observe | Policy fikri | Beklenen |
|----------------|--------|--------------|----------|
| **Draft → InProgress** | `process:Draft`, `process:InProgress` | Allow veya Observe (iş devam ediyor) | Allow veya Observe |
| **Draft → InProgress → UnderReview → Approved** | Draft, InProgress, UnderReview, Approved | Approved sinyali varken Allow | Allow |
| **Draft → InProgress → UnderReview → Approved → Completed** | Tam yaşam döngüsü | Completed varken Allow | Allow |
| **Draft → InProgress → UnderReview → Rejected** | Draft, InProgress, UnderReview, Rejected | Rejected varken Block | Block |
| **Draft / InProgress’te takılı** | Sadece Draft, InProgress | Observe (Approved/Rejected/Completed yok) | Observe veya Allow |

Sample ve **WorkflowStatusTests** bu geçişleri ESG raporu, Sukuk ihracı, ICMA uyumluluğu, LMA kredi ve EU Green Bond tarzı iş akışları için içerir.

---

## Sample ve testler buna nasıl karşılık geliyor

- **Sample projesi** şunları çalıştırır: ESG/Carbon/Sukuk/EU Green Bond senaryoları; **iş akışı process status** (Draft, InProgress, UnderReview, Approved, Rejected, Completed) EU Green Bond, ESG, Sukuk, ICMA, LMA için; ve **klasik** örnekler (ödeme başarılı, tekrarlı ödeme, şüpheli tekrarlar, destek eskalasyonu, e‑ticaret checkout).
- **Test projesi** şunlara sahiptir: **LowLevelScenarioTests**, **MediumLevelScenarioTests**, **HighLevelScenarioTests**, **SectorScenarioTests** (ESG, Carbon, Sukuk, ICMA + klasik: Fintech, Destek, E‑ticaret), **WorkflowStatusTests** (process status geçişleri: Draft→InProgress, tam yaşam döngüsü Approved/Completed, Rejected yolu, takılı Draft/InProgress, EU Green Bond tarzı).

Sample’ı çalıştırmak için [Kurulum](setup.md), testleri çalıştırmak için [Test](testing.md).
