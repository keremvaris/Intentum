namespace Intentum.MultiTenancy;

/// <summary>
/// Provides the current tenant identifier for multi-tenant isolation.
/// </summary>
public interface ITenantProvider
{
    /// <summary>
    /// Gets the current tenant identifier (e.g. from HTTP context, claims, or ambient context).
    /// </summary>
    string? GetCurrentTenantId();
}
