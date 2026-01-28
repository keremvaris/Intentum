namespace Intentum.AI.Embeddings;

public interface IIntentEmbeddingProvider
{
    IntentEmbedding Embed(string behaviorKey);
}
