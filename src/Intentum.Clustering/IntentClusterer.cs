using Intentum.Persistence.Repositories;

namespace Intentum.Clustering;

/// <summary>
/// Default implementation of intent clustering using pattern-based and score-based grouping.
/// </summary>
public sealed class IntentClusterer : IIntentClusterer
{
    /// <inheritdoc />
    public Task<IReadOnlyList<IntentCluster>> ClusterByPatternAsync(
        IReadOnlyList<IntentHistoryRecord> records,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var groups = records
            .GroupBy(r => (r.ConfidenceLevel, r.Decision))
            .Select(g =>
            {
                var list = g.ToList();
                var ids = list.Select(x => x.Id).ToList();
                var scores = list.Select(x => x.ConfidenceScore).ToList();
                var avg = scores.Average();
                var min = scores.Min();
                var max = scores.Max();
                var id = $"{g.Key.ConfidenceLevel}_{g.Key.Decision}";
                var label = $"{g.Key.ConfidenceLevel} / {g.Key.Decision}";
                var summary = new ClusterSummary(avg, min, max);
                return new IntentCluster(id, label, ids, list.Count, summary);
            })
            .OrderByDescending(c => c.Count)
            .ToList();

        return Task.FromResult<IReadOnlyList<IntentCluster>>(groups);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<IntentCluster>> ClusterByConfidenceScoreAsync(
        IReadOnlyList<IntentHistoryRecord> records,
        int k = 3,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (k < 1) k = 1;
        var list = records.ToList();
        if (list.Count == 0)
            return Task.FromResult<IReadOnlyList<IntentCluster>>(Array.Empty<IntentCluster>());

        var scores = list.Select(r => r.ConfidenceScore).OrderBy(s => s).ToList();
        var minScore = scores.First();
        var maxScore = scores.Last();
        var range = maxScore - minScore;
        var bucketWidth = range <= 0 ? 1.0 : range / k;

        var clusters = new List<IntentCluster>();
        for (var i = 0; i < k; i++)
        {
            var low = minScore + i * bucketWidth;
            var high = i == k - 1 ? maxScore + 0.001 : minScore + (i + 1) * bucketWidth;
            var inBucket = list.Where(r => r.ConfidenceScore >= low && r.ConfidenceScore < high).ToList();
            var ids = inBucket.Select(x => x.Id).ToList();
            var bucketScores = inBucket.Select(x => x.ConfidenceScore).ToList();
            var avg = bucketScores.Count > 0 ? bucketScores.Average() : 0;
            var min = bucketScores.Count > 0 ? bucketScores.Min() : 0;
            var max = bucketScores.Count > 0 ? bucketScores.Max() : 0;
            var clusterId = $"ScoreBucket_{i}";
            var label = $"[{low:F2}, {high:F2})";
            clusters.Add(new IntentCluster(clusterId, label, ids, inBucket.Count, new ClusterSummary(avg, min, max)));
        }

        return Task.FromResult<IReadOnlyList<IntentCluster>>(clusters);
    }
}
