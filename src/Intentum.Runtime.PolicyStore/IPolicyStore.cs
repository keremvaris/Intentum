using Intentum.Runtime.Policy;
using Intentum.Versioning;

namespace Intentum.Runtime.PolicyStore;

/// <summary>
/// Loads intent policies from an external store (file, database, config server).
/// Supports versioning for rollback and comparison.
/// </summary>
public interface IPolicyStore
{
    /// <summary>
    /// Loads the current (active) policy.
    /// </summary>
    Task<IntentPolicy> LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific version of the policy, if available.
    /// </summary>
    Task<VersionedPolicy?> GetVersionAsync(string version, CancellationToken cancellationToken = default);
}
