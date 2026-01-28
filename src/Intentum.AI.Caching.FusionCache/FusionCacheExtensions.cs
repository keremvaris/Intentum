using Intentum.AI.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Intentum.AI.Caching.FusionCache;

/// <summary>
/// Extension methods for integrating FusionCache with Intentum.
/// </summary>
public static class FusionCacheExtensions
{
    /// <summary>
    /// Adds FusionCache as the embedding cache implementation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureFusionCache">Optional action to configure FusionCache.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddIntentumFusionCache(
        this IServiceCollection services,
        Action<FusionCacheOptions>? configureFusionCache = null)
    {
        // Add FusionCache if not already added
        if (!services.Any(s => s.ServiceType == typeof(IFusionCache)))
        {
            var builder = services.AddFusionCache();
            if (configureFusionCache != null)
            {
                builder.WithOptions(configureFusionCache);
            }
        }

        // Register FusionCacheEmbeddingCache
        services.AddSingleton<IEmbeddingCache>(sp =>
        {
            var fusionCache = sp.GetRequiredService<IFusionCache>();
            return new FusionCacheEmbeddingCache(fusionCache);
        });

        return services;
    }

    /// <summary>
    /// Adds FusionCache with Redis as the distributed cache (L2).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="redisConnectionString">Redis connection string.</param>
    /// <param name="configureFusionCache">Optional action to configure FusionCache.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddIntentumFusionCacheWithRedis(
        this IServiceCollection services,
        string redisConnectionString,
        Action<FusionCacheOptions>? configureFusionCache = null)
    {
        // Add Redis distributed cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
        });

        // Configure FusionCache with Redis as L2
        var fusionBuilder = services.AddFusionCache()
            .WithDefaultEntryOptions(new FusionCacheEntryOptions
            {
                Duration = TimeSpan.FromHours(24), // Default 24 hours for embeddings
                Priority = CacheItemPriority.Normal
            })
            .WithDistributedCache();

        if (configureFusionCache != null)
        {
            fusionBuilder.WithOptions(configureFusionCache);
        }

        // Register FusionCacheEmbeddingCache
        services.AddSingleton<IEmbeddingCache>(sp =>
        {
            var fusionCache = sp.GetRequiredService<IFusionCache>();
            return new FusionCacheEmbeddingCache(fusionCache);
        });

        return services;
    }

    /// <summary>
    /// Adds FusionCache with Redis backplane for multi-node synchronization.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="redisConnectionString">Redis connection string.</param>
    /// <param name="configureFusionCache">Optional action to configure FusionCache.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddIntentumFusionCacheWithRedisBackplane(
        this IServiceCollection services,
        string redisConnectionString,
        Action<FusionCacheOptions>? configureFusionCache = null)
    {
        // Add Redis distributed cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
        });

        // Add Redis backplane
        var fusionBuilder = services.AddFusionCache()
            .WithDefaultEntryOptions(new FusionCacheEntryOptions
            {
                Duration = TimeSpan.FromHours(24),
                Priority = CacheItemPriority.Normal
            })
            .WithDistributedCache()
            .WithBackplane(options =>
            {
                options.ConnectionString = redisConnectionString;
            });

        if (configureFusionCache != null)
        {
            fusionBuilder.WithOptions(configureFusionCache);
        }

        // Register FusionCacheEmbeddingCache
        services.AddSingleton<IEmbeddingCache>(sp =>
        {
            var fusionCache = sp.GetRequiredService<IFusionCache>();
            return new FusionCacheEmbeddingCache(fusionCache);
        });

        return services;
    }
}
