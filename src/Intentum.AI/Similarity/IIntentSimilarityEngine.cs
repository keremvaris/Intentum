using Intentum.AI.Embeddings;

namespace Intentum.AI.Similarity;

public interface IIntentSimilarityEngine
{
    double CalculateIntentScore(IReadOnlyCollection<IntentEmbedding> embeddings);
}
