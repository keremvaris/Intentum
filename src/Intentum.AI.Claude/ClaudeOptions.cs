namespace Intentum.AI.Claude;

public sealed class ClaudeOptions
{
    public required string ApiKey { get; init; }
    public string EmbeddingModel { get; init; } = "claude-embedding-1";
}
