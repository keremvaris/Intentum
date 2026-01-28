using Intentum.Runtime.Policy;

namespace Intentum.Versioning;

/// <summary>
/// Wraps an intent policy with a version identifier.
/// </summary>
/// <param name="Version">Version identifier.</param>
/// <param name="Policy">The policy.</param>
public sealed record VersionedPolicy(string Version, IntentPolicy Policy) : IVersionedPolicy
{
    /// <inheritdoc />
    IntentPolicy IVersionedPolicy.Policy => Policy;
}
