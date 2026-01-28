using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Intentum.AI.Embeddings;

namespace Intentum.AI.AzureOpenAI;

public sealed class AzureOpenAIEmbeddingProvider : IIntentEmbeddingProvider
{
    private readonly AzureOpenAIOptions _options;
    private readonly HttpClient _httpClient;

    public AzureOpenAIEmbeddingProvider(AzureOpenAIOptions options, HttpClient httpClient)
    {
        _options = options;
        _httpClient = httpClient;
    }

    public IntentEmbedding Embed(string behaviorKey)
    {
        _options.Validate();

        var request = new AzureEmbeddingRequest(behaviorKey);
        var url = $"openai/deployments/{_options.EmbeddingDeployment}/embeddings?api-version={_options.ApiVersion}";

        var response = _httpClient
            .PostAsJsonAsync(url, request)
            .GetAwaiter()
            .GetResult();

        response.EnsureSuccessStatusCode();

        var payload = response.Content
            .ReadFromJsonAsync<AzureEmbeddingResponse>()
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

    private sealed record AzureEmbeddingRequest(
        [property: JsonPropertyName("input")] string Input);

    private sealed record AzureEmbeddingResponse(
        [property: JsonPropertyName("data")] List<AzureEmbeddingData> Data);

    private sealed record AzureEmbeddingData(
        [property: JsonPropertyName("embedding")] List<double> Embedding);
}
