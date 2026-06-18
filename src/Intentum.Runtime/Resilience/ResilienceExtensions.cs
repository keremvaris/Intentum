using Intentum.Runtime.Resilience.Bulkhead;
using Intentum.Runtime.Resilience.CircuitBreaker;
using Intentum.Runtime.Resilience.Degradation;
using Intentum.Runtime.Resilience.Retry;
using Intentum.Runtime.Resilience.Timeout;
using Microsoft.Extensions.DependencyInjection;

namespace Intentum.Runtime.Resilience;

public static class ResilienceExtensions
{
    public static IServiceCollection AddIntentumResilience(
        this IServiceCollection services)
    {
        services.AddIntentumCircuitBreaker();
        services.AddIntentumRetryPolicy();
        services.AddIntentumBulkhead();
        services.AddIntentumDegradationPolicy();
        services.AddIntentumTimeoutPolicy();
        return services;
    }

    public static IServiceCollection AddIntentumResilience(
        this IServiceCollection services,
        CircuitBreakerOptions? circuitBreaker = null,
        RetryOptions? retry = null,
        BulkheadOptions? bulkhead = null,
        DegradationOptions? degradation = null,
        TimeoutOptions? timeout = null)
    {
        services.AddIntentumCircuitBreaker(circuitBreaker);
        services.AddIntentumRetryPolicy(retry);
        services.AddIntentumBulkhead(bulkhead);
        services.AddIntentumDegradationPolicy(degradation);
        services.AddIntentumTimeoutPolicy(timeout);
        return services;
    }
}
