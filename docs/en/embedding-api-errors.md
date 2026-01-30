# Embedding API error handling

This document describes **current behavior** when embedding providers (OpenAI, Azure, Gemini, Mistral, etc.) fail, and how to add **timeout**, **retry**, and **rate-limit handling** for production.

## Current behavior

- **HTTP errors:** Each provider calls `response.EnsureSuccessStatusCode()`. On non-2xx (e.g. 401, 429, 500), an `HttpRequestException` is thrown. The caller (e.g. `LlmIntentModel.Infer`) does not catch it; the exception propagates.
- **Timeout:** No explicit timeout is set inside the provider. The `HttpClient` passed via DI uses its default timeout (often 100 seconds). Configure `HttpClient.Timeout` when registering the client to limit wait time.
- **Retry / circuit breaker:** Providers do **not** implement retry or circuit breaker. Transient failures (network, 429, 503) will fail the request once.

## Recommendations

### 1. Timeout

Set a timeout on the `HttpClient` used by the embedding provider so that slow or stuck API calls do not hang indefinitely:

```csharp
services.AddHttpClient<OpenAIEmbeddingProvider>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});
```

(Adjust the client registration name to match how you register the OpenAI provider; see [Providers](providers.md) and [AI providers how-to](ai-providers-howto.md).)

### 2. Retry (transient failures)

For transient failures (e.g. 429 rate limit, 503 service unavailable), use a retry policy around the HTTP client:

- **Polly:** Add a `DelegatingHandler` that uses Polly’s `RetryPolicy` or `ResiliencePipeline` (e.g. retry 2–3 times with exponential backoff on 429/503). Register the handler when adding the `HttpClient` for the embedding provider.
- **Custom handler:** Implement a `DelegatingHandler` that, on 429 or 503, waits and retries a limited number of times, then throws.

After retries are exhausted, the provider still throws; the application can catch `HttpRequestException` and log or emit an “inference failed” event (see below).

### 3. Rate limit (429)

When the API returns 429 (Too Many Requests), the provider throws. To reduce 429s:

- Use **rate limiting** (e.g. [Intentum.Runtime MemoryRateLimiter](api.md) or a token bucket) before calling the embedding provider so you do not exceed the API’s request rate.
- Combine with **retry with backoff** (e.g. Polly) so that occasional 429s are retried after a delay (respect `Retry-After` if the API sends it).

### 4. Logging and “inference failed” events

- **Logging:** In the application layer, wrap `model.Infer(space)` in try/catch; on `HttpRequestException` (or similar), log the error and optionally return a fallback intent or rethrow.
- **Events:** If you use [Intentum.Events](advanced-features.md), you can define a custom event type (e.g. `InferenceFailed`) and publish it when embedding or inference fails, so that monitoring or downstream systems are aware.

## Summary

| Aspect            | Current behavior                          | Recommendation                                      |
|------------------|--------------------------------------------|----------------------------------------------------|
| **HTTP errors**  | `EnsureSuccessStatusCode()` → throws      | Catch at app layer; log; optional fallback/event  |
| **Timeout**      | HttpClient default (e.g. 100 s)           | Set `HttpClient.Timeout` (e.g. 30 s)              |
| **Retry**        | None                                       | Polly or custom handler with backoff for 429/503  |
| **Rate limit**   | 429 → throw                                | Rate limit calls; retry with backoff on 429       |

These practices apply to all HTTP-based embedding providers (OpenAI, Azure OpenAI, Gemini, Mistral); configure the same `HttpClient` (and handlers) used for each provider’s registration.
