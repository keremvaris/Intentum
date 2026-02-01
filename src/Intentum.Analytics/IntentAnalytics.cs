using System.Text;
using System.Text.Json;
using Intentum.Analytics.Models;
using Intentum.Persistence.Repositories;
using Intentum.Runtime.Policy;

namespace Intentum.Analytics;

/// <summary>
/// Default implementation of intent analytics using intent history repository.
/// </summary>
public sealed class IntentAnalytics : IIntentAnalytics
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly IIntentHistoryRepository _historyRepository;

    public IntentAnalytics(IIntentHistoryRepository historyRepository)
    {
        _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ConfidenceTrendPoint>> GetConfidenceTrendsAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        TimeSpan bucketSize,
        CancellationToken cancellationToken = default)
    {
        var records = await _historyRepository.GetByTimeWindowAsync(start, end, cancellationToken);
        var buckets = new Dictionary<(DateTimeOffset Bucket, string Level), (int Count, double Sum)>();

        foreach (var r in records)
        {
            var bucketStart = TruncateToBucket(r.RecordedAt, bucketSize);
            var key = (bucketStart, r.ConfidenceLevel);
            if (!buckets.TryGetValue(key, out var existing))
                existing = (0, 0);
            buckets[key] = (existing.Count + 1, existing.Sum + r.ConfidenceScore);
        }

        var bucketEnds = buckets.Keys.Select(k => k.Bucket).Distinct().OrderBy(x => x).ToList();
        var levels = buckets.Keys.Select(k => k.Level).Distinct().ToList();
        var result = new List<ConfidenceTrendPoint>();

        foreach (var bucketStart in bucketEnds)
        {
            var bucketEnd = bucketStart + bucketSize;
            foreach (var level in levels)
            {
                if (!buckets.TryGetValue((bucketStart, level), out var v))
                    continue;
                result.Add(new ConfidenceTrendPoint(
                    bucketStart,
                    bucketEnd,
                    level,
                    v.Count,
                    v.Count > 0 ? v.Sum / v.Count : 0));
            }
        }

        return result.OrderBy(x => x.BucketStart).ThenBy(x => x.ConfidenceLevel).ToList();
    }

    /// <inheritdoc />
    public async Task<DecisionDistributionReport> GetDecisionDistributionAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default)
    {
        var records = await _historyRepository.GetByTimeWindowAsync(start, end, cancellationToken);
        var countByDecision = records
            .GroupBy(r => r.Decision)
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var decision in Enum.GetValues<PolicyDecision>().Where(d => !countByDecision.ContainsKey(d)))
            countByDecision[decision] = 0;

        return new DecisionDistributionReport(
            start,
            end,
            records.Count,
            countByDecision);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AnomalyReport>> DetectAnomaliesAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        TimeSpan bucketSize,
        CancellationToken cancellationToken = default)
    {
        var records = await _historyRepository.GetByTimeWindowAsync(start, end, cancellationToken);
        var anomalies = new List<AnomalyReport>();

        if (records.Count == 0)
            return anomalies;

        var totalBlockRate = records.Count(r => r.Decision == PolicyDecision.Block) / (double)records.Count;
        var buckets = records.GroupBy(r => TruncateToBucket(r.RecordedAt, bucketSize)).ToList();
        var avgCountPerBucket = records.Count / (double)Math.Max(1, buckets.Count);

        foreach (var g in buckets)
        {
            var bucketStart = g.Key;
            var bucketEnd = bucketStart + bucketSize;
            var bucketRecords = g.ToList();
            var blockCount = bucketRecords.Count(r => r.Decision == PolicyDecision.Block);
            var blockRate = bucketRecords.Count > 0 ? blockCount / (double)bucketRecords.Count : 0;

            if (blockRate > totalBlockRate + 0.2 && totalBlockRate < 0.5)
            {
                anomalies.Add(new AnomalyReport(
                    "BlockRateSpike",
                    $"Block rate spike in bucket: {blockRate:P0} (overall: {totalBlockRate:P0})",
                    bucketStart,
                    bucketStart,
                    bucketEnd,
                    Math.Min(1, (blockRate - totalBlockRate) * 2),
                    new Dictionary<string, object> { ["BlockCount"] = blockCount, ["TotalInBucket"] = bucketRecords.Count }));
            }

            if (avgCountPerBucket > 0 && bucketRecords.Count > avgCountPerBucket * 1.8)
            {
                anomalies.Add(new AnomalyReport(
                    "VolumeSpike",
                    $"Volume spike: {bucketRecords.Count} inferences in bucket (avg: {avgCountPerBucket:F0})",
                    bucketStart,
                    bucketStart,
                    bucketEnd,
                    Math.Min(1, bucketRecords.Count / (avgCountPerBucket * 4)),
                    new Dictionary<string, object> { ["Count"] = bucketRecords.Count, ["Average"] = avgCountPerBucket }));
            }

            var lowConfidenceCount = bucketRecords.Count(r => string.Equals(r.ConfidenceLevel, "Low", StringComparison.OrdinalIgnoreCase));
            if (bucketRecords.Count >= 2 && lowConfidenceCount >= 1 && lowConfidenceCount / (double)bucketRecords.Count >= 0.5)
            {
                anomalies.Add(new AnomalyReport(
                    "LowConfidenceCluster",
                    $"Low confidence cluster: {lowConfidenceCount}/{bucketRecords.Count} in bucket",
                    bucketStart,
                    bucketStart,
                    bucketEnd,
                    0.5,
                    new Dictionary<string, object> { ["LowCount"] = lowConfidenceCount, ["TotalInBucket"] = bucketRecords.Count }));
            }
        }

        return anomalies.OrderByDescending(a => a.Severity).ToList();
    }

    /// <inheritdoc />
    public async Task<AnalyticsSummary> GetSummaryAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        TimeSpan bucketSize,
        CancellationToken cancellationToken = default)
    {
        var trends = await GetConfidenceTrendsAsync(start, end, bucketSize, cancellationToken);
        var distribution = await GetDecisionDistributionAsync(start, end, cancellationToken);
        var anomalies = await DetectAnomaliesAsync(start, end, bucketSize, cancellationToken);

        var records = await _historyRepository.GetByTimeWindowAsync(start, end, cancellationToken);
        var uniqueSpaces = records.Select(r => r.BehaviorSpaceId).Distinct().Count();

        return new AnalyticsSummary(
            start,
            end,
            records.Count,
            uniqueSpaces,
            trends,
            distribution,
            anomalies);
    }

    /// <inheritdoc />
    public async Task<IntentTimeline> GetIntentTimelineAsync(
        string entityId,
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default)
    {
        var records = await _historyRepository.GetByEntityIdAsync(entityId, start, end, cancellationToken);
        var points = records
            .Select(r => new IntentTimelinePoint(
                r.RecordedAt,
                r.IntentName,
                r.ConfidenceLevel,
                r.ConfidenceScore,
                r.Decision))
            .ToList();
        return new IntentTimeline(entityId, start, end, points);
    }

    /// <inheritdoc />
    public async Task<IntentGraphSnapshot> GetIntentGraphSnapshotAsync(
        string entityId,
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default)
    {
        var records = (await _historyRepository.GetByEntityIdAsync(entityId, start, end, cancellationToken))
            .OrderBy(r => r.RecordedAt)
            .ToList();

        var nodes = records
            .Select(r => new IntentGraphNode(
                r.Id,
                r.IntentName,
                r.ConfidenceScore,
                r.ConfidenceLevel,
                r.RecordedAt))
            .ToList();

        var edges = new List<IntentGraphEdge>();
        for (var i = 0; i < records.Count - 1; i++)
        {
            var from = records[i];
            var to = records[i + 1];
            edges.Add(new IntentGraphEdge(from.Id, to.Id, to.RecordedAt));
        }

        return new IntentGraphSnapshot(
            entityId,
            start,
            end,
            nodes,
            edges,
            DateTimeOffset.UtcNow);
    }

    /// <inheritdoc />
    public async Task<string> ExportToJsonAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default)
    {
        var records = await _historyRepository.GetByTimeWindowAsync(start, end, cancellationToken);
        var dto = records.Select(r => new
        {
            r.Id,
            r.BehaviorSpaceId,
            r.IntentName,
            r.ConfidenceLevel,
            r.ConfidenceScore,
            Decision = r.Decision.ToString(),
            r.RecordedAt
        }).ToList();
        return JsonSerializer.Serialize(dto, JsonOptions);
    }

    /// <inheritdoc />
    public async Task<string> ExportToCsvAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default)
    {
        var records = await _historyRepository.GetByTimeWindowAsync(start, end, cancellationToken);
        var sb = new StringBuilder();
        sb.AppendLine("Id,BehaviorSpaceId,IntentName,ConfidenceLevel,ConfidenceScore,Decision,RecordedAt");
        foreach (var r in records)
        {
            sb.AppendLine($"{EscapeCsv(r.Id)},{EscapeCsv(r.BehaviorSpaceId)},{EscapeCsv(r.IntentName)},{EscapeCsv(r.ConfidenceLevel)},{r.ConfidenceScore},{EscapeCsv(r.Decision.ToString())},{r.RecordedAt:O}");
        }
        return sb.ToString();
    }

    private static DateTimeOffset TruncateToBucket(DateTimeOffset value, TimeSpan bucketSize)
    {
        var ticks = value.UtcTicks - (value.UtcTicks % bucketSize.Ticks);
        return new DateTimeOffset(ticks, TimeSpan.Zero);
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }
}
