using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Intentum.AI.Embeddings;
using Intentum.AI.Http;
using JetBrains.Annotations;

namespace Intentum.AI.Gemini;

/// <summary>
/// Gemini embedding provider. On 429 after retries throws <see cref="GeminiRateLimitException"/>.
/// Built-in retry for 429 (up to 5 attempts, respects Retry-After).
/// </summary>
public sealed class GeminiEmbeddingProvider(GeminiOptions options, HttpClient httpClient) : IIntentEmbeddingProvider
{
    /// <inheritdoc />
    public IntentEmbedding Embed(string behaviorKey)
        => EmbedAsync(behaviorKey, CancellationToken.None).GetAwaiter().GetResult();

    /// <inheritdoc />
    public async Task<IntentEmbedding> EmbedAsync(string behaviorKey, CancellationToken cancellationToken = default)
    {
        options.Validate();

        var request = new GeminiEmbedRequest(
            new GeminiContent([new GeminiPart(behaviorKey)]));
        var url = $"models/{options.EmbeddingModel}:embedContent?key={options.ApiKey}";

        var response = await EmbeddingHttpRetryHandler.SendWithRetryAsync(
            ct => httpClient.PostAsJsonAsync(url, request, ct),
            (retryAfterSec, body) => throw new GeminiRateLimitException(retryAfterSec, body),
            cancellationToken: cancellationToken);

        var payload = await response.Content
            .ReadFromJsonAsync<GeminiEmbedResponse>(cancellationToken);

        var values = payload?.Embedding.Values ?? [];
        var score = EmbeddingScore.Normalize(values);

        return new IntentEmbedding(Source: behaviorKey, Score: score);
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
