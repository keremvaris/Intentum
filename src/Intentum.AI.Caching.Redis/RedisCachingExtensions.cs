using Intentum.AI.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace Intentum.AI.Caching.Redis;

/// <summary>
/// Extension methods for registering Redis embedding cache with dependency injection.
/// </summary>
public static class RedisCachingExtensions
{
    /// <summary>
    /// Adds Redis-backed distributed embedding cache to the service collection.
    /// Configures StackExchangeRedisCache and registers RedisEmbeddingCache as IEmbeddingCache.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to configure Redis cache options (connection string, instance name, etc.).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddIntentumRedisCache(
        this IServiceCollection services,
        Action<IntentumRedisCacheOptions>? configure = null)
    {
        var options = new IntentumRedisCacheOptions();
        configure?.Invoke(options);

        services.AddStackExchangeRedisCache(redisOptions =>
        {
            redisOptions.Configuration = options.ConnectionString;
            redisOptions.InstanceName = options.InstanceName ?? "Intentum:";
        });

        services.AddSingleton<IEmbeddingCache>(sp =>
        {
            var cache = sp.GetRequiredService<IDistributedCache>();
            var entryOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = options.DefaultExpiration
            };
            return new RedisEmbeddingCache(cache, entryOptions);
        });

        return services;
    }
}

/// <summary>
/// Options for Intentum Redis embedding cache.
/// </summary>
public sealed class IntentumRedisCacheOptions
{
    /// <summary>
    /// Redis connection string (e.g. "localhost:6379" or "contoso.redis.cache.windows.net:6380,password=...").
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// Optional instance name prefix for keys (default "Intentum:").
    /// </summary>
    public string? InstanceName { get; set; }

    /// <summary>
    /// Default expiration for cached embeddings (default 24 hours).
    /// </summary>
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromHours(24);
}
