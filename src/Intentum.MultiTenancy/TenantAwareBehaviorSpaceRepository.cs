using Intentum.Core.Behavior;
using Intentum.Persistence.Repositories;

namespace Intentum.MultiTenancy;

/// <summary>
/// Behavior space repository that scopes all operations to the current tenant.
/// Injects TenantId into metadata on save and filters by tenant on read.
/// </summary>
public sealed class TenantAwareBehaviorSpaceRepository : IBehaviorSpaceRepository
{
    private const string TenantIdKey = "TenantId";

    private readonly IBehaviorSpaceRepository _inner;
    private readonly ITenantProvider _tenantProvider;

    public TenantAwareBehaviorSpaceRepository(IBehaviorSpaceRepository inner, ITenantProvider tenantProvider)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _tenantProvider = tenantProvider ?? throw new ArgumentNullException(nameof(tenantProvider));
    }

    /// <inheritdoc />
    public async Task<string> SaveAsync(BehaviorSpace behaviorSpace, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();
        if (!string.IsNullOrEmpty(tenantId))
            behaviorSpace.SetMetadata(TenantIdKey, tenantId);
        return await _inner.SaveAsync(behaviorSpace, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<BehaviorSpace?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var space = await _inner.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        return IsCurrentTenant(space) ? space : null;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BehaviorSpace>> GetByMetadataAsync(
        string key,
        object value,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();
        if (string.IsNullOrEmpty(tenantId))
            return await _inner.GetByMetadataAsync(key, value, cancellationToken).ConfigureAwait(false);

        var byTenant = await _inner.GetByMetadataAsync(TenantIdKey, tenantId, cancellationToken).ConfigureAwait(false);
        return byTenant
            .Where(s => s.Metadata.TryGetValue(key, out var v) && Equals(v, value))
            .ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BehaviorSpace>> GetByTimeWindowAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();
        if (string.IsNullOrEmpty(tenantId))
            return await _inner.GetByTimeWindowAsync(start, end, cancellationToken).ConfigureAwait(false);

        var byTenant = await _inner.GetByMetadataAsync(TenantIdKey, tenantId, cancellationToken).ConfigureAwait(false);
        return byTenant
            .Where(s => s.Events.Count > 0 && s.Events.Min(e => e.OccurredAt) <= end && s.Events.Max(e => e.OccurredAt) >= start)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var space = await _inner.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (space is null || !IsCurrentTenant(space))
            return false;
        return await _inner.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
    }

    private bool IsCurrentTenant(BehaviorSpace? space)
    {
        if (space is null) return false;
        var tenantId = _tenantProvider.GetCurrentTenantId();
        if (string.IsNullOrEmpty(tenantId)) return true;
        return space.Metadata.TryGetValue(TenantIdKey, out var v) && Equals(v, tenantId);
    }
}
