using Intentum.Runtime.Policy;

namespace Intentum.Versioning;

/// <summary>
/// A policy with an associated version for tracking and rollback.
/// </summary>
public interface IVersionedPolicy
{
    /// <summary>
    /// Version identifier (e.g. "1.0", "2024-01-15").
    /// </summary>
    string Version { get; }

    /// <summary>
    /// The underlying policy.
    /// </summary>
    IntentPolicy Policy { get; }
}
