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

    /// <summary>
    /// Adds behavior pattern detector (sequence mining, frequency anomalies, template matching) to the service collection.
    /// </summary>
    public static IServiceCollection AddBehaviorPatternDetector(this IServiceCollection services)
    {
        services.AddScoped<IBehaviorPatternDetector, BehaviorPatternDetector>();
        return services;
    }

    /// <summary>
    /// Adds intent-based profile service (timeline → labels, top intents, confidence summary) to the service collection.
    /// </summary>
    public static IServiceCollection AddIntentProfileService(this IServiceCollection services)
    {
        services.AddScoped<IIntentProfileService, IntentProfileService>();
        return services;
    }
}
