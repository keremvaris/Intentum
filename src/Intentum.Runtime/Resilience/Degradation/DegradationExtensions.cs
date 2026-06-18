using Intentum.Runtime.Resilience.Degradation;
using Microsoft.Extensions.DependencyInjection;

namespace Intentum.Runtime.Resilience;

public static class DegradationExtensions
{
    public static IServiceCollection AddIntentumDegradationPolicy(
        this IServiceCollection services,
        DegradationOptions? options = null)
    {
        services.AddSingleton<IDegradationPolicy>(
            new MemoryDegradationPolicy(options ?? new DegradationOptions()));
        return services;
    }
}
