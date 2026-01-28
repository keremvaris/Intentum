namespace Intentum.Analytics.Models;

/// <summary>
/// A single point in a confidence trend (e.g. one time bucket).
/// </summary>
public sealed record ConfidenceTrendPoint(
    DateTimeOffset BucketStart,
    DateTimeOffset BucketEnd,
    string ConfidenceLevel,
    int Count,
    double AverageScore
);
