namespace Intentum.AI.Mistral;

public sealed class MistralOptions
{
    public required string ApiKey { get; init; }
    public string EmbeddingModel { get; init; } = "mistral-embed";
}
