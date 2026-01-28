namespace Intentum.Analytics.Models;

/// <summary>
/// A detected anomaly (e.g. spike in block rate, low confidence cluster).
/// </summary>
public sealed record AnomalyReport(
    string Type,
    string Description,
    DateTimeOffset DetectedAt,
    DateTimeOffset WindowStart,
    DateTimeOffset WindowEnd,
    double Severity,
    IReadOnlyDictionary<string, object>? Details = null
);
