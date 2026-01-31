using JetBrains.Annotations;

namespace Intentum.AI.Mistral;

public sealed class MistralOptions
{
    private const string DefaultBaseUrl = "https://api.mistral.ai/v1/";
    private const string UrlTrailingSlash = "/";

    public required string ApiKey { get; init; }
    public string EmbeddingModel { get; init; } = "mistral-embed";
    public string? BaseUrl { get; init; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
            throw new ArgumentException("Mistral ApiKey is required.");
        if (string.IsNullOrWhiteSpace(EmbeddingModel))
            throw new ArgumentException("Mistral EmbeddingModel is required.");
        if (string.IsNullOrWhiteSpace(BaseUrl))
            throw new ArgumentException("Mistral BaseUrl is required.");
    }

    [UsedImplicitly]
    public static MistralOptions FromEnvironment()
    {
        var apiKey = Environment.GetEnvironmentVariable("MISTRAL_API_KEY")
            ?? throw new InvalidOperationException("MISTRAL_API_KEY is not set. Copy .env.example to .env and set MISTRAL_API_KEY, or run from repo root so .env is loaded.");

        var baseUrl = Environment.GetEnvironmentVariable("MISTRAL_BASE_URL")
            ?? DefaultBaseUrl;

        var normalizedBaseUrl = baseUrl.TrimEnd(UrlTrailingSlash[0]) + UrlTrailingSlash;

        return new MistralOptions
        {
            ApiKey = apiKey,
            EmbeddingModel = Environment.GetEnvironmentVariable("MISTRAL_EMBEDDING_MODEL") ?? "mistral-embed",
            BaseUrl = normalizedBaseUrl
        };
    }
}
