namespace Intentum.Analytics.Models;

/// <summary>
/// Anomaly report for pattern/frequency (e.g. "Intent X 10x more frequent than baseline").
/// </summary>
/// <param name="Kind">Kind of anomaly (e.g. FrequencySpike, SequenceAnomaly).</param>
/// <param name="Description">Human-readable description.</param>
/// <param name="DetectedAt">When detected (bucket or time).</param>
/// <param name="BucketStart">Start of the bucket if bucket-based.</param>
/// <param name="BucketEnd">End of the bucket if bucket-based.</param>
/// <param name="Severity">Severity 0–1.</param>
/// <param name="Details">Additional key-value details.</param>
public sealed record PatternAnomalyReport(
    string Kind,
    string Description,
    DateTimeOffset DetectedAt,
    DateTimeOffset BucketStart,
    DateTimeOffset BucketEnd,
    double Severity,
    IReadOnlyDictionary<string, object> Details);
