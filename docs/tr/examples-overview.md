# Örnekler Rehberi — Basit, Orta, Zor

**Bu sayfayı neden okuyorsunuz?** Bu sayfa Intentum örneklerini ve sample uygulamalarını basit / orta / zor olarak gruplar; hangi örneğin ne işe yaradığını ve nasıl çalıştırılacağını gösterir. "Hangi örnekle başlamalıyım?" veya "Bu senaryo için hangi proje?" sorularına yanıt arıyorsanız doğru yerdesiniz.

Bu sayfa Intentum **örneklerini** ve **sample** uygulamalarını zorluk derecesine göre gruplar ve gerçek hayat kullanımlarıyla eşleştirir. Tüm örnekler API anahtarı olmadan (Mock sağlayıcı) çalışır; aksi belirtilmedikçe.

---

## Zorluk dereceleri

| Seviye | Açıklama |
|--------|----------|
| **Basit** | Tek kavram, az kod, birkaç dakikada çalıştırılır. "Niyet çıkarımı nedir?" sorusu için uygun. |
| **Orta** | 2–3 kavramı bir arada kullanır (kurallar + LLM, zaman azalması, normalleştirme). Üretim benzeri akışlar için uygun. |
| **Zor** | Tam pipeline, alan odaklı (dolandırıcılık, greenwashing, müşteri yolculuğu) veya çok aşamalı / analitik. |

---

## Basit örnekler

### [hello-intentum](https://github.com/keremvaris/Intentum/tree/master/examples/hello-intentum) — 5 dakikada başla

**Kavram:** Tek sinyal, tek niyet, konsol çıktısı. Bir dakikada çalıştırılabilecek minimal "Hello Intentum".

**Gerçek hayat:** İlk adım: tek davranış gözle (`user:hello`), tek niyet çıkar (`Greeting`), politika uygula (Allow), sonucu yazdır.

```bash
dotnet run --project examples/hello-intentum
```

---

### [vector-normalization](https://github.com/keremvaris/Intentum/tree/master/examples/vector-normalization)

**Kavram:** Davranış vektörü normalleştirme (Cap, L1, SoftCap); tekrarlayan olaylar skoru boğmasın.

**Gerçek hayat:** Bir aksiyon (örn. `user:click`) 100 kez gelince ham sayılar niyeti çarpıtabilir; normalleştirme sinyali dengeler.

```bash
dotnet run --project examples/vector-normalization
```

---

### [time-decay-intent](https://github.com/keremvaris/Intentum/tree/master/examples/time-decay-intent)

**Kavram:** Yakın zamandaki olaylar daha ağır (yarı ömürlü `TimeDecaySimilarityEngine`).

**Gerçek hayat:** Oturum bazlı niyet: "Son 5 dakikada ne yaptı?" bir saat önceki aksiyondan daha önemli.

```bash
dotnet run --project examples/time-decay-intent
```

---

## Orta örnekler

### [chained-intent](https://github.com/keremvaris/Intentum/tree/master/examples/chained-intent)

**Kavram:** Önce kural tabanlı model; güven eşiğin altındaysa LLM’e düş (`ChainedIntentModel`). Maliyeti düşürür, yüksek güvenli durumları deterministik tutar.

**Gerçek hayat:** Trafiğin çoğu net kurallara takılır (örn. "3 başarısız giriş + IP değişti → Şüpheli"); sadece belirsizler LLM’e gider.

```bash
dotnet run --project examples/chained-intent
```

---

### [ai-fallback-intent](https://github.com/keremvaris/Intentum/tree/master/examples/ai-fallback-intent)

**Kavram:** Modelin "acele ettiği" (PrematureClassification) mi yoksa "dikkatli" (CarefulUnderstanding) mi olduğunu çıkar; politika insana veya oto karara yönlendirir.

**Gerçek hayat:** Kalite kapısı: düşük güven veya çelişkili sinyaller → insan incelemesine aktar.

```bash
dotnet run --project examples/ai-fallback-intent
```

---

## Zor örnekler (alan + tam pipeline)

### [fraud-intent](https://github.com/keremvaris/Intentum/tree/master/examples/fraud-intent)

**Kavram:** Dolandırıcılık/istismar niyeti: giriş hataları, IP değişimi, denemeler, captcha, şifre sıfırlama → ŞüpheliErişim vs HesapKurtarma; politika Engelle / İzle / İzin Ver.

