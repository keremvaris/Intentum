namespace Intentum.Core.Behavior;

/// <summary>
/// Numeric representation of observed behaviors.
/// </summary>
public sealed record BehaviorVector(
    IReadOnlyDictionary<string, double> Dimensions
);
