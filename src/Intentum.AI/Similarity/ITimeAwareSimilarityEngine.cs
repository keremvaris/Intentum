using Intentum.AI.Embeddings;
using Intentum.Core.Behavior;

namespace Intentum.AI.Similarity;

/// <summary>
/// Similarity engine that can use behavior space timestamps (e.g. time decay: recent events weigh more).
/// When used by LlmIntentModel, the model will call this overload so that temporal weighting is applied.
/// </summary>
public interface ITimeAwareSimilarityEngine : IIntentSimilarityEngine
{
    /// <summary>Calculates intent score with time-based weighting (e.g. exponential decay by event age).</summary>
    double CalculateIntentScoreWithTimeDecay(BehaviorSpace behaviorSpace, IReadOnlyCollection<IntentEmbedding> embeddings);
}
