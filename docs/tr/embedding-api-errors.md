# Embedding API hata yönetimi

Bu belge, embedding sağlayıcıları (OpenAI, Azure, Gemini, Mistral vb.) hata verdiğinde **mevcut davranışı** ve production için **timeout**, **retry** ve **rate-limit** eklemenin nasıl yapılacağını açıklar.

## Mevcut davranış

- **HTTP hataları:** Her sağlayıcı `response.EnsureSuccessStatusCode()` çağırır. 2xx dışı (örn. 401, 429, 500) durumda `HttpRequestException` fırlatılır. Çağıran (örn. `LlmIntentModel.Infer`) yakalamaz; istisna yukarı yayılır.
- **Timeout:** Sağlayıcı içinde açık timeout yok. DI ile verilen `HttpClient` kendi varsayılan timeout'unu (genelde 100 saniye) kullanır. Bekleme süresini sınırlamak için istemci kaydederken `HttpClient.Timeout` ayarlayın.
- **Retry / circuit breaker:** Sağlayıcılar **retry veya circuit breaker** uygulamaz. Geçici hatalar (ağ, 429, 503) tek denemede kalır.

## Öneriler

### 1. Timeout

Embedding sağlayıcısı için kullanılan `HttpClient` üzerinde timeout verin; yavaş veya takılı API çağrıları sınırsız beklemesin:

```csharp
services.AddHttpClient<OpenAIEmbeddingProvider>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});
```

(OpenAI sağlayıcısını nasıl kaydettiğinize göre istemci adını uyarlayın; bkz. [Sağlayıcılar](providers.md) ve [AI sağlayıcıları how-to](ai-providers-howto.md).)

### 2. Retry (geçici hatalar)

Geçici hatalar (örn. 429 rate limit, 503 service unavailable) için HTTP istemcisine retry ekleyin:

- **Polly:** Polly’nin `RetryPolicy` veya `ResiliencePipeline` kullanan bir `DelegatingHandler` ekleyin (örn. 429/503’te 2–3 kez exponential backoff ile retry). Embedding sağlayıcısı için `HttpClient` eklerken bu handler’ı kaydedin.
- **Özel handler:** 429 veya 503’te bekleyip sınırlı sayıda yeniden deneyen bir `DelegatingHandler` yazın; tükendikten sonra fırlatın.

Retry’lar bittikten sonra sağlayıcı yine fırlatır; uygulama `HttpRequestException` yakalayıp loglayabilir veya “inference failed” event’i üretebilir (aşağıya bakın).

### 3. Rate limit (429)

API 429 (Too Many Requests) döndüğünde sağlayıcı fırlatır. 429’ları azaltmak için:

- **Rate limiting** kullanın (örn. [Intentum.Runtime MemoryRateLimiter](api.md) veya token bucket); embedding sağlayıcısını çağırmadan önce istek hızını API limitinin altında tutun.
- **Retry with backoff** (örn. Polly) ile birleştirin; ara sıra 429’lar gecikmeyle yeniden denensin (API `Retry-After` gönderiyorsa buna uyun).

### 4. Loglama ve “inference failed” event’leri

- **Loglama:** Uygulama katmanında `model.Infer(space)` çağrısını try/catch ile sarın; `HttpRequestException` (veya benzeri) durumunda hatayı loglayın, isteğe bağlı fallback intent dönün veya yeniden fırlatın.
- **Event’ler:** [Intentum.Events](advanced-features.md) kullanıyorsanız, embedding veya inference başarısız olduğunda yayınlanacak özel bir event türü (örn. `InferenceFailed`) tanımlayabilirsiniz; böylece izleme veya downstream sistemler haberdar olur.

## Özet

| Konu              | Mevcut davranış                            | Öneri                                                |
|-------------------|--------------------------------------------|------------------------------------------------------|
| **HTTP hataları** | `EnsureSuccessStatusCode()` → fırlatır    | Uygulama katmanında yakala; logla; isteğe bağlı fallback/event |
| **Timeout**       | HttpClient varsayılanı (örn. 100 s)       | `HttpClient.Timeout` ayarla (örn. 30 s)              |
| **Retry**         | Yok                                        | 429/503 için Polly veya özel handler + backoff     |
| **Rate limit**    | 429 → fırlatır                             | Çağrıları rate limit’le; 429’da backoff ile retry   |

Bu uygulamalar tüm HTTP tabanlı embedding sağlayıcıları (OpenAI, Azure OpenAI, Gemini, Mistral) için geçerlidir; her sağlayıcının kaydında kullanılan aynı `HttpClient` (ve handler’lar) yapılandırılmalıdır.
