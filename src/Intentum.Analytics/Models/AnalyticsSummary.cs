namespace Intentum.Analytics.Models;

/// <summary>
/// Summary of intent analytics for a time window (dashboard-ready).
/// </summary>
public sealed record AnalyticsSummary(
    DateTimeOffset Start,
    DateTimeOffset End,
    int TotalInferences,
    int UniqueBehaviorSpaces,
    IReadOnlyList<ConfidenceTrendPoint> ConfidenceTrend,
    DecisionDistributionReport DecisionDistribution,
    IReadOnlyList<AnomalyReport> Anomalies
);
