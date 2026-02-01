using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Intentum.AI.Embeddings;
using JetBrains.Annotations;

namespace Intentum.AI.OpenAI;

/// <summary>
/// OpenAI embedding provider. Calls the OpenAI embeddings API; on 429 after retries throws <see cref="OpenAIRateLimitException"/> with optional Retry-After; other non-2xx throw <see cref="HttpRequestException"/>.
/// Built-in retry for 429 (up to 5 attempts, respects Retry-After). No timeout (use <see cref="HttpClient.Timeout"/>).
/// </summary>
public sealed class OpenAIEmbeddingProvider(OpenAIOptions options, HttpClient httpClient) : IIntentEmbeddingProvider
{
    /// <inheritdoc />
    public IntentEmbedding Embed(string behaviorKey)
    {
        options.Validate();

        var request = new OpenAIEmbeddingRequest(options.EmbeddingModel, behaviorKey);
        const int maxAttempts = 5;
        const int maxWaitSeconds = 90;
        HttpResponseMessage? response = null;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            response = httpClient
                .PostAsJsonAsync("embeddings", request)
                .GetAwaiter()
                .GetResult();

            if (response.IsSuccessStatusCode)
                break;
            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests && attempt == maxAttempts)
            {
                var retryAfterSec = response.Headers.RetryAfter?.Delta?.TotalSeconds;
                var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                response.Dispose();
                throw new OpenAIRateLimitException(retryAfterSec, body);
            }
            if (response.StatusCode != System.Net.HttpStatusCode.TooManyRequests)
                response.EnsureSuccessStatusCode();

            var delay = TimeSpan.FromSeconds(5 * attempt);
            if (response.Headers.RetryAfter?.Delta is { } retryAfter)
                delay = TimeSpan.FromSeconds(Math.Min(retryAfter.TotalSeconds, maxWaitSeconds));
            response.Dispose();
            Thread.Sleep(delay);
        }

        response!.EnsureSuccessStatusCode();

        var payload = response.Content
            .ReadFromJsonAsync<OpenAIEmbeddingResponse>()
            .GetAwaiter()
            .GetResult();

        var values = payload?.Data.FirstOrDefault()?.Embedding ?? [];
        var score = EmbeddingScore.Normalize(values);

        return new IntentEmbedding(
            Source: behaviorKey,
            Score: score
        );
    }

    [UsedImplicitly]
    private sealed record OpenAIEmbeddingRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("input")] string Input);

    private sealed record OpenAIEmbeddingResponse(
        [property: JsonPropertyName("data")] List<OpenAIEmbeddingData> Data);

    private sealed record OpenAIEmbeddingData(
        [property: JsonPropertyName("embedding")] List<double> Embedding);
}
