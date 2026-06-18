using Intentum.Runtime.Resilience.CircuitBreaker;
using Microsoft.Extensions.DependencyInjection;

namespace Intentum.Runtime.Resilience;

public static class CircuitBreakerExtensions
{
    public static IServiceCollection AddIntentumCircuitBreaker(
        this IServiceCollection services,
        CircuitBreakerOptions? options = null)
    {
        services.AddSingleton<ICircuitBreaker>(
            new MemoryCircuitBreaker(options ?? new CircuitBreakerOptions()));
        return services;
    }
}
