using Microsoft.Extensions.DependencyInjection;

namespace Intentum.Events;

/// <summary>
/// Extension methods for registering Intentum events with dependency injection.
/// </summary>
public static class EventsExtensions
{
    /// <summary>
    /// Adds Intentum event handling (webhooks) to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to configure webhooks and retry.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddIntentumEvents(
        this IServiceCollection services,
        Action<IntentumEventsOptions>? configure = null)
    {
        services.AddOptions<IntentumEventsOptions>();
        if (configure != null)
            services.Configure(configure);

        services.AddHttpClient(IntentumEventsOptions.HttpClientName);

        services.AddScoped<IIntentEventHandler, WebhookIntentEventHandler>();
        return services;
    }

    /// <summary>
    /// Adds a webhook to the events options. Call from within Configure(IntentumEventsOptions).
    /// </summary>
    public static IntentumEventsOptions AddWebhook(
        this IntentumEventsOptions options,
        string url,
        IEnumerable<string>? events = null)
    {
        var eventTypes = events?.ToHashSet(StringComparer.OrdinalIgnoreCase)
            ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                nameof(IntentumEventType.IntentInferred),
                nameof(IntentumEventType.PolicyDecisionChanged)
            };
        options.Webhooks.Add(new WebhookConfig { Url = url, EventTypes = eventTypes });
        return options;
    }
}
