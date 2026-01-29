# API Referansı (TR)

Bu sayfa ana tipleri ve birbirleriyle nasıl uyumlu olduklarını açıklar. Tam metod imzaları ve otomatik üretilen doküman için [API sitesine](https://keremvaris.github.io/Intentum/api/) bakın.

---

## Intentum nasıl çalışır (tipik akış)

1. **Gözlemle** — Kullanıcı veya sistem olaylarını (örn. login, retry, submit) bir **BehaviorSpace** içine kaydedersin.
2. **Çıkar** — Bir **LlmIntentModel** (embedding sağlayıcı ve similarity engine ile) bu davranışı bir **Intent** ve güven seviyesine (High / Medium / Low / Certain) dönüştürür.
3. **Karar ver** — **Intent** ve bir **IntentPolicy** (kurallar) ile **Decide** çağırırsın; **Allow**, **Observe**, **Warn**, **Block**, **Escalate**, **RequireAuth** veya **RateLimit** alırsın.

Yani: *davranış → intent → policy kararı*. Sabit senaryo adımları yok; model gözlenen olaylardan intent’i çıkarır.

---

## Core (`Intentum.Core`)

| Tip | Ne işe yarar |
|-----|----------------|
| **BehaviorSpace** | Gözlenen olayların konteyneri. `.Observe(actor, action)` çağırırsın (örn. `"user"`, `"login"`). Çıkarım için `.ToVector()` ile behavior vektörü alırsın. |
| **Intent** | Çıkarım sonucu: güven seviyesi, skor ve sinyaller (ağırlıklı davranışlar). |
| **IntentConfidence** | Intent’in parçası: `Level` (string) ve `Score` (0–1). |
| **IntentSignal** | Intent'teki bir sinyal: `Source`, `Description`, `Weight`. |
| **IntentEvaluator** | Intent’i kriterlere göre değerlendirir; model tarafından dahili kullanılır. |

**Namespace:** `Intent`, `IntentConfidence` ve `IntentSignal` **`Intentum.Core.Intents`** ad alanındadır. Kullanmak için `using Intentum.Core.Intents;` ekleyin.

**Nereden başlanır:** Bir `BehaviorSpace` oluştur, her olay için `.Observe(...)` çağır, sonra space’i intent modelinin `Infer(space)` metoduna ver.

---

## Runtime (`Intentum.Runtime`)

| Tip | Ne işe yarar |
|-----|----------------|
| **IntentPolicy** | Sıralı kural listesi. `.AddRule(PolicyRule(...))` ile kural eklenir. İlk eşleşen kural kazanır. |
| **IntentPolicyBuilder** | IntentPolicy oluşturmak için fluent builder. `.Allow(...)`, `.Block(...)`, `.Escalate(...)` vb. metodlar kullanılır. |
| **PolicyRule** | İsim + koşul (örn. `Intent` üzerinde lambda) + **PolicyDecision** (Allow, Observe, Warn, Block, Escalate, RequireAuth, RateLimit). |
| **PolicyDecision** | Karar enum'u: **Allow**, **Observe**, **Warn**, **Block**, **Escalate**, **RequireAuth**, **RateLimit**. |
| **IntentPolicyEngine** | Intent’i policy’ye göre değerlendirir; **PolicyDecision** döndürür. |
| **RuntimeExtensions.Decide** | Extension: `intent.Decide(policy)` — policy’yi çalıştırır ve kararı döndürür. |
| **RuntimeExtensions.DecideWithRateLimit** / **DecideWithRateLimitAsync** | Karar RateLimit olduğunda **IRateLimiter** ile kontrol eder; **RateLimitResult** döndürür. **RateLimitOptions** (Key, Limit, Window) geçirin. |
| **RateLimitOptions** | Key, Limit, Window — DecideWithRateLimit / DecideWithRateLimitAsync ile kullanın. |
| **IRateLimiter** / **MemoryRateLimiter** | PolicyDecision.RateLimit için rate limiting. **MemoryRateLimiter** = in-memory fixed window; çok node için dağıtık implementasyon kullanın. |
| **RateLimitResult** | Allowed, CurrentCount, Limit, RetryAfter. |
| **RuntimeExtensions.ToLocalizedString** | Extension: `decision.ToLocalizedString(localizer)` — insan tarafından okunabilir metin (örn. UI için). |
| **IIntentumLocalizer** / **DefaultLocalizer** | Karar etiketleri için yerelleştirme (örn. "Allow", "Block"). **DefaultLocalizer** culture alır (örn. `"tr"`). |

**Nereden başlanır:** `.AddRule(...)` ile bir `IntentPolicy` oluştur (örn. önce Block kuralları, sonra Allow). Çıkarımdan sonra `intent.Decide(policy)` çağır.

**Fluent API örneği:**
```csharp
var policy = new IntentPolicyBuilder()
    .Block("ExcessiveRetry", i => i.Signals.Count(s => s.Description.Contains("retry")) >= 3)
    .Escalate("LowConfidence", i => i.Confidence.Level == "Low")
    .RequireAuth("SensitiveAction", i => i.Signals.Any(s => s.Description.Contains("sensitive")))
    .Allow("HighConfidence", i => i.Confidence.Level is "High" or "Certain")
    .Build();
```

**Yeni karar tipleri:**
- **Escalate** — Daha yüksek seviyeye yükselt
- **RequireAuth** — Devam etmeden önce ek kimlik doğrulama gerektir
- **RateLimit** — Aksiyona hız sınırı uygula

---

## AI (`Intentum.AI`)

| Tip | Ne işe yarar |
|-----|----------------|
| **IIntentEmbeddingProvider** | Bir behavior anahtarını (örn. `"user:login"`) **IntentEmbedding** (vektör + skor) yapar. Her sağlayıcı (OpenAI, Gemini vb.) veya test için **MockEmbeddingProvider** tarafından uygulanır. |
| **IIntentSimilarityEngine** | Embedding’leri tek bir similarity skoruna birleştirir. **SimpleAverageSimilarityEngine** varsayılan seçenektir. |
| **LlmIntentModel** | Embedding sağlayıcı + similarity engine alır; **Infer(BehaviorSpace)** bir **Intent** (güven ve sinyallerle) döndürür. |

**Nereden başlanır:** Hızlı yerel çalıştırma için **MockEmbeddingProvider** ve **SimpleAverageSimilarityEngine** kullan; production için gerçek sağlayıcıya geç ([Sağlayıcılar](providers.md)).

**AI pipeline (özet):**  
1) **Embedding** — Her davranış anahtarı (`actor:action`) sağlayıcıya gider; vektör + skor döner. Mock = hash; gerçek sağlayıcı = anlamsal embedding.  
2) **Similarity** — Tüm embedding’ler tek skorda birleştirilir (örn. ortalama).  
3) **Confidence** — Skor High/Medium/Low/Certain seviyesine çevrilir.  
4) **Signals** — Her davranışın ağırlığı Intent sinyallerinde yer alır; policy kurallarında (örn. retry sayısı) kullanılabilir.

