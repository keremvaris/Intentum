namespace Intentum.AI.Gemini;

public sealed class GeminiOptions
{
    public const string DefaultBaseUrl = "https://generativelanguage.googleapis.com/v1beta/";

    public required string ApiKey { get; init; }
    public string EmbeddingModel { get; init; } = "text-embedding-004";
    public string BaseUrl { get; init; } = DefaultBaseUrl;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
            throw new ArgumentException("Gemini ApiKey is required.");
        if (string.IsNullOrWhiteSpace(EmbeddingModel))
            throw new ArgumentException("Gemini EmbeddingModel is required.");
        if (string.IsNullOrWhiteSpace(BaseUrl))
            throw new ArgumentException("Gemini BaseUrl is required.");
    }

    public static GeminiOptions FromEnvironment()
    {
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
            ?? throw new InvalidOperationException("GEMINI_API_KEY is not set.");

        return new GeminiOptions
        {
            ApiKey = apiKey,
            EmbeddingModel = Environment.GetEnvironmentVariable("GEMINI_EMBEDDING_MODEL") ?? "text-embedding-004",
            BaseUrl = Environment.GetEnvironmentVariable("GEMINI_BASE_URL") ?? DefaultBaseUrl
        };
    }
}
