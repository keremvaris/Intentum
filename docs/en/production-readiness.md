# Production readiness

Short guide to **rate limiting**, **fallback**, and **cost control** when using Intentum with real embedding APIs.

## Rate limiting

- **Intentum.Runtime:** [MemoryRateLimiter](api.md) (in-memory fixed window) limits how often a key (e.g. user or session) can trigger a policy decision of type `RateLimit`. Use it with `intent.DecideWithRateLimit(policy, rateLimiter, options)`.
- **Embedding API:** To avoid exceeding the provider's request rate (and 429s), limit how often you call the embedding provider. Options: (1) Wrap the provider in a rate-limiting layer (e.g. token bucket) before passing it to `LlmIntentModel`; (2) Use a queue and throttle inference; (3) Cache embeddings (see [AI providers how-to](ai-providers-howto.md)) so repeated behavior keys do not call the API again.
- See [Embedding API error handling](embedding-api-errors.md) for retry and 429 handling.

## Fallback

When the embedding API fails (timeout, 429, 5xx):

- **Catch at app layer:** Wrap `model.Infer(space)` in try/catch; on `HttpRequestException`, log and either return a **fallback intent** (e.g. low confidence, single signal) or rethrow.
- **Rule-based fallback:** Use [ChainedIntentModel](api.md): try LLM first; if confidence below threshold or inference fails, fall back to a [RuleBasedIntentModel](api.md). See [examples/chained-intent](../../examples/chained-intent/) and [examples/ai-fallback-intent](../../examples/ai-fallback-intent/).
- **Cache fallback:** If you use a cached embedding provider, on API failure you can return a cached result for the same behavior key (if available) or a default low-confidence intent.

## Cost control

- **Cap embedding calls:** For large behavior spaces, the number of dimensions (unique actor:action) equals the number of embedding calls. Use [ToVectorOptions](api.md) (e.g. CapPerDimension, normalization) to limit dimension count, or **sample** dimensions (e.g. top N by count) before calling the model.
- **Cache:** Use [CachedEmbeddingProvider](api.md) (or Redis/FusionCache adapters) so repeated behavior keys do not call the API. Reduces cost and latency.
- **Benchmark:** Run the [benchmarks](../../benchmarks/README.md) to see latency and throughput; use that to size timeouts and rate limits.

## Summary

| Topic          | Where to look |
|----------------|----------------|
| Rate limiting | [api.md](api.md) (MemoryRateLimiter, DecideWithRateLimit), [embedding-api-errors.md](embedding-api-errors.md) |
| Fallback      | [ChainedIntentModel](api.md), [examples/ai-fallback-intent](../../examples/ai-fallback-intent/), [embedding-api-errors.md](embedding-api-errors.md) |
| Cost          | ToVectorOptions (cap/sampling), CachedEmbeddingProvider, [benchmarks](../../benchmarks/README.md) |
