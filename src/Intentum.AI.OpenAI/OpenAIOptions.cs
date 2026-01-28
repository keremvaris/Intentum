namespace Intentum.AI.OpenAI;

public sealed class OpenAIOptions
{
    public required string ApiKey { get; init; }
    public string EmbeddingModel { get; init; } = "text-embedding-3-large";
}
