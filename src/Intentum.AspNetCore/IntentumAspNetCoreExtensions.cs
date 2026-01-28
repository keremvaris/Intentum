using Intentum.AI.Embeddings;
using Intentum.Core.Behavior;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Intentum.AspNetCore;

/// <summary>
/// Extension methods for integrating Intentum with ASP.NET Core.
/// </summary>
public static class IntentumAspNetCoreExtensions
{
    /// <summary>
    /// Adds Intentum services to the service collection.
    /// </summary>
    public static IServiceCollection AddIntentum(this IServiceCollection services)
    {
        services.AddSingleton<BehaviorSpace>();
        return services;
    }

    /// <summary>
    /// Adds Intentum services with a custom BehaviorSpace instance.
    /// </summary>
    public static IServiceCollection AddIntentum(this IServiceCollection services, BehaviorSpace behaviorSpace)
    {
        services.AddSingleton(behaviorSpace);
        return services;
    }

    /// <summary>
    /// Uses Intentum behavior observation middleware.
    /// </summary>
    public static IApplicationBuilder UseIntentumBehaviorObservation(
        this IApplicationBuilder app,
        BehaviorObservationOptions? options = null)
    {
        var behaviorSpace = app.ApplicationServices.GetRequiredService<BehaviorSpace>();
        return app.UseMiddleware<BehaviorObservationMiddleware>(behaviorSpace, options);
    }

    /// <summary>
    /// Adds Intentum health checks.
    /// </summary>
    public static IHealthChecksBuilder AddIntentumHealthChecks(
        this IServiceCollection services)
    {
        services.AddSingleton<HealthChecks.EmbeddingProviderHealthCheck>();
        services.AddSingleton<HealthChecks.PolicyEngineHealthCheck>();
        
        return services.AddHealthChecks()
            .AddCheck<HealthChecks.EmbeddingProviderHealthCheck>(
                "embedding-provider",
                tags: new[] { "intentum", "embedding" })
            .AddCheck<HealthChecks.PolicyEngineHealthCheck>(
                "policy-engine",
                tags: new[] { "intentum", "policy" });
    }
}
