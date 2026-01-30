# Gerçek dünya senaryoları

Intentum canlıda: dolandırıcılık/kötüye kullanım tespiti ve AI karar yedeklemesi.

---

## Use case 1: Dolandırıcılık / Kötüye kullanım niyet tespiti

### Sorun (klasik yaklaşım)

Klasik sistemler şöyle çalışır:

```
IF too_many_failures AND unusual_ip AND velocity_high
THEN block_user
```

Sorunlar:

- False positive (yanlış alarm)
- Akış değişince kurallar çöker
- AI sinyalleri senaryoya sığmaz
- "Şüpheli ama masum" durumlar patlar

### Intentum’un sorduğu soru

Bu kullanıcı gerçekten dolandırıcılık mı peşinde, yoksa sadece başarısız bir girişim mi yaşıyor?

Cevap evet-hayır değil.

### 1. Gözlemlenen davranış uzayı

```csharp
var space = new BehaviorSpace()
    .Observe("user", "login.failed")
    .Observe("user", "login.failed")
    .Observe("user", "ip.changed")
    .Observe("user", "login.retry")
    .Observe("user", "captcha.passed");
```

Not: Sıra kritik değil; tekrarlar bilgi taşır; "başarılı" event’ler de sinyal.

### 2. Niyet çıkarımı

```csharp
var intentModel = new LlmIntentModel(
    new MockEmbeddingProvider(),
    new SimpleAverageSimilarityEngine());
var intent = intentModel.Infer(space);
```

Model şunu yapar:

- "başarısız giriş" → negatif sinyal
- "captcha geçti" → pozitif sinyal
- "ip değişti" → riski artırır ama tek başına kanıt değil

### 3. Risk niyetine göre doğrula

Özel niyet modeli (örn. SuspiciousAccess, AccountRecovery) kullanıyorsanız:

```csharp
// intent.Name == "SuspiciousAccess"
// intent.Confidence.Score yaklaşık 0.4–0.7 aralığında
```

Anlamı: "Bu kullanıcı muhtemelen riskli, ama kesin dolandırıcı değil."

BDD’de test geçer ya kalır. Intentum’da test kararı *besler*.

### 4. Farklı davranış, aynı niyet

```csharp
var space2 = new BehaviorSpace()
    .Observe("user", "login.failed")
    .Observe("user", "password.reset")
    .Observe("user", "login.success")
    .Observe("user", "device.verified");
```

Sonuç (uygun modelle): niyet adı "AccountRecovery", güven ~0.8.

Akış değişti, niyet değişti; sistem kırılmadı.

### 5. Karar katmanı (kritik)

Intentum bloklamaz; kararı besler.

```csharp
if (intent.Name == "SuspiciousAccess" && intent.Confidence.Score > 0.65)
{
    StepUpAuth();
}
else if (intent.Confidence.Score < 0.4)
{
    Allow();
}
else
{
    Monitor();
}
```

Niyet doğruluğu ≠ aksiyon doğruluğu. Bu ayrımı net tutuyorsun.

### 6. Neden BDD ile zor?

BDD’de şunlar için ayrı senaryolar gerekir:

- yeni IP’den iki kez başarısız giriş
- captcha’lı üç deneme
- farklı cihazdan şifre sıfırlama
- sıfırlamadan sonra tekrar giriş
- IP değişiminden sonra captcha başarısı
- …

Kombinasyon patlaması. Bakımı zor. AI sinyalleri sabit senaryoya sığmaz.

Intentum’da: tek model, geniş davranış uzayı, güvene dayalı karar.

### 7. Özet

Bu örnek Intentum’un test çerçevesi değil; karar *öncesi* bir *anlam katmanı* olduğunu gösteriyor.

---

## Use case 2: AI karar yedeklemesi ve doğrulama

Model kendinden emin… ama yanlış.

### Sorun (gerçek dünya)

Bir LLM/ML modeli:

