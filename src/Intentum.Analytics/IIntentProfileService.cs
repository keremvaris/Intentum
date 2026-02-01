using Intentum.Analytics.Models;

namespace Intentum.Analytics;

/// <summary>
/// Builds an intent-based profile for an entity from timeline data (aggregate intent names, confidence distribution → labels).
/// </summary>
public interface IIntentProfileService
{
    /// <summary>
    /// Gets a profile for the entity in the time window: labels, top intents, confidence summary.
    /// </summary>
    Task<IntentProfile> GetProfileAsync(
        string entityId,
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default);
}