**Gerçek hayat:** Giriş akışı: N hata ve IP değişiminden sonra engelleme, ek doğrulama veya izin verme kararı.

```bash
dotnet run --project examples/fraud-intent
```

**Döküman:** [Gerçek dünya senaryoları — Dolandırıcılık](real-world-scenarios.md), [Domain intent şablonları — Fraud](domain-intent-templates.md#fraud--security).

---

### [customer-intent](https://github.com/keremvaris/Intentum/tree/master/examples/customer-intent)

**Kavram:** Müşteri niyeti (satın alma, bilgi toplama, destek): gezinme, sepet, ödeme, arama, SSS, iletişim; politika İzin / İzle / niyete göre yönlendir.

**Gerçek hayat:** E-ticaret veya destek: kullanıcıyı doğru akışa (ödeme, SSS, iletişim) davranıştan yönlendirme.

```bash
dotnet run --project examples/customer-intent
```

**Döküman:** [Domain intent şablonları — Müşteri](domain-intent-templates.md#customer-intent).

---

### [greenwashing-intent](https://github.com/keremvaris/Intentum/tree/master/examples/greenwashing-intent)

**Kavram:** Rapor metni ve sinyallerden greenwashing tespiti; ESG/iddialar için politika. Anlamsal analiz için gerçek sağlayıcı kullanılabilir.

**Gerçek hayat:** Sürdürülebilirlik raporları veya iddialarını analiz et; düşük güven veya yanıltıcı niyeti inceleme için işaretle.

```bash
dotnet run --project examples/greenwashing-intent
```

**Döküman:** [Greenwashing tespiti how-to](greenwashing-detection-howto.md), [Örnek olay — Greenwashing metrikleri](../case-studies/greenwashing-metrics.md).

---

## Sample uygulamalar (tam uygulama)

| Sample | Açıklama | Zorluk |
|--------|----------|--------|
| [Intentum.Sample](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample) | Konsol: tek uygulamada birçok senaryo (ödeme, ESG, uyumluluk, yeniden denemeler). | Orta |
| [Intentum.Sample.Blazor](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample.Blazor) | Blazor UI + CQRS Web API: infer, explain, explain-tree, analytics, timeline, playground compare, greenwashing; Overview, Commerce, Explain, FraudLive, Sustainability, Timeline, PolicyLab, Sandbox; SSE inference, dolandırıcılık ve sürdürülebilirlik simülasyonu. | Zor |

Web sample’ı çalıştırma:

```bash
dotnet run --project samples/Intentum.Sample.Blazor
```

Ardından arayüzü ve Scalar API dokümanını açın; `POST /api/intent/infer`, `POST /api/intent/explain-tree`, `GET /api/intent/analytics/timeline/{entityId}`, `POST /api/intent/playground/compare` deneyin.

Ardından tarayıcıda Overview, Commerce, Explain, FraudLive, Sustainability, Timeline, PolicyLab, Sandbox sayfalarını deneyebilirsiniz.

---

## Hızlı referans

| Örnek | Seviye | Çalıştırma |
|-------|--------|------------|
| vector-normalization | Basit | `dotnet run --project examples/vector-normalization` |
| time-decay-intent | Basit | `dotnet run --project examples/time-decay-intent` |
| chained-intent | Orta | `dotnet run --project examples/chained-intent` |
| ai-fallback-intent | Orta | `dotnet run --project examples/ai-fallback-intent` |
| fraud-intent | Zor | `dotnet run --project examples/fraud-intent` |
| customer-intent | Zor | `dotnet run --project examples/customer-intent` |
| greenwashing-intent | Zor | `dotnet run --project examples/greenwashing-intent` |
| Sample.Blazor | Zor | `dotnet run --project samples/Intentum.Sample.Blazor` |

Yukarıdaki örnekler için API anahtarı gerekmez; Mock sağlayıcı kullanılır. Gerçek AI için ortam değişkenlerini ayarlayıp bir sağlayıcı kullanın — bkz. [Sağlayıcılar](providers.md) ve [AI sağlayıcılarını kullanma](ai-providers-howto.md).

**Sonraki adım:** Bu sayfayı bitirdiyseniz → [Kurulum](setup.md) veya [Senaryolar](scenarios.md).