- bazen yanlış anlar
- bazen halüsinasyon görür
- bazen aşırı kendinden emin olur

Klasik sistem: "Model cevap verdi → kabul et."

BDD ile test: sabit prompt, beklenen string, en ufak sapma = fail. AI gerçeğiyle uyuşmaz.

### Intentum’un sorduğu soru

Model bu kararı verirken *ne yapmaya çalışıyordu*? Bu davranış beklenen niyetle tutarlı mı?

Bu soru modelden bağımsız. GPT, Claude, yerel LLM — aynı mantık.

### Senaryo: AI destekli karar motoru

Örnek: Bir AI kullanıcı taleplerini şöyle sınıflandırıyor:

- İade
- Teknik destek
- Hesap sorunu
- Kötüye kullanım girişimi

### 1. Gözlemlenen AI davranış uzayı

Burada "davranış" *modelin* davranışı, kullanıcının değil.

```csharp
var space = new BehaviorSpace()
    .Observe("llm", "high_confidence")
    .Observe("llm", "short_reasoning")
    .Observe("llm", "no_followup_question")
    .Observe("user", "rephrased_request")
    .Observe("llm", "changed_answer");
```

Bu sinyaller şunu anlatıyor: model hızlı karar verdi, zayıf açıklama yaptı, kullanıcı memnun olmadı, model geri adım attı.

### 2. AI niyetini çıkar

```csharp
var intent = intentModel.Infer(space);
```

Sonuç (uygun modelle): niyet adı örn. "PrematureClassification", güven 0.78.

Anlamı: "Model hızlı cevap vermeye çalışmış, dikkatli sınıflandırmaya değil."

BDD bunu yakalayamaz.

### 3. Niyete dayalı yedekleme

```csharp
if (intent.Name == "PrematureClassification" && intent.Confidence.Score > 0.7)
{
    RouteToHuman();
    ReduceModelTrust();
}
```

Önemli: Modelin "yanlış" olduğunu söylemiyorsun; *niyetine* göre hareket ediyorsun. Güveni ayarlıyorsun. Bu, canlıda kullanılabilecek AI davranışı.

### 4. Farklı davranış, sağlıklı niyet

```csharp
var space2 = new BehaviorSpace()
    .Observe("llm", "asked_clarifying_question")
    .Observe("user", "provided_details")
    .Observe("llm", "reasoning_explicit")
    .Observe("llm", "moderate_confidence");
```

Sonuç: niyet "CarefulUnderstanding", güven ~0.85.

Karar: `AllowAutoDecision();`

### 5. Kritik fark

BDD / unit test sorar: "Çıktı doğru mu?"

Intentum sorar: "Bu çıktı nasıl ve neden üretildi?"

AI sistemlerinde *neden* çoğu zaman ham çıktıdan daha değerli.

### 6. Intentum burada ne sağlıyor?

- Model sürümü değişimi
- Prompt değişimi
- Sağlayıcı değişimi
- Temperature / sampling değişimi

Niyet hizalı kaldığı sürece hiçbiri testleri kırmak zorunda değil.

### 7. AI mühendisleri için mesaj

"Bunu canlıda zor yoldan çözdük." — Intentum tam bu boşluk için.

### Intentum’un gerçek yeri

Intentum:

- **değildir:** test çerçevesi, prompt aracı, AI sarmalayıcı

Intentum:

- **AI akıl yürütme doğrulama katmanı**
- **karar güven motoru**
- **niyet odaklı güvenlik ağı**

---

## Kullanım senaryosu 3: Zincirli intent (Kural → LLM fallback)

Önce kuralları dene; kural eşleşmezse veya güven eşiğin altındaysa LLM çağır. Maliyet ve gecikmeyi azaltır; yüksek güvenli kural eşleşmelerini deterministik ve açıklanabilir tutar.

### Fikir

