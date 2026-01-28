namespace Intentum.AI.OpenAI;

public sealed class OpenAIOptions
{
    public const string DefaultBaseUrl = "https://api.openai.com/v1/";

    public required string ApiKey { get; init; }
    public string EmbeddingModel { get; init; } = "text-embedding-3-large";
    public string BaseUrl { get; init; } = DefaultBaseUrl;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
            throw new ArgumentException("OpenAI ApiKey is required.", nameof(ApiKey));
        if (string.IsNullOrWhiteSpace(EmbeddingModel))
            throw new ArgumentException("OpenAI EmbeddingModel is required.", nameof(EmbeddingModel));
        if (string.IsNullOrWhiteSpace(BaseUrl))
            throw new ArgumentException("OpenAI BaseUrl is required.", nameof(BaseUrl));
    }

    public static OpenAIOptions FromEnvironment()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
            ?? throw new InvalidOperationException("OPENAI_API_KEY is not set.");

        return new OpenAIOptions
        {
            ApiKey = apiKey,
            EmbeddingModel = Environment.GetEnvironmentVariable("OPENAI_EMBEDDING_MODEL") ?? "text-embedding-3-large",
            BaseUrl = Environment.GetEnvironmentVariable("OPENAI_BASE_URL") ?? DefaultBaseUrl
        };
    }
}
