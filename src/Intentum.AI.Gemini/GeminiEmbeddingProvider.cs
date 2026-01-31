using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Intentum.AI.Embeddings;
using JetBrains.Annotations;

namespace Intentum.AI.Gemini;

/// <summary>
/// Gemini embedding provider. On 429 after retries throws <see cref="GeminiRateLimitException"/> with optional Retry-After; other non-2xx throw <see cref="HttpRequestException"/>.
/// Built-in retry for 429 (up to 5 attempts, respects Retry-After). No timeout (use <see cref="HttpClient.Timeout"/>).
/// </summary>
public sealed class GeminiEmbeddingProvider(GeminiOptions options, HttpClient httpClient) : IIntentEmbeddingProvider
{
    public IntentEmbedding Embed(string behaviorKey)
    {
        options.Validate();

        var request = new GeminiEmbedRequest(
            new GeminiContent([new GeminiPart(behaviorKey)]));

        var url = $"models/{options.EmbeddingModel}:embedContent?key={options.ApiKey}";

        const int maxAttempts = 5;
        const int maxWaitSeconds = 90;
        HttpResponseMessage? response = null;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            response = httpClient
                .PostAsJsonAsync(url, request)
                .GetAwaiter()
                .GetResult();

            if (response.IsSuccessStatusCode)
                break;
            if (response.StatusCode == HttpStatusCode.TooManyRequests && attempt == maxAttempts)
            {
                var retryAfterSec = response.Headers.RetryAfter?.Delta?.TotalSeconds;
                var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                response.Dispose();
                throw new GeminiRateLimitException(retryAfterSec, body);
            }
            if (response.StatusCode != HttpStatusCode.TooManyRequests)
                response.EnsureSuccessStatusCode();

            var delay = TimeSpan.FromSeconds(5 * attempt);
            if (response.Headers.RetryAfter?.Delta is { } retryAfter)
                delay = TimeSpan.FromSeconds(Math.Min(retryAfter.TotalSeconds, maxWaitSeconds));
            response.Dispose();
            Thread.Sleep(delay);
        }

        response!.EnsureSuccessStatusCode();

        var payload = response.Content
            .ReadFromJsonAsync<GeminiEmbedResponse>()
            .GetAwaiter()
            .GetResult();

        var values = payload?.Embedding.Values ?? new List<double>();
        var score = EmbeddingScore.Normalize(values);

        return new IntentEmbedding(
            Source: behaviorKey,
            Score: score
        );
    }

    [UsedImplicitly]
    private sealed record GeminiEmbedRequest(
        [property: JsonPropertyName("content")] GeminiContent Content);

    [UsedImplicitly]
    private sealed record GeminiContent(
        [property: JsonPropertyName("parts")] IReadOnlyList<GeminiPart> Parts);

    [UsedImplicitly]
    private sealed record GeminiPart(
        [property: JsonPropertyName("text")] string Text);

    private sealed record GeminiEmbedResponse(
        [property: JsonPropertyName("embedding")] GeminiEmbedding Embedding);

    private sealed record GeminiEmbedding(
        [property: JsonPropertyName("values")] List<double> Values);
}
