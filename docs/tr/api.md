# API Referansı (TR)

Bu sayfa ana tipleri ve birbirleriyle nasıl uyumlu olduklarını açıklar. Tam metod imzaları ve otomatik üretilen doküman için [API sitesine](https://keremvaris.github.io/Intentum/api/) bakın.

---

## Intentum nasıl çalışır (tipik akış)

1. **Gözlemle** — Kullanıcı veya sistem olaylarını (örn. login, retry, submit) bir **BehaviorSpace** içine kaydedersin.
2. **Çıkar** — Bir **LlmIntentModel** (embedding sağlayıcı ve similarity engine ile) bu davranışı bir **Intent** ve güven seviyesine (High / Medium / Low / Certain) dönüştürür.
3. **Karar ver** — **Intent** ve bir **IntentPolicy** (kurallar) ile **Decide** çağırırsın; **Allow**, **Observe**, **Warn** veya **Block** alırsın.

Yani: *davranış → intent → policy kararı*. Sabit senaryo adımları yok; model gözlenen olaylardan intent’i çıkarır.

---

## Core (`Intentum.Core`)

| Tip | Ne işe yarar |
|-----|----------------|
| **BehaviorSpace** | Gözlenen olayların konteyneri. `.Observe(actor, action)` çağırırsın (örn. `"user"`, `"login"`). Çıkarım için `.ToVector()` ile behavior vektörü alırsın. |
| **Intent** | Çıkarım sonucu: güven seviyesi, skor ve sinyaller (ağırlıklı davranışlar). |
| **IntentConfidence** | Intent’in parçası: `Level` (string) ve `Score` (0–1). |
| **IntentEvaluator** | Intent’i kriterlere göre değerlendirir; model tarafından dahili kullanılır. |

**Nereden başlanır:** Bir `BehaviorSpace` oluştur, her olay için `.Observe(...)` çağır, sonra space’i intent modelinin `Infer(space)` metoduna ver.

---

## Runtime (`Intentum.Runtime`)

| Tip | Ne işe yarar |
|-----|----------------|
| **IntentPolicy** | Sıralı kural listesi. `.AddRule(PolicyRule(...))` ile kural eklenir. İlk eşleşen kural kazanır. |
| **PolicyRule** | İsim + koşul (örn. `Intent` üzerinde lambda) + **PolicyDecision** (Allow, Observe, Warn, Block). |
| **IntentPolicyEngine** | Intent’i policy’ye göre değerlendirir; **PolicyDecision** döndürür. |
| **RuntimeExtensions.Decide** | Extension: `intent.Decide(policy)` — policy’yi çalıştırır ve kararı döndürür. |
| **RuntimeExtensions.ToLocalizedString** | Extension: `decision.ToLocalizedString(localizer)` — insan tarafından okunabilir metin (örn. UI için). |
| **IIntentumLocalizer** / **DefaultLocalizer** | Karar etiketleri için yerelleştirme (örn. "Allow", "Block"). **DefaultLocalizer** culture alır (örn. `"tr"`). |

**Nereden başlanır:** `.AddRule(...)` ile bir `IntentPolicy` oluştur (örn. önce Block kuralları, sonra Allow). Çıkarımdan sonra `intent.Decide(policy)` çağır.

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

// 3) Karar ver
var policy = new IntentPolicy()
    .AddRule(new PolicyRule("AllowHigh", i => i.Confidence.Level is "High" or "Certain", PolicyDecision.Allow));
var decision = intent.Decide(policy);
```

Tam çalışan örnek için [sample](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample) ve [Kurulum](setup.md).
