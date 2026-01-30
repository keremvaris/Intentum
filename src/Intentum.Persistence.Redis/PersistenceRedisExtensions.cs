using Intentum.Persistence.Repositories;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Intentum.Persistence.Redis;

/// <summary>
/// Extension methods for Redis persistence.
/// </summary>
[UsedImplicitly]
public static class PersistenceRedisExtensions
{
    /// <summary>
    /// Adds Redis persistence for Intentum (behavior spaces and intent history).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="redis">The Redis connection multiplexer.</param>
    /// <param name="keyPrefix">Optional key prefix (default: "intentum:"). Behavior spaces use "{keyPrefix}behaviorspace:", intent history uses "{keyPrefix}inthistory:".</param>
    [UsedImplicitly]
    public static IServiceCollection AddIntentumPersistenceRedis(
        this IServiceCollection services,
        IConnectionMultiplexer redis,
        string keyPrefix = "intentum:")
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(redis);

        var bsPrefix = keyPrefix.TrimEnd(':') + ":behaviorspace:";
        var ihPrefix = keyPrefix.TrimEnd(':') + ":inthistory:";
        services.AddSingleton<IBehaviorSpaceRepository>(_ => new RedisBehaviorSpaceRepository(redis, bsPrefix));
        services.AddSingleton<IIntentHistoryRepository>(_ => new RedisIntentHistoryRepository(redis, ihPrefix));
        return services;
    }
}
