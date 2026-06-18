# Production readiness

**Why you're reading this page:** This page gives a short guide to rate limiting, fallback, and cost control when using Intentum in production with real embedding APIs. It is useful to read before going live.

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
- **Cache:** Use [CachedEmbeddingProvider](api.md) (or Redis adapter) so repeated behavior keys do not call the API. Reduces cost and latency.
- **Benchmark:** Run the [benchmarks](../../benchmarks/README.md) to see latency and throughput; use that to size timeouts and rate limits.

## Resilience Patterns (v1.2)

Intentum.Runtime now includes production-grade resilience patterns:

### Circuit Breaker
`ICircuitBreaker` — Prevents cascading failures by stopping calls to a failing dependency. Three states: **Closed** (normal), **Open** (blocked), **HalfOpen** (trial). Default: 3 failures → 30s open → HalfOpen → if successful, Closed.

```csharp
var cb = new MemoryCircuitBreaker(new CircuitBreakerOptions(
    FailureThreshold: 5,
    DurationOfBreak: TimeSpan.FromSeconds(60)));
var result = await cb.ExecuteAsync(() => SomeRiskyOperationAsync());
```

### Retry Policy
`IRetryPolicy` — Retries transient failures with configurable backoff. Three backoff types: **Constant**, **Linear**, **Exponential**. Default: 3 retries, exponential backoff.

```csharp
var retry = new MemoryRetryPolicy(new RetryOptions(
    MaxRetries: 3,
    BaseDelay: TimeSpan.FromMilliseconds(100),
    Backoff: RetryBackoffType.Exponential));
var result = await retry.ExecuteAsync(() => UnreliableApiCallAsync());
```

### Bulkhead
`IBulkhead` — Limits concurrent operations to protect resources. Default: 10 parallel, 10 queued, 30s timeout.

```csharp
var bulkhead = new MemoryBulkhead(new BulkheadOptions(
    MaxParallelization: 5,
    QueueTimeout: TimeSpan.FromSeconds(10)));
var result = await bulkhead.ExecuteAsync(() => Task.FromResult(42));
```

### Degradation Policy
`IDegradationPolicy` — Enters degraded mode after consecutive failures and returns a fallback. Automatically recovers after a check interval.

```csharp
var degradation = new MemoryDegradationPolicy(new DegradationOptions(
    DegradationThreshold: 3,
    CheckInterval: TimeSpan.FromSeconds(30)));
var result = await degradation.ExecuteAsync(
    () => PrimaryOperationAsync(),
    () => FallbackResult());
```

### Timeout Policy
`ITimeoutPolicy` — Cancels operations that exceed the specified duration. Default: 5 seconds.

```csharp
var timeout = new MemoryTimeoutPolicy(new TimeoutOptions(
    TimeoutDuration: TimeSpan.FromSeconds(3)));
var result = await timeout.ExecuteAsync(ct => FastOperationAsync(ct));
```

### Aggregate Registration
Register all resilience patterns at once with `AddIntentumResilience()`:

```csharp
services.AddIntentumResilience();
```

## Summary

| Topic          | Where to look |
|----------------|----------------|
| Rate limiting | [api.md](api.md) (MemoryRateLimiter, DecideWithRateLimit), [embedding-api-errors.md](embedding-api-errors.md) |
| Fallback      | [ChainedIntentModel](api.md), [examples/ai-fallback-intent](../../examples/ai-fallback-intent/), [embedding-api-errors.md](embedding-api-errors.md) |
| Cost          | ToVectorOptions (cap/sampling), CachedEmbeddingProvider, [benchmarks](../../benchmarks/README.md) |
| Circuit Breaker | [Intentum.Runtime.Resilience](../../src/Intentum.Runtime/Resilience/) — `ICircuitBreaker` |
| Retry         | [Intentum.Runtime.Resilience](../../src/Intentum.Runtime/Resilience/) — `IRetryPolicy` |
| Bulkhead      | [Intentum.Runtime.Resilience](../../src/Intentum.Runtime/Resilience/) — `IBulkhead` |
| Degradation   | [Intentum.Runtime.Resilience](../../src/Intentum.Runtime/Resilience/) — `IDegradationPolicy` |
| Timeout       | [Intentum.Runtime.Resilience](../../src/Intentum.Runtime/Resilience/) — `ITimeoutPolicy` |

**Next step:** When you're done with this page → [Embedding API error handling](embedding-api-errors.md) or [Benchmarks](benchmarks.md).
