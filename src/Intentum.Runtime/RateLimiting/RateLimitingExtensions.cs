using Microsoft.Extensions.DependencyInjection;

namespace Intentum.Runtime.RateLimiting;

/// <summary>
/// Extension methods for registering rate limiting services.
/// </summary>
public static class RateLimitingExtensions
{
    /// <summary>
    /// Adds Intentum rate limiting services with default MemoryRateLimiter.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddIntentumRateLimiting(this IServiceCollection services)
    {
        services.AddSingleton<IRateLimiter, MemoryRateLimiter>();
        return services;
    }
}
