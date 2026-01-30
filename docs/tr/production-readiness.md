# Üretim hazırlığı

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
- **Önbellek:** Tekrarlayan davranış anahtarları API'yi tekrar çağırmasın diye [CachedEmbeddingProvider](api.md) (veya Redis/FusionCache adaptörleri) kullanın. Maliyet ve gecikmeyi azaltır.
- **Benchmark:** Gecikme ve throughput için [benchmarks](../../benchmarks/README.md) çalıştırın; timeout ve rate limit boyutlandırmasında bunu kullanın.

## Özet

| Konu          | Nerede         |
|---------------|-----------------|
| Rate limiting | [api.md](api.md) (MemoryRateLimiter, DecideWithRateLimit), [embedding-api-errors.md](embedding-api-errors.md) |
| Fallback      | [ChainedIntentModel](api.md), [examples/ai-fallback-intent](../../examples/ai-fallback-intent/), [embedding-api-errors.md](embedding-api-errors.md) |
| Maliyet       | ToVectorOptions (cap/örnekleme), CachedEmbeddingProvider, [benchmarks](../../benchmarks/README.md) |
