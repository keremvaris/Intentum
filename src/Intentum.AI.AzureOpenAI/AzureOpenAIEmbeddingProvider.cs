using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Intentum.AI.Embeddings;
using Intentum.AI.Http;
using JetBrains.Annotations;

namespace Intentum.AI.AzureOpenAI;

/// <summary>
/// Azure OpenAI embedding provider. On 429 after retries throws <see cref="AzureOpenAIRateLimitException"/>.
/// Built-in retry for 429 (up to 5 attempts, respects Retry-After).
/// </summary>
public sealed class AzureOpenAIEmbeddingProvider(AzureOpenAIOptions options, HttpClient httpClient)
    : IIntentEmbeddingProvider
{
    /// <inheritdoc />
    public IntentEmbedding Embed(string behaviorKey)
        => EmbedAsync(behaviorKey, CancellationToken.None).GetAwaiter().GetResult();

    /// <inheritdoc />
    public async Task<IntentEmbedding> EmbedAsync(string behaviorKey, CancellationToken cancellationToken = default)
    {
        options.Validate();

        var request = new AzureEmbeddingRequest(behaviorKey);
        var url = $"openai/deployments/{options.EmbeddingDeployment}/embeddings?api-version={options.ApiVersion}";

        var response = await EmbeddingHttpRetryHandler.SendWithRetryAsync(
            ct => httpClient.PostAsJsonAsync(url, request, ct),
            (retryAfterSec, body) => throw new AzureOpenAIRateLimitException(retryAfterSec, body),
            cancellationToken: cancellationToken);

        var payload = await response.Content
            .ReadFromJsonAsync<AzureEmbeddingResponse>(cancellationToken);

        var values = payload?.Data.FirstOrDefault()?.Embedding ?? [];
        var score = EmbeddingScore.Normalize(values);

        return new IntentEmbedding(Source: behaviorKey, Score: score);
    }

    [UsedImplicitly]
    private sealed record AzureEmbeddingRequest(
        [property: JsonPropertyName("input")] string Input);

    private sealed record AzureEmbeddingResponse(
        [property: JsonPropertyName("data")] List<AzureEmbeddingData> Data);

    private sealed record AzureEmbeddingData(
        [property: JsonPropertyName("embedding")] List<double> Embedding);
}
