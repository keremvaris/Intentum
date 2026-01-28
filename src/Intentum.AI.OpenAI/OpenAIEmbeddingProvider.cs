using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Intentum.AI.Embeddings;

namespace Intentum.AI.OpenAI;

public sealed class OpenAIEmbeddingProvider : IIntentEmbeddingProvider
{
    private readonly OpenAIOptions _options;
    private readonly HttpClient _httpClient;

    public OpenAIEmbeddingProvider(OpenAIOptions options, HttpClient httpClient)
    {
        _options = options;
        _httpClient = httpClient;
    }

    public IntentEmbedding Embed(string behaviorKey)
    {
        _options.Validate();

        var request = new OpenAIEmbeddingRequest(_options.EmbeddingModel, behaviorKey);
        var response = _httpClient
            .PostAsJsonAsync("embeddings", request)
            .GetAwaiter()
            .GetResult();

        response.EnsureSuccessStatusCode();

        var payload = response.Content
            .ReadFromJsonAsync<OpenAIEmbeddingResponse>()
            .GetAwaiter()
            .GetResult();

        var values = payload?.Data?.FirstOrDefault()?.Embedding ?? new List<double>();
        var score = Normalize(values);

        return new IntentEmbedding(
            Source: behaviorKey,
            Score: score
        );
    }

    private static double Normalize(List<double> values)
    {
        if (values.Count == 0)
            return 0;

        var avgAbs = values.Average(v => Math.Abs(v));
        return Math.Clamp(avgAbs, 0.0, 1.0);
    }

    private sealed record OpenAIEmbeddingRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("input")] string Input);

    private sealed record OpenAIEmbeddingResponse(
        [property: JsonPropertyName("data")] List<OpenAIEmbeddingData> Data);

    private sealed record OpenAIEmbeddingData(
        [property: JsonPropertyName("embedding")] List<double> Embedding);
}
