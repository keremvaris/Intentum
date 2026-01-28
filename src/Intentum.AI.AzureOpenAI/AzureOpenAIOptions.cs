namespace Intentum.AI.AzureOpenAI;

public sealed class AzureOpenAIOptions
{
    public required string Endpoint { get; init; }
    public required string ApiKey { get; init; }
    public string EmbeddingDeployment { get; init; } = "embedding";
}
