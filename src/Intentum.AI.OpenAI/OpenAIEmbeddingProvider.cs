using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Intentum.AI.Embeddings;
using Intentum.AI.Http;
using JetBrains.Annotations;

namespace Intentum.AI.OpenAI;

/// <summary>
/// OpenAI embedding provider. Calls the OpenAI embeddings API; on 429 after retries throws <see cref="OpenAIRateLimitException"/>.
/// Built-in retry for 429 (up to 5 attempts, respects Retry-After).
/// </summary>
public sealed class OpenAIEmbeddingProvider(OpenAIOptions options, HttpClient httpClient) : IIntentEmbeddingProvider
{
    /// <inheritdoc />
    public IntentEmbedding Embed(string behaviorKey)
        => EmbedAsync(behaviorKey, CancellationToken.None).GetAwaiter().GetResult();

    /// <inheritdoc />
    public async Task<IntentEmbedding> EmbedAsync(string behaviorKey, CancellationToken cancellationToken = default)
    {
        options.Validate();

        var request = new OpenAIEmbeddingRequest(options.EmbeddingModel, behaviorKey);

        var response = await EmbeddingHttpRetryHandler.SendWithRetryAsync(
            ct => httpClient.PostAsJsonAsync("embeddings", request, ct),
            (retryAfterSec, body) => throw new OpenAIRateLimitException(retryAfterSec, body),
            cancellationToken: cancellationToken);

        var payload = await response.Content
            .ReadFromJsonAsync<OpenAIEmbeddingResponse>(cancellationToken);

        var values = payload?.Data.FirstOrDefault()?.Embedding ?? [];
        var score = EmbeddingScore.Normalize(values);

        return new IntentEmbedding(Source: behaviorKey, Score: score);
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
