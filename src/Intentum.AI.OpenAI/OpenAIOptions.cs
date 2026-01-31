namespace Intentum.AI.OpenAI;

public sealed class OpenAIOptions
{
    private const string DefaultBaseUrl = "https://api.openai.com/v1/";
    private const string UrlTrailingSlash = "/";

    public required string ApiKey { get; init; }
    public string EmbeddingModel { get; init; } = "text-embedding-3-large";
    public string? BaseUrl { get; init; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
            throw new ArgumentException("OpenAI ApiKey is required.");
        if (string.IsNullOrWhiteSpace(EmbeddingModel))
            throw new ArgumentException("OpenAI EmbeddingModel is required.");
        if (string.IsNullOrWhiteSpace(BaseUrl))
            throw new ArgumentException("OpenAI BaseUrl is required.");
    }

    public static OpenAIOptions FromEnvironment()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
            ?? throw new InvalidOperationException("OPENAI_API_KEY is not set. Copy .env.example to .env and set OPENAI_API_KEY, or run from repo root so .env is loaded.");

        var baseUrl = Environment.GetEnvironmentVariable("OPENAI_BASE_URL")
            ?? DefaultBaseUrl;

        var normalizedBaseUrl = baseUrl.TrimEnd(UrlTrailingSlash[0]) + UrlTrailingSlash;

        return new OpenAIOptions
        {
            ApiKey = apiKey,
            EmbeddingModel = Environment.GetEnvironmentVariable("OPENAI_EMBEDDING_MODEL") ?? "text-embedding-3-large",
            BaseUrl = normalizedBaseUrl
        };
    }
}
