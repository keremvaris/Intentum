using Intentum.Runtime.Resilience.Bulkhead;
using Microsoft.Extensions.DependencyInjection;

namespace Intentum.Runtime.Resilience;

public static class BulkheadExtensions
{
    public static IServiceCollection AddIntentumBulkhead(
        this IServiceCollection services,
        BulkheadOptions? options = null)
    {
        services.AddSingleton<IBulkhead>(
            new MemoryBulkhead(options ?? new BulkheadOptions()));
        return services;
    }
}
