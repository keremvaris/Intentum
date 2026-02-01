using Intentum.Core.Behavior;

namespace Intentum.Core.Pipeline;

/// <summary>
/// Default signal-to-vector step: delegates to BehaviorSpace.ToVector().
/// </summary>
public sealed class DefaultSignalToVector : ISignalToVector
{
    /// <inheritdoc />
    public BehaviorVector ToVector(BehaviorSpace behaviorSpace, ToVectorOptions? options = null)
        => behaviorSpace.ToVector(options);
}
