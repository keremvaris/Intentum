using Intentum.Analytics.Models;

namespace Intentum.Analytics;

/// <summary>
/// Intent analytics and reporting service.
/// </summary>
public interface IIntentAnalytics
{
    /// <summary>
    /// Gets confidence trend over time (bucketed by the specified interval).
    /// </summary>
    Task<IReadOnlyList<ConfidenceTrendPoint>> GetConfidenceTrendsAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        TimeSpan bucketSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the distribution of policy decisions in the time window.
    /// </summary>
    Task<DecisionDistributionReport> GetDecisionDistributionAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Detects anomalies (e.g. spike in Block rate, low confidence cluster).
    /// </summary>
    Task<IReadOnlyList<AnomalyReport>> DetectAnomaliesAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        TimeSpan bucketSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a full analytics summary (dashboard-ready).
    /// </summary>
    Task<AnalyticsSummary> GetSummaryAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        TimeSpan bucketSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets intent timeline for an entity (e.g. userId) within a time window: time-ordered intent inference points.
    /// </summary>
    Task<IntentTimeline> GetIntentTimelineAsync(
        string entityId,
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an intent graph snapshot for an entity within a time window: nodes (intent + confidence) and edges (transitions).
    /// </summary>
    Task<IntentGraphSnapshot> GetIntentGraphSnapshotAsync(
        string entityId,
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports intent history in the time window to JSON.
    /// </summary>
    Task<string> ExportToJsonAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports intent history in the time window to CSV.
    /// </summary>
    Task<string> ExportToCsvAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default);
}
