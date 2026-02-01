namespace Intentum.Events;

/// <summary>
/// Options for Intentum events (webhooks, retry).
/// </summary>
public sealed class IntentumEventsOptions
{
    internal const string HttpClientName = "Intentum.Events.Webhook";

    /// <summary>
    /// Configured webhooks (URL + event types to send).
    /// </summary>
    public List<WebhookConfig> Webhooks { get; } = [];

    /// <summary>
    /// Number of retries for failed HTTP POST (default 3).
    /// </summary>
    public int RetryCount { get; set; } = 3;
}

/// <summary>
/// Configuration for a single webhook endpoint.
/// </summary>
public sealed class WebhookConfig
{
    /// <summary>
    /// Webhook URL (HTTP POST).
    /// </summary>
    public string Url { get; set; } = "";

    /// <summary>
    /// Event types to send (e.g. "IntentInferred", "PolicyDecisionChanged").
    /// </summary>
    public HashSet<string> EventTypes { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
