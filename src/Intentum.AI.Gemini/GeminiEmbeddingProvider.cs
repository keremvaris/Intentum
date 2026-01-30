using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Intentum.AI.Embeddings;
using JetBrains.Annotations;

namespace Intentum.AI.Gemini;

public sealed class GeminiEmbeddingProvider(GeminiOptions options, HttpClient httpClient) : IIntentEmbeddingProvider
{
    public IntentEmbedding Embed(string behaviorKey)
    {
        options.Validate();

        var request = new GeminiEmbedRequest(
            new GeminiContent([new GeminiPart(behaviorKey)]));

        var url = $"models/{options.EmbeddingModel}:embedContent?key={options.ApiKey}";

        var response = httpClient
            .PostAsJsonAsync(url, request)
            .GetAwaiter()
            .GetResult();

        response.EnsureSuccessStatusCode();

        var payload = response.Content
            .ReadFromJsonAsync<GeminiEmbedResponse>()
            .GetAwaiter()
            .GetResult();

        var values = payload?.Embedding.Values ?? new List<double>();
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
