using JetBrains.Annotations;

namespace Intentum.AI.Gemini;

public sealed class GeminiOptions
{
    private const string DefaultBaseUrl = "https://generativelanguage.googleapis.com/v1beta/";
    private const string UrlTrailingSlash = "/";

    public required string ApiKey { get; init; }
    public string EmbeddingModel { get; init; } = "text-embedding-004";
    public string? BaseUrl { get; init; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
            throw new ArgumentException("Gemini ApiKey is required.");
        if (string.IsNullOrWhiteSpace(EmbeddingModel))
            throw new ArgumentException("Gemini EmbeddingModel is required.");
        if (string.IsNullOrWhiteSpace(BaseUrl))
            throw new ArgumentException("Gemini BaseUrl is required.");
    }

    [UsedImplicitly]
    public static GeminiOptions FromEnvironment()
    {
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
            ?? throw new InvalidOperationException("GEMINI_API_KEY is not set. Copy .env.example to .env and set GEMINI_API_KEY, or run from repo root so .env is loaded.");

        var baseUrl = Environment.GetEnvironmentVariable("GEMINI_BASE_URL")
            ?? DefaultBaseUrl;

        var normalizedBaseUrl = baseUrl.TrimEnd(UrlTrailingSlash[0]) + UrlTrailingSlash;

        return new GeminiOptions
        {
            ApiKey = apiKey,
            EmbeddingModel = Environment.GetEnvironmentVariable("GEMINI_EMBEDDING_MODEL") ?? "text-embedding-004",
            BaseUrl = normalizedBaseUrl
        };
    }
}
