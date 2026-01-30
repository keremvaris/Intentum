using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Intentum.Clustering;

/// <summary>
/// Extension methods for registering intent clustering with dependency injection.
/// </summary>
[UsedImplicitly]
public static class ClusteringExtensions
{
    /// <summary>
    /// Adds intent clustering to the service collection.
    /// </summary>
    [UsedImplicitly]
    public static IServiceCollection AddIntentClustering(this IServiceCollection services)
    {
        services.AddScoped<IIntentClusterer, IntentClusterer>();
        return services;
    }
}
