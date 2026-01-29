using Intentum.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Intentum.Persistence.Redis;

/// <summary>
/// Extension methods for Redis persistence.
/// </summary>
public static class PersistenceRedisExtensions
{
    /// <summary>
    /// Adds Redis persistence for Intentum (behavior spaces and intent history).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="redis">The Redis connection multiplexer.</param>
    /// <param name="keyPrefix">Optional key prefix (default: "intentum:"). Behavior spaces use "{keyPrefix}behaviorspace:", intent history uses "{keyPrefix}inthistory:".</param>
    public static IServiceCollection AddIntentumPersistenceRedis(
        this IServiceCollection services,
        IConnectionMultiplexer redis,
        string keyPrefix = "intentum:")
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        if (redis == null)
            throw new ArgumentNullException(nameof(redis));

        var bsPrefix = keyPrefix.TrimEnd(':') + ":behaviorspace:";
        var ihPrefix = keyPrefix.TrimEnd(':') + ":inthistory:";
        services.AddSingleton<IBehaviorSpaceRepository>(sp => new RedisBehaviorSpaceRepository(redis, bsPrefix));
        services.AddSingleton<IIntentHistoryRepository>(sp => new RedisIntentHistoryRepository(redis, ihPrefix));
        return services;
    }
}
