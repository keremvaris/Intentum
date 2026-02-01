# Niyet Modelleri Tasarlamak

**Bu sayfayı neden okuyorsunuz?** Bu sayfa davranıştan niyet nasıl çıkarılacağını ve niyet modellerini nasıl tasarlayacağınızı anlatır: niyet ≠ etiket, sinyaller, güven, policy. İlk intent modelinizi kurarken veya mevcut kuralları niyet odaklı hale getirirken faydalıdır.

Davranıştan anlam nasıl çıkar.

---

## 1. Niyet nedir?

Niyet:

- event değil
- state değil
- kural değil

Niyet, gözlemlenen davranışın arkasındaki *yönelim*.

- **Event'ler** → oldu
- **Davranış** → gözlemlendi
- **Niyet** → çıkarıldı

Niyet hesaplanır; elle tanımlanmaz.

---

## 2. Niyet ≠ Etiket

Bu ayrım çok önemli.

**Yanlış:**

Niyet = `"Fraud"`

**Doğru:**

Niyet = hipotezler üzerinde olasılık dağılımı

Örnek:

- AccountRecovery: 0,62
- SuspiciousAccess: 0,28
- Unknown: 0,10

Intentum'daki **Confidence** kavramı buradan gelir.

---

## 3. Niyet modelleri nasıl çalışır?

Intentum tek bir yöntem dayatmaz.

Üç seviye vardır:

### Seviye 1 — Heuristik niyet modelleri

En basit, en hızlı. Intentum bunun için **RuleBasedIntentModel** sağlar: kurallar listesi verilir (her biri **RuleMatch** veya null döner); ilk eşleşen kazanır. Önce kuralları denemek, güven eşiğin altında LLM fallback için **ChainedIntentModel** kullan. Intent opsiyonel **Reasoning** içerebilir (hangi kural eşleşti veya "Fallback: LLM").

**Artıları:** deterministik, açıklanabilir, hızlı  
**Eksileri:** ölçeklenmez, örüntü çeşitliliği sınırlı

### Seviye 2 — Ağırlıklı sinyal modelleri

Gerçek dünya burada başlar.

- `login.failed` → +0,3 risk
- `captcha.passed` → -0,2 risk
- `device.verified` → -0,4 risk

Confidence: `confidence = Σ(ağırlıklar × sinyaller)`

**Artıları:** esnek, toleranslı, ayarlanabilir  
Intentum'un varsayılan modeli buna yakındır.

### Seviye 3 — AI destekli niyet modelleri

Burada AI devreye girer: embedding, clustering, LLM reasoning, hibrit skorlama.

Ama:

**AI niyet üretmez.**  
**AI niyet *sinyali* üretir.**

Son karar Intentum'dadır. Bu bilinçli bir tasarımdır.

---

## 4. Confidence nedir, ne değildir?

Confidence:

- doğruluk değil
- gerçek değil
- kesin doğru değil

Confidence şu soruya cevap verir:

*"Bu niyet tahminine ne kadar güveniyoruz?"*

Bu yüzden 0,6 bazen yeterlidir, 0,9 bazen şüphelidir.

Intentum'da:

- Yüksek güven + yanlış niyet = 🚨
- Düşük güven + doğru niyet = 🟢

---

## 5. Niyet anti-pattern'leri

**❌ Niyeti sonuçla karıştırmak**  
Niyet = `"Blocked"` — yanlış. Niyet karar öncesidir.

**❌ Niyeti tek event'e bağlamak**  
`if (login.failed) → Fraud` — bu niyet değil, refleks.

**❌ Confidence'ı boolean gibi kullanmak**  
`if (intent.Confidence.Score == 1.0)` — AI çağında bu satır kırmızı bayrak.

---

## 6. Niyet ne zaman "yeterince doğru"?

Intentum'da doğruluk sorusu şudur:

*"Bu niyetle verilen karar, sistem için kabul edilebilir mi?"*

Bu risk toleransına, iş bağlamına, kullanıcıya, zamana göre değişir. Yani niyet bağlamsaldır.

---

## 7. Niyet model yaşam döngüsü

Gözle → Çıkar → Karar ver → Öğren → Ayarla

Intentum bu döngüyü bilinçli olarak açık bırakır: öğrenme dışarıda olabilir, AI sonradan eklenebilir, insan geri bildirimi dahil olabilir.

---

## 8. Intentum'un felsefi sınırı

Intentum niyetin "gerçek" olduğunu iddia etmez, insan zihnini taklit etmez, mutlak doğruluk vaat etmez.

Yaptığı şey: Belirsizlik altında makul karar vermeyi mümkün kılmak.

**Sonraki adım:** Bu sayfayı bitirdiyseniz → [Senaryolar](scenarios.md) veya [Mimari](architecture.md).
