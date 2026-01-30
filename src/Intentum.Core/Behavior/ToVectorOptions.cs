namespace Intentum.Core.Behavior;

/// <summary>
/// Options for building a behavior vector from observed events (e.g. normalization, caps).
/// </summary>
/// <param name="Normalization">How to normalize dimension values. None = raw counts.</param>
/// <param name="CapPerDimension">Maximum count per dimension (actor:action). 0 = no cap.</param>
public sealed record ToVectorOptions(
    VectorNormalization Normalization = VectorNormalization.None,
    double CapPerDimension = 0
);

/// <summary>
/// Strategy for normalizing dimension values in a behavior vector.
/// </summary>
public enum VectorNormalization
{
    /// <summary>No normalization; use raw counts.</summary>
    None,

    /// <summary>Cap each dimension at a maximum (see CapPerDimension).</summary>
    Cap,

    /// <summary>L1 norm: scale so that the sum of dimension values equals 1.</summary>
    L1,

    /// <summary>Scale each dimension by min(1, value / cap); cap from CapPerDimension.</summary>
    SoftCap
}
