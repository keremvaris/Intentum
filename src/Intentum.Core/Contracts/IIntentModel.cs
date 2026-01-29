using Intentum.Core.Behavior;
using Intentum.Core.Intents;

namespace Intentum.Core.Contracts;

/// <summary>
/// Adapter interface for LLM/ML intent inference models.
/// </summary>
public interface IIntentModel
{
    /// <summary>Infers intent and confidence from the observed behavior space. Pass a pre-computed vector to avoid recomputation.</summary>
    /// <param name="behaviorSpace">The observed behavior space.</param>
    /// <param name="precomputedVector">Optional pre-computed behavior vector (e.g. from persistence); when null, computed from behaviorSpace.</param>
    Intent Infer(BehaviorSpace behaviorSpace, BehaviorVector? precomputedVector = null);
}
