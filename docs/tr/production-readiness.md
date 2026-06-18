# Üretim hazırlığı

**Bu sayfayı neden okuyorsunuz?** Bu sayfa Intentum'u production'da kullanırken rate limiting, fallback ve maliyet kontrolü için kısa rehber sunar. Gerçek embedding API'leriyle canlıya geçmeden önce bu sayfayı okumanız faydalıdır.

Intentum'u gerçek embedding API'leriyle kullanırken **rate limiting**, **fallback** ve **maliyet kontrolü** için kısa rehber.

## Rate limiting

- **Intentum.Runtime:** [MemoryRateLimiter](api.md) (bellek içi sabit pencere), bir anahtarın (örn. kullanıcı veya oturum) `RateLimit` türünde politika kararı tetikleme sıklığını sınırlar. `intent.DecideWithRateLimit(policy, rateLimiter, options)` ile kullanın.
- **Embedding API:** Sağlayıcının istek hızını aşmamak (ve 429'ları önlemek) için embedding sağlayıcısını ne sıklıkla çağırdığınızı sınırlayın. Seçenekler: (1) Sağlayıcıyı `LlmIntentModel`'e vermeden önce rate-limiting katmanı (örn. token bucket) ile sarın; (2) Kuyruk kullanıp inference'ı daraltın; (3) Embedding'leri önbelleğe alın (bkz. [AI sağlayıcılarını kullanma](ai-providers-howto.md)) böylece tekrarlayan davranış anahtarları API'yi tekrar çağırmaz.
- Retry ve 429 yönetimi için [Embedding API hata yönetimi](embedding-api-errors.md) sayfasına bakın.

## Fallback

Embedding API başarısız olduğunda (timeout, 429, 5xx):

- **Uygulama katmanında yakala:** `model.Infer(space)`'i try/catch ile sarın; `HttpRequestException`'da loglayıp **fallback intent** (örn. düşük güven, tek sinyal) döndürün veya yeniden fırlatın.
- **Kural tabanlı fallback:** [ChainedIntentModel](api.md) kullanın: önce LLM dene; güven eşiğin altındaysa veya inference başarısızsa [RuleBasedIntentModel](api.md) ile devam et. Bkz. [examples/chained-intent](../../examples/chained-intent/) ve [examples/ai-fallback-intent](../../examples/ai-fallback-intent/).
- **Önbellek fallback:** Önbellekli embedding sağlayıcısı kullanıyorsanız, API hatasında aynı davranış anahtarı için (varsa) önbellekteki sonucu veya varsayılan düşük güvenli intent döndürebilirsiniz.

## Maliyet kontrolü

- **Embedding çağrılarını sınırla:** Büyük davranış uzaylarında boyut sayısı (benzersiz actor:action) embedding çağrı sayısına eşittir. Boyut sayısını sınırlamak için [ToVectorOptions](api.md) (örn. CapPerDimension, normalizasyon) kullanın veya modele çağırmadan önce **örnekleme** (örn. sayıya göre ilk N) yapın.
- **Önbellek:** Tekrarlayan davranış anahtarları API'yi tekrar çağırmasın diye [CachedEmbeddingProvider](api.md) (veya Redis adaptörü) kullanın. Maliyet ve gecikmeyi azaltır.
- **Benchmark:** Gecikme ve throughput için [benchmarks](../../benchmarks/README.md) çalıştırın; timeout ve rate limit boyutlandırmasında bunu kullanın.

## Resilience Pattern'leri (v1.2)

Intentum.Runtime artık production-grade resilience pattern'leri içerir:

### Circuit Breaker
`ICircuitBreaker` — Art arda başarısız çağrıları tespit edip sistemi korur. Üç durum: **Closed** (normal), **Open** (engellenmiş), **HalfOpen** (deneme). Varsayılan: 3 başarısızlık → 30sn açık → HalfOpen → başarılı olursa Closed.

```csharp
var cb = new MemoryCircuitBreaker(new CircuitBreakerOptions(
    FailureThreshold: 5,
    DurationOfBreak: TimeSpan.FromSeconds(60)));
var result = await cb.ExecuteAsync(() => SomeRiskyOperationAsync());
```

### Retry Policy
`IRetryPolicy` — Geçici hataları belirlenen aralıklarla yeniden dener. Üç backoff türü: **Constant**, **Linear**, **Exponential**. Varsayılan: 3 tekrar, exponential backoff.

```csharp
var retry = new MemoryRetryPolicy(new RetryOptions(
    MaxRetries: 3,
    BaseDelay: TimeSpan.FromMilliseconds(100),
    Backoff: RetryBackoffType.Exponential));
var result = await retry.ExecuteAsync(() => UnreliableApiCallAsync());
```

### Bulkhead
`IBulkhead` — Eşzamanlı çalışan işlem sayısını sınırlayarak kaynakları korur. Varsayılan: 10 paralel, 10 kuyruk, 30sn timeout.

```csharp
var bulkhead = new MemoryBulkhead(new BulkheadOptions(
    MaxParallelization: 5,
    QueueTimeout: TimeSpan.FromSeconds(10)));
var result = await bulkhead.ExecuteAsync(() => Task.FromResult(42));
```

### Degradation Policy
`IDegradationPolicy` — Art arda başarısızlıklarda degrade moduna geçer ve fallback döndürür. Belirli aralık sonra kendiliğinden düzelir.

```csharp
var degradation = new MemoryDegradationPolicy(new DegradationOptions(
    DegradationThreshold: 3,
    CheckInterval: TimeSpan.FromSeconds(30)));
var result = await degradation.ExecuteAsync(
    () => PrimaryOperationAsync(),
    () => FallbackResult());
```

### Timeout Policy
`ITimeoutPolicy` — İşlemi belirlenen sürede tamamlanmazsa iptal eder. Varsayılan: 5 saniye.

```csharp
var timeout = new MemoryTimeoutPolicy(new TimeoutOptions(
    TimeoutDuration: TimeSpan.FromSeconds(3)));
var result = await timeout.ExecuteAsync(ct => FastOperationAsync(ct));
```

### Toplu Kayıt
Tüm resilience pattern'lerini tek çağrıyla kaydetmek için `AddIntentumResilience()` kullanılır:

```csharp
services.AddIntentumResilience();
```

## Özet

| Konu          | Nerede         |
|---------------|-----------------|
| Rate limiting | [api.md](api.md) (MemoryRateLimiter, DecideWithRateLimit), [embedding-api-errors.md](embedding-api-errors.md) |
| Fallback      | [ChainedIntentModel](api.md), [examples/ai-fallback-intent](../../examples/ai-fallback-intent/), [embedding-api-errors.md](embedding-api-errors.md) |
| Maliyet       | ToVectorOptions (cap/örnekleme), CachedEmbeddingProvider, [benchmarks](../../benchmarks/README.md) |
| Circuit Breaker | [Intentum.Runtime.Resilience](../../src/Intentum.Runtime/Resilience/) — `ICircuitBreaker` |
| Retry         | [Intentum.Runtime.Resilience](../../src/Intentum.Runtime/Resilience/) — `IRetryPolicy` |
| Bulkhead      | [Intentum.Runtime.Resilience](../../src/Intentum.Runtime/Resilience/) — `IBulkhead` |
| Degradation   | [Intentum.Runtime.Resilience](../../src/Intentum.Runtime/Resilience/) — `IDegradationPolicy` |
| Timeout       | [Intentum.Runtime.Resilience](../../src/Intentum.Runtime/Resilience/) — `ITimeoutPolicy` |

**Sonraki adım:** Bu sayfayı bitirdiyseniz → [Embedding API hata yönetimi](embedding-api-errors.md) veya [Benchmark'lar](benchmarks.md).
