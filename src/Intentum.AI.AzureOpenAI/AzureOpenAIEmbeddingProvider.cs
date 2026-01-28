using Intentum.AI.Embeddings;

namespace Intentum.AI.AzureOpenAI;

public sealed class AzureOpenAIEmbeddingProvider : IIntentEmbeddingProvider
{
    private readonly AzureOpenAIOptions _options;

    public AzureOpenAIEmbeddingProvider(AzureOpenAIOptions options)
    {
        _options = options;
    }

    public IntentEmbedding Embed(string behaviorKey)
    {
        var hash = behaviorKey.GetHashCode();
        var normalized = Math.Abs(hash % 100) / 100.0;

        return new IntentEmbedding(
            Source: behaviorKey,
            Score: normalized
        );
    }
}
