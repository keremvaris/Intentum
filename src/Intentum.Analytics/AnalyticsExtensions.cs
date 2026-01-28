using Intentum.Analytics;
using Microsoft.Extensions.DependencyInjection;

namespace Intentum.Analytics;

/// <summary>
/// Extension methods for registering Intent Analytics.
/// </summary>
public static class AnalyticsExtensions
{
    /// <summary>
    /// Adds Intent Analytics to the service collection.
    /// </summary>
    public static IServiceCollection AddIntentAnalytics(this IServiceCollection services)
    {
        services.AddScoped<IIntentAnalytics, IntentAnalytics>();
        return services;
    }
}
