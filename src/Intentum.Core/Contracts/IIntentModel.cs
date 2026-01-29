using Intentum.Core.Behavior;
using Intentum.Core.Intents;

namespace Intentum.Core.Contracts;

/// <summary>
/// Adapter interface for LLM/ML intent inference models.
/// </summary>
public interface IIntentModel
{
    /// <summary>Infers intent and confidence from the observed behavior space.</summary>
    Intent Infer(BehaviorSpace behaviorSpace);
}
