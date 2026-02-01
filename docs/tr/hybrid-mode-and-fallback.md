# Hibrit mod ve kural tabanlı yedekleme

**Bu sayfayı neden okuyorsunuz?** Bu sayfa hibrit niyet çıkarımını (kural tabanlı + LLM/ONNX yedek) anlatır: yüksek güvenli durumlar hızlı kalır, belirsiz durumlarda yedek model kullanılır. Maliyet ve güven dengesi arıyorsanız doğru yerdesiniz.

Bu sayfa **hibrit niyet çıkarımını** anlatır: Kural tabanlı mantığı AI (LLM veya yerel ONNX) ile birleştirerek yüksek güvenli durumların hızlı ve ucuz kalması, belirsiz durumlarda ise yedek modelin kullanılması. En iyi uygulamalar özetlenir.

---

## Hibrit mod nedir?

**Hibrit mod**, tek bir çıkarım yolunda birden fazla niyet modeli kullanmaktır:

1. **Birincil model** — Önce denenir (genelde kural tabanlı veya hızlı yerel model).
2. **Yedek model** — Birincil sonuç güven eşiğinin altındayken (veya başarısız olunca) kullanılır.

Faydalar:

- **Maliyet:** Yüksek güvenli kural eşleşmeleri LLM/API çağrısı gerektirmez.
- **Gecikme:** Kurallar hızlıdır; LLM yalnızca gerektiğinde kullanılır.
- **Determinizm:** Kural eşleşen niyetler tekrarlanabilir; yedek yalnızca belirsizken devreye girer.
- **Açıklanabilirlik:** Intent **Reasoning** alanı sonucun birincil mi yedek mi olduğunu gösterir.

---

## ChainedIntentModel (kural → LLM yedek)

**ChainedIntentModel** önce birincil modeli dener; güven eşiğin altındaysa ikincil modeli çağırır.

### Tipik kurulum: önce kurallar, LLM yedek

```csharp
var rules = new List<Func<BehaviorSpace, RuleMatch?>>
{
    space =>
    {
        var loginFails = space.Events.Count(e => e.Action == "login.failed");
        if (loginFails >= 2)
            return new RuleMatch("SuspiciousAccess", 0.85, "login.failed>=2");
        return null;
    }
};

var primary = new RuleBasedIntentModel(rules);
var fallback = new LlmIntentModel(embeddingProvider, new SimpleAverageSimilarityEngine());
var chained = new ChainedIntentModel(primary, fallback, confidenceThreshold: 0.7);

var intent = chained.Infer(space);
// intent.Reasoning: "Primary: login.failed>=2" veya "Fallback: LLM (primary confidence below 0.7)"
```

### Güven eşiği seçimi

- **Yüksek (örn. 0.8):** Daha fazla istek yedeğe gider; belirsiz durumlar için daha güvenli, maliyet artar.
- **Düşük (örn. 0.5):** Daha az yedek; maliyet düşer, ancak LLM tercih edilebilecekken kural sonucu kullanılma ihtimali artar.

A/B testleri veya değerlendirme verisi ile ayarlayın (bkz. [IntentExperiment](api.md) ve [examples/ai-fallback-intent](https://github.com/keremvaris/Intentum/tree/master/examples/ai-fallback-intent)).

---

## Kural → yerel ONNX yedek

Düşük gecikme ve harici API istemiyorsanız yedek olarak yerel sınıflandırıcı kullanın:

```csharp
var primary = new RuleBasedIntentModel(rules);
var onnxOptions = new OnnxIntentModelOptions(
    ModelPath: "path/to/intent_classifier.onnx",
    IntentLabels: ["IntentA", "IntentB", "Unknown"]);
using var fallback = new OnnxIntentModel(onnxOptions);
var chained = new ChainedIntentModel(primary, fallback, confidenceThreshold: 0.7);
```

Model formatı (girdi/çıktı şekilleri ve intent etiketleri) için [Intentum.AI.ONNX](https://www.nuget.org/packages/Intentum.AI.ONNX) dokümantasyonuna bakın.

---

## MultiStageIntentModel

Pipeline’a (sinyal → vektör → niyet → güven) tam kontrol istediğinizde **MultiStageIntentModel** kullanın. Özel aşamalar (örn. özel vektörleştirici veya güven hesaplayıcı) için uygundur. “Önce kural, sonra yedek” için **ChainedIntentModel** daha basittir ve önerilir.

---

## En iyi uygulamalar

| Amaç | Öneri |
|------|--------|
| **Maliyet** | **ChainedIntentModel** ile önce kurallar; eşiği trafiğin çoğunun kurallarda kalacak şekilde ayarlayın. |
| **Gecikme** | Birincil olarak kural tabanlı kullanın; mümkünse LLM yerine ONNX yedek kullanın. |
| **Açıklanabilirlik** | **Reasoning** alanını kullanın (Intentum "Primary: …" veya "Fallback: …" yazar); denetim için loglayın. |
| **Hata yönetimi** | `model.Infer(space)` çağrısını try/catch ile sarın; API hatasında yedek intent dönün veya önbellek sonucu kullanın. Bkz. [Production readiness](production-readiness.md) ve [Embedding API errors](embedding-api-errors.md). |
| **Test** | Kuralları izole unit test edin; eşik davranışı için ChainedIntentModel’i mock yedekle test edin. Bkz. [examples/chained-intent](https://github.com/keremvaris/Intentum/tree/master/examples/chained-intent) ve [examples/ai-fallback-intent](https://github.com/keremvaris/Intentum/tree/master/examples/ai-fallback-intent). |

---

## İlgili

- [Gelişmiş özellikler](advanced-features.md) — RuleBasedIntentModel, ChainedIntentModel, fluent API
- [Gerçek dünya senaryoları](real-world-scenarios.md) — Zincirli intent (kural → LLM yedek)
- [Production readiness](production-readiness.md) — Yedekleme ve hata yönetimi
- [API özeti](api.md) — ChainedIntentModel, OnnxIntentModel, Intent.Reasoning

**Sonraki adım:** Bu sayfayı bitirdiyseniz → [Gerçek dünya senaryoları](real-world-scenarios.md) veya [Üretim hazırlığı](production-readiness.md).
