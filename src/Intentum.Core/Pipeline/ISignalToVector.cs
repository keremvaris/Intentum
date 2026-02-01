using Intentum.Core.Behavior;

namespace Intentum.Core.Pipeline;

/// <summary>
/// Converts observed behavior (signals) into a numeric vector.
/// Override this step to customize vectorization (e.g. different normalization, windowing).
/// </summary>
public interface ISignalToVector
{
    /// <summary>Builds a behavior vector from the observed behavior space.</summary>
    /// <param name="behaviorSpace">The observed behavior space.</param>
    /// <param name="options">Optional normalization/cap options; when null, default vectorization is used.</param>
    BehaviorVector ToVector(BehaviorSpace behaviorSpace, ToVectorOptions? options = null);
}
