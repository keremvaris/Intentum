using Intentum.Runtime.Resilience.Timeout;
using Microsoft.Extensions.DependencyInjection;

namespace Intentum.Runtime.Resilience;

public static class TimeoutExtensions
{
    public static IServiceCollection AddIntentumTimeoutPolicy(
        this IServiceCollection services,
        TimeoutOptions? options = null)
    {
        services.AddSingleton<ITimeoutPolicy>(
            new MemoryTimeoutPolicy(options ?? new TimeoutOptions()));
        return services;
    }
}
