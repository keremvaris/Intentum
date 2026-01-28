namespace Intentum.AI.Mistral;

public sealed class MistralOptions
{
    public required string ApiKey { get; init; }
    public string EmbeddingModel { get; init; } = "mistral-embed";
    public string BaseUrl { get; init; } = "https://api.mistral.ai/v1/";

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
            throw new ArgumentException("Mistral ApiKey is required.", nameof(ApiKey));
        if (string.IsNullOrWhiteSpace(EmbeddingModel))
            throw new ArgumentException("Mistral EmbeddingModel is required.", nameof(EmbeddingModel));
        if (string.IsNullOrWhiteSpace(BaseUrl))
            throw new ArgumentException("Mistral BaseUrl is required.", nameof(BaseUrl));
    }

    public static MistralOptions FromEnvironment()
    {
        var apiKey = Environment.GetEnvironmentVariable("MISTRAL_API_KEY")
            ?? throw new InvalidOperationException("MISTRAL_API_KEY is not set.");

        return new MistralOptions
        {
            ApiKey = apiKey,
            EmbeddingModel = Environment.GetEnvironmentVariable("MISTRAL_EMBEDDING_MODEL") ?? "mistral-embed",
            BaseUrl = Environment.GetEnvironmentVariable("MISTRAL_BASE_URL") ?? "https://api.mistral.ai/v1/"
        };
    }
}
