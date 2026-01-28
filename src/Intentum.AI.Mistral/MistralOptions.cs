namespace Intentum.AI.Mistral;

public sealed class MistralOptions
{
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

    public static MistralOptions FromEnvironment()
    {
        var apiKey = Environment.GetEnvironmentVariable("MISTRAL_API_KEY")
            ?? throw new InvalidOperationException("MISTRAL_API_KEY is not set.");

        var baseUrl = Environment.GetEnvironmentVariable("MISTRAL_BASE_URL");
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException("MISTRAL_BASE_URL is not set.");

        return new MistralOptions
        {
            ApiKey = apiKey,
            EmbeddingModel = Environment.GetEnvironmentVariable("MISTRAL_EMBEDDING_MODEL") ?? "mistral-embed",
            BaseUrl = baseUrl
        };
    }
}
