namespace Intentum.AI.Embeddings;

public interface IIntentEmbeddingProvider
{
    IntentEmbedding Embed(string behaviorKey);

    Task<IntentEmbedding> EmbedAsync(string behaviorKey, CancellationToken cancellationToken = default)
        => Task.FromResult(Embed(behaviorKey));
}
