using Intentum.Runtime.Resilience.Retry;
using Microsoft.Extensions.DependencyInjection;

namespace Intentum.Runtime.Resilience;

public static class RetryExtensions
{
    public static IServiceCollection AddIntentumRetryPolicy(
        this IServiceCollection services,
        RetryOptions? options = null)
    {
        services.AddSingleton<IRetryPolicy>(
            new MemoryRetryPolicy(options ?? new RetryOptions()));
        return services;
    }
}
