namespace Intentum.Clustering;

/// <summary>
/// Represents a cluster of intent history records.
/// </summary>
/// <param name="Id">Unique cluster identifier.</param>
/// <param name="Label">Human-readable label (e.g. "High_Allow", "Low_Block").</param>
/// <param name="RecordIds">IDs of records in this cluster.</param>
/// <param name="Count">Number of records.</param>
/// <param name="Summary">Optional summary (e.g. average confidence score).</param>
public sealed record IntentCluster(
    string Id,
    string Label,
    IReadOnlyList<string> RecordIds,
    int Count,
    ClusterSummary? Summary = null);

/// <summary>
/// Summary statistics for a cluster.
/// </summary>
/// <param name="AverageConfidenceScore">Average confidence score of records in the cluster.</param>
/// <param name="MinScore">Minimum confidence score.</param>
/// <param name="MaxScore">Maximum confidence score.</param>
public sealed record ClusterSummary(
    double AverageConfidenceScore,
    double MinScore,
    double MaxScore);
