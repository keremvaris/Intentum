using Intentum.Persistence.Repositories;
using JetBrains.Annotations;

namespace Intentum.Clustering;

/// <summary>
/// Clusters intent history records for pattern detection and analysis.
/// </summary>
public interface IIntentClusterer
{
    /// <summary>
    /// Clusters records by confidence level and policy decision (pattern groups).
    /// Each unique (ConfidenceLevel, Decision) pair becomes one cluster.
    /// </summary>
    /// <param name="records">Intent history records to cluster.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of clusters, each containing records with the same (ConfidenceLevel, Decision).</returns>
    [UsedImplicitly]
    Task<IReadOnlyList<IntentCluster>> ClusterByPatternAsync(
        IReadOnlyList<IntentHistoryRecord> records,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Splits records into k groups by confidence score (equal-width buckets).
    /// Useful for segmentation (e.g. low/medium/high confidence bands).
    /// </summary>
    /// <param name="records">Intent history records to cluster.</param>
    /// <param name="k">Number of clusters (buckets).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of k clusters ordered by score range.</returns>
    [UsedImplicitly]
    Task<IReadOnlyList<IntentCluster>> ClusterByConfidenceScoreAsync(
        IReadOnlyList<IntentHistoryRecord> records,
        int k = 3,
        CancellationToken cancellationToken = default);
}
