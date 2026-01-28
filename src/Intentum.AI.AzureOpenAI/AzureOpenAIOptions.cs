namespace Intentum.AI.AzureOpenAI;

public sealed class AzureOpenAIOptions
{
    public required string Endpoint { get; init; }
    public required string ApiKey { get; init; }
    public string EmbeddingDeployment { get; init; } = "embedding";
    public string ApiVersion { get; init; } = "2023-05-15";

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Endpoint))
            throw new ArgumentException("AzureOpenAI Endpoint is required.");
        if (string.IsNullOrWhiteSpace(ApiKey))
            throw new ArgumentException("AzureOpenAI ApiKey is required.");
        if (string.IsNullOrWhiteSpace(EmbeddingDeployment))
            throw new ArgumentException("AzureOpenAI EmbeddingDeployment is required.");
        if (string.IsNullOrWhiteSpace(ApiVersion))
            throw new ArgumentException("AzureOpenAI ApiVersion is required.");
    }

    public static AzureOpenAIOptions FromEnvironment()
    {
        var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
            ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
        var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")
            ?? throw new InvalidOperationException("AZURE_OPENAI_API_KEY is not set.");

        return new AzureOpenAIOptions
        {
            Endpoint = endpoint,
            ApiKey = apiKey,
            EmbeddingDeployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_EMBEDDING_DEPLOYMENT") ?? "embedding",
            ApiVersion = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_VERSION") ?? "2023-05-15"
        };
    }
}
