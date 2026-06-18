using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Intentum.AI.Embeddings;
using Intentum.AI.Http;
using JetBrains.Annotations;

namespace Intentum.AI.DeepSeek;

[UsedImplicitly]
public sealed class DeepSeekEmbeddingProvider(DeepSeekOptions options, HttpClient httpClient) : IIntentEmbeddingProvider
{
    public IntentEmbedding Embed(string behaviorKey)
        => EmbedAsync(behaviorKey, CancellationToken.None).GetAwaiter().GetResult();

    public async Task<IntentEmbedding> EmbedAsync(string behaviorKey, CancellationToken cancellationToken = default)
    {
        options.Validate();

        var request = new DeepSeekEmbeddingRequest(options.EmbeddingModel, behaviorKey);

        var response = await EmbeddingHttpRetryHandler.SendWithRetryAsync(
            ct => httpClient.PostAsJsonAsync("embeddings", request, ct),
            (retryAfterSec, body) => throw new DeepSeekRateLimitException((int?)retryAfterSec, body),
            cancellationToken: cancellationToken);

        var payload = await response.Content
            .ReadFromJsonAsync<DeepSeekEmbeddingResponse>(cancellationToken);

        var values = payload?.Data.FirstOrDefault()?.Embedding ?? [];
        var score = EmbeddingScore.Normalize(values);

        return new IntentEmbedding(Source: behaviorKey, Score: score);
    }

    [UsedImplicitly]
    private sealed record DeepSeekEmbeddingRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("input")] string Input);

    private sealed record DeepSeekEmbeddingResponse(
        [property: JsonPropertyName("data")] List<DeepSeekEmbeddingData> Data);

    private sealed record DeepSeekEmbeddingData(
        [property: JsonPropertyName("embedding")] List<double> Embedding);
}