1. **Birincil model** — RuleBasedIntentModel: örn. "login.failed >= 2 ve password.reset ve login.success" → AccountRecovery (0.85).
2. **Fallback model** — Hiç kural eşleşmezse veya birincil güven eşiğin altındaysa LlmIntentModel.
3. **ChainedIntentModel** — `ChainedIntentModel(primary, fallback, confidenceThreshold: 0.7)`.

### Kod

```csharp
var rules = new List<Func<BehaviorSpace, RuleMatch?>>
{
    space =>
    {
        var loginFails = space.Events.Count(e => e.Action == "login.failed");
        var hasReset = space.Events.Any(e => e.Action == "password.reset");
        var hasSuccess = space.Events.Any(e => e.Action == "login.success");
        if (loginFails >= 2 && hasReset && hasSuccess)
            return new RuleMatch("AccountRecovery", 0.85, "login.failed>=2 and password.reset and login.success");
        return null;
    }
};

var primary = new RuleBasedIntentModel(rules);
var fallback = new LlmIntentModel(embeddingProvider, new SimpleAverageSimilarityEngine());
var chained = new ChainedIntentModel(primary, fallback, confidenceThreshold: 0.7);

var intent = chained.Infer(space);
// intent.Reasoning: "Primary: ..." veya "Fallback: LLM (primary confidence below 0.7)"
```

### Açıklanabilirlik

Her intent **Reasoning** içerir (hangi kural eşleşti veya fallback kullanıldı). Log, debug ve "neden bu karar?" için kullanılır.

Çalışan örnek: [examples/chained-intent](https://github.com/keremvaris/Intentum/tree/master/examples/chained-intent).

---

## Çalıştırılabilir örnekler

Bu senaryolar için minimal çalıştırılabilir projeler:

- [examples/fraud-intent](https://github.com/keremvaris/Intentum/tree/master/examples/fraud-intent) — Dolandırıcılık / kötüye kullanım intent, policy → StepUpAuth / Allow / Monitor
- [examples/customer-intent](https://github.com/keremvaris/Intentum/tree/master/examples/customer-intent) — Müşteri intent (satın alma, destek), policy → Allow / Observe / intent'e göre yönlendirme
- [examples/greenwashing-intent](https://github.com/keremvaris/Intentum/tree/master/examples/greenwashing-intent) — Rapor metninden greenwashing tespiti
- [examples/ai-fallback-intent](https://github.com/keremvaris/Intentum/tree/master/examples/ai-fallback-intent) — AI karar yedeklemesi (PrematureClassification / CarefulUnderstanding)
- [examples/chained-intent](https://github.com/keremvaris/Intentum/tree/master/examples/chained-intent) — Kural → LLM fallback
- [examples/time-decay-intent](https://github.com/keremvaris/Intentum/tree/master/examples/time-decay-intent) — Yakın event'ler daha ağır
- [examples/vector-normalization](https://github.com/keremvaris/Intentum/tree/master/examples/vector-normalization) — Cap, L1, SoftCap

**Web örneği (dolandırıcılık + greenwashing + explain):** [samples/Intentum.Sample.Web](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample.Web) — tam UI ve HTTP API: intent infer/explain, greenwashing tespiti (çok dilli, opsiyonel görsel, Scope 3/blockchain mock), analytics ve son analizlerle Dashboard. Bkz. [Greenwashing tespiti (how-to)](greenwashing-detection-howto.md#6-örnek-uygulama-intentumsampleweb) ve [Kurulum – Web örneği](setup.md#repo-sampleını-derle-ve-çalıştır).

Konsol örneklerini nasıl çalıştıracağınız için repodaki [examples README](https://github.com/keremvaris/Intentum/tree/master/examples) sayfasına bakın.

Ödeme, destek, ESG gibi daha fazla akış için [Senaryolar](scenarios.md) ve [Kitle ve kullanım alanları](audience.md) sayfalarına bakın.
