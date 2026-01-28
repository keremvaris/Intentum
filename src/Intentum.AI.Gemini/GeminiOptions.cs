namespace Intentum.AI.Gemini;

public sealed class GeminiOptions
{
    public required string ApiKey { get; init; }
    public string EmbeddingModel { get; init; } = "text-embedding-004";
}
