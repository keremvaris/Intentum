using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Intentum.AI.Embeddings;
using Intentum.AI.Http;
using JetBrains.Annotations;

namespace Intentum.AI.Mistral;

/// <summary>
/// Mistral embedding provider. On 429 after retries throws <see cref="MistralRateLimitException"/>.
/// Built-in retry for 429 (up to 5 attempts, respects Retry-After).
/// </summary>
public sealed class MistralEmbeddingProvider(MistralOptions options, HttpClient httpClient) : IIntentEmbeddingProvider
{
    /// <inheritdoc />
    public IntentEmbedding Embed(string behaviorKey)
        => EmbedAsync(behaviorKey, CancellationToken.None).GetAwaiter().GetResult();

    /// <inheritdoc />
    public async Task<IntentEmbedding> EmbedAsync(string behaviorKey, CancellationToken cancellationToken = default)
    {
        options.Validate();

        var request = new MistralEmbeddingRequest(options.EmbeddingModel, [behaviorKey]);

        var response = await EmbeddingHttpRetryHandler.SendWithRetryAsync(
            ct => httpClient.PostAsJsonAsync("embeddings", request, ct),
            (retryAfterSec, body) => throw new MistralRateLimitException(retryAfterSec, body),
            cancellationToken: cancellationToken);

        var payload = await response.Content
            .ReadFromJsonAsync<MistralEmbeddingResponse>(cancellationToken);

        var values = payload?.Data.FirstOrDefault()?.Embedding ?? [];
        var score = EmbeddingScore.Normalize(values);

        return new IntentEmbedding(Source: behaviorKey, Score: score);
    }

    [UsedImplicitly]
    private sealed record MistralEmbeddingRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("input")] IReadOnlyList<string> Input);

    private sealed record MistralEmbeddingResponse(
        [property: JsonPropertyName("data")] List<MistralEmbeddingData> Data);

    private sealed record MistralEmbeddingData(
        [property: JsonPropertyName("embedding")] List<double> Embedding);
}