---

## Sağlayıcılar (opsiyonel paketler)

| Tip | Ne işe yarar |
|-----|----------------|
| **OpenAIEmbeddingProvider** | OpenAI embedding API kullanır; **OpenAIOptions** (örn. `FromEnvironment()`) ile yapılandırılır. |
| **GeminiEmbeddingProvider** | Google Gemini embedding API kullanır; **GeminiOptions**. |
| **MistralEmbeddingProvider** | Mistral embedding API kullanır; **MistralOptions**. |
| **AzureOpenAIEmbeddingProvider** | Azure OpenAI embedding deployment kullanır; **AzureOpenAIOptions**. |
| **ClaudeMessageIntentModel** | Claude tabanlı intent modeli (mesaj skoru); **ClaudeOptions**. |

Sağlayıcılar **AddIntentum\*** extension metodları ve options (env var) ile kaydedilir. Kurulum ve env var için [Sağlayıcılar](providers.md).

---

## Analytics (`Intentum.Analytics`)

| Tip | Ne işe yarar |
|-----|----------------|
| **IIntentAnalytics** | Intent history üzerinde analytics: confidence trendleri, decision dağılımı, anomali tespiti, export. |
| **IntentAnalytics** | IIntentHistoryRepository kullanan varsayılan implementasyon. |
| **ConfidenceTrendPoint** | Confidence trendinde bir bucket (BucketStart, BucketEnd, ConfidenceLevel, Count, AverageScore). |
| **DecisionDistributionReport** | Zaman penceresinde PolicyDecision başına sayı. |
| **AnomalyReport** | Tespit edilen anomali (Type, Description, Severity, Details). |
| **AnalyticsSummary** | Dashboard için özet (trends, distribution, anomalies). |

**Nereden başlanır:** `IIntentHistoryRepository` kaydedin (örn. `AddIntentumPersistence`), sonra `AddIntentAnalytics()` ekleyin ve `IIntentAnalytics` inject edin. `GetSummaryAsync()`, `GetConfidenceTrendsAsync()`, `GetDecisionDistributionAsync()`, `DetectAnomaliesAsync()`, `ExportToJsonAsync()`, `ExportToCsvAsync()` kullanın.

---

## Minimal kod özeti

```csharp
// 1) Davranış oluştur
var space = new BehaviorSpace()
    .Observe("user", "login")
    .Observe("user", "retry")
    .Observe("user", "submit");

// 2) Intent çıkar (Mock = API anahtarı gerekmez)
var model = new LlmIntentModel(
    new MockEmbeddingProvider(),
    new SimpleAverageSimilarityEngine());
var intent = model.Infer(space);

// 3) Karar ver (fluent API ile)
var policy = new IntentPolicyBuilder()
    .Block("ExcessiveRetry", i => i.Signals.Count(s => s.Description.Contains("retry")) >= 3)
    .Escalate("LowConfidence", i => i.Confidence.Level == "Low")
    .RequireAuth("SensitiveAction", i => i.Signals.Any(s => s.Description.Contains("sensitive")))
    .Allow("HighConfidence", i => i.Confidence.Level is "High" or "Certain")
    .Build();
var decision = intent.Decide(policy);
```

Tam çalışan örnek için [sample](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample) ve [Kurulum](setup.md).
