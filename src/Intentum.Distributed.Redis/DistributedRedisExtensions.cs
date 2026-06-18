using Intentum.Distributed.Locking;
using Intentum.Distributed.RateLimiting;
using Intentum.Distributed.Redis;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Intentum.Distributed;

public static class DistributedRedisExtensions
{
    public static IServiceCollection AddIntentumDistributedRedis(
        this IServiceCollection services,
        string connectionString)
    {
        var multiplexer = ConnectionMultiplexer.Connect(connectionString);
        services.AddSingleton<IConnectionMultiplexer>(multiplexer);
        services.AddSingleton<IDistributedLock, RedisDistributedLock>();
        services.AddSingleton<IDistributedRateLimiter, RedisDistributedRateLimiter>();
        return services;
    }

    public static IServiceCollection AddIntentumDistributedRedis(
        this IServiceCollection services,
        IConnectionMultiplexer multiplexer)
    {
        services.AddSingleton(multiplexer);
        services.AddSingleton<IDistributedLock, RedisDistributedLock>();
        services.AddSingleton<IDistributedRateLimiter, RedisDistributedRateLimiter>();
        return services;
    }
}
