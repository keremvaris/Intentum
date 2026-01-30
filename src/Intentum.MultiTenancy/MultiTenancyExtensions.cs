using Intentum.Persistence.Repositories;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Intentum.MultiTenancy;

/// <summary>
/// Extension methods for registering multi-tenancy with dependency injection.
/// </summary>
[UsedImplicitly]
public static class MultiTenancyExtensions
{
    /// <summary>
    /// Registers tenant-aware behavior space repository. Inject <see cref="TenantAwareBehaviorSpaceRepository"/> when tenant scope is needed; <see cref="IBehaviorSpaceRepository"/> remains the inner (non-tenant) repo.
    /// Requires <see cref="IBehaviorSpaceRepository"/> and <see cref="ITenantProvider"/> to be registered.
    /// </summary>
    [UsedImplicitly]
    public static IServiceCollection AddTenantAwareBehaviorSpaceRepository(this IServiceCollection services)
    {
        services.AddScoped<TenantAwareBehaviorSpaceRepository>(sp =>
        {
            var inner = sp.GetRequiredService<IBehaviorSpaceRepository>();
            var tenantProvider = sp.GetRequiredService<ITenantProvider>();
            return new TenantAwareBehaviorSpaceRepository(inner, tenantProvider);
        });
        return services;
    }
}
