using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Intentum.AI.Embeddings;
using JetBrains.Annotations;

namespace Intentum.AI.AzureOpenAI;

public sealed class AzureOpenAIEmbeddingProvider(AzureOpenAIOptions options, HttpClient httpClient)
    : IIntentEmbeddingProvider
{
    public IntentEmbedding Embed(string behaviorKey)
    {
        options.Validate();

        var request = new AzureEmbeddingRequest(behaviorKey);
        var url = $"openai/deployments/{options.EmbeddingDeployment}/embeddings?api-version={options.ApiVersion}";

        var response = httpClient
            .PostAsJsonAsync(url, request)
            .GetAwaiter()
            .GetResult();

        response.EnsureSuccessStatusCode();

        var payload = response.Content
            .ReadFromJsonAsync<AzureEmbeddingResponse>()
            .GetAwaiter()
            .GetResult();

        var values = payload?.Data.FirstOrDefault()?.Embedding ?? new List<double>();
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

        var avgAbs = values.Average(Math.Abs);
        return Math.Clamp(avgAbs, 0.0, 1.0);
    }

    [UsedImplicitly]
    private sealed record AzureEmbeddingRequest(
        [property: JsonPropertyName("input")] string Input);

    private sealed record AzureEmbeddingResponse(
        [property: JsonPropertyName("data")] List<AzureEmbeddingData> Data);

    private sealed record AzureEmbeddingData(
        [property: JsonPropertyName("embedding")] List<double> Embedding);
}
