using Microsoft.Extensions.DependencyInjection;

namespace Intentum.Runtime.PolicyStore;

/// <summary>
/// Extension methods for registering policy store.
/// </summary>
public static class PolicyStoreExtensions
{
    /// <summary>
    /// Registers a file-based policy store. Policy is loaded from the given path (default: intent-policy.json in app base directory).
    /// </summary>
    public static IServiceCollection AddFilePolicyStore(this IServiceCollection services, string? filePath = null)
    {
        services.AddSingleton<IPolicyStore>(_ => new FilePolicyStore(filePath));
        return services;
    }
}
