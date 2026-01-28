using Intentum.Core.Behavior;
using Intentum.Core.Intents;

namespace Intentum.Core.Contracts;

/// <summary>
/// Adapter interface for LLM/ML intent inference models.
/// </summary>
public interface IIntentModel
{
    Intent Infer(BehaviorSpace behaviorSpace);
}
