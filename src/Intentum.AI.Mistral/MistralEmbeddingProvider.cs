using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Intentum.AI.Embeddings;
using JetBrains.Annotations;

namespace Intentum.AI.Mistral;

public sealed class MistralEmbeddingProvider(MistralOptions options, HttpClient httpClient) : IIntentEmbeddingProvider
{
    public IntentEmbedding Embed(string behaviorKey)
    {
        options.Validate();

        var request = new MistralEmbeddingRequest(options.EmbeddingModel, [behaviorKey]);

        var response = httpClient
            .PostAsJsonAsync("embeddings", request)
            .GetAwaiter()
            .GetResult();

        response.EnsureSuccessStatusCode();

        var payload = response.Content
            .ReadFromJsonAsync<MistralEmbeddingResponse>()
            .GetAwaiter()
            .GetResult();

        var values = payload?.Data.FirstOrDefault()?.Embedding ?? new List<double>();
        var score = EmbeddingScore.Normalize(values);

        return new IntentEmbedding(
            Source: behaviorKey,
            Score: score
        );
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
