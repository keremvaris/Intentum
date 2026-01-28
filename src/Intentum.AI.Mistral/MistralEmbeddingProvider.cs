using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Intentum.AI.Embeddings;

namespace Intentum.AI.Mistral;

public sealed class MistralEmbeddingProvider : IIntentEmbeddingProvider
{
    private readonly MistralOptions _options;
    private readonly HttpClient _httpClient;

    public MistralEmbeddingProvider(MistralOptions options, HttpClient httpClient)
    {
        _options = options;
        _httpClient = httpClient;
    }

    public IntentEmbedding Embed(string behaviorKey)
    {
        _options.Validate();

        var request = new MistralEmbeddingRequest(_options.EmbeddingModel, [behaviorKey]);

        var response = _httpClient
            .PostAsJsonAsync("embeddings", request)
            .GetAwaiter()
            .GetResult();

        response.EnsureSuccessStatusCode();

        var payload = response.Content
            .ReadFromJsonAsync<MistralEmbeddingResponse>()
            .GetAwaiter()
            .GetResult();

        var values = payload?.Data?.FirstOrDefault()?.Embedding ?? new List<double>();
        var score = Normalize(values);

        return new IntentEmbedding(
            Source: behaviorKey,
            Score: score
        );
    }

    private static double Normalize(IReadOnlyList<double> values)
    {
        if (values.Count == 0)
            return 0;

        var avgAbs = values.Average(v => Math.Abs(v));
        return Math.Clamp(avgAbs, 0.0, 1.0);
    }

    private sealed record MistralEmbeddingRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("input")] IReadOnlyList<string> Input);

    private sealed record MistralEmbeddingResponse(
        [property: JsonPropertyName("data")] List<MistralEmbeddingData> Data);

    private sealed record MistralEmbeddingData(
        [property: JsonPropertyName("embedding")] List<double> Embedding);
}
