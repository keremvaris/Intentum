using Intentum.Analytics.Models;
using Intentum.Persistence.Repositories;

namespace Intentum.Analytics;

/// <summary>
/// Detects behavior patterns and frequency anomalies from intent history using intent-name sequences.
/// </summary>
public sealed class BehaviorPatternDetector : IBehaviorPatternDetector
{
    private readonly IIntentHistoryRepository _historyRepository;

    public BehaviorPatternDetector(IIntentHistoryRepository historyRepository)
    {
        _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BehaviorPattern>> GetBehaviorPatternsAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        int minSequenceLength = 2,
        int maxSequenceLength = 5,
        CancellationToken cancellationToken = default)
    {
        var records = await _historyRepository.GetByTimeWindowAsync(start, end, cancellationToken);
        var ordered = records.OrderBy(r => r.RecordedAt).ToList();
        var sequenceCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (var len = Math.Min(minSequenceLength, maxSequenceLength); len <= maxSequenceLength; len++)
        {
            for (var i = 0; i <= ordered.Count - len; i++)
            {
                var seq = ordered.Skip(i).Take(len).Select(r => r.IntentName).ToList();
                var key = string.Join("|", seq);
                sequenceCounts[key] = sequenceCounts.GetValueOrDefault(key) + 1;
            }
        }

        return sequenceCounts
            .Where(kv => kv.Value >= 1)
            .Select(kv => new BehaviorPattern(
                kv.Key.Split('|').ToList(),
                kv.Value))
            .OrderByDescending(p => p.Count)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PatternAnomalyReport>> GetPatternAnomaliesAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        TimeSpan bucketSize,
        double frequencyMultiplierThreshold = 10.0,
        CancellationToken cancellationToken = default)
    {
        var records = await _historyRepository.GetByTimeWindowAsync(start, end, cancellationToken);
        var reports = new List<PatternAnomalyReport>();

        if (records.Count == 0)
            return reports;

        var globalIntentCounts = records.GroupBy(r => r.IntentName).ToDictionary(g => g.Key, g => g.Count());
        var total = records.Count;
        var buckets = records.GroupBy(r => TruncateToBucket(r.RecordedAt, bucketSize)).ToList();

        foreach (var bucket in buckets)
        {
            var bucketStart = bucket.Key;
            var bucketEnd = bucketStart + bucketSize;
            var bucketRecords = bucket.ToList();
            var bucketTotal = bucketRecords.Count;
            var bucketIntentCounts = bucketRecords.GroupBy(r => r.IntentName).ToDictionary(g => g.Key, g => g.Count());

            foreach (var (intentName, bucketCount) in bucketIntentCounts)
            {
                var globalCount = globalIntentCounts.GetValueOrDefault(intentName, 0);
                var expectedInBucket = (globalCount / (double)total) * bucketTotal;
                if (expectedInBucket <= 0) continue;
                var ratio = bucketCount / expectedInBucket;
                if (ratio >= frequencyMultiplierThreshold)
                {
                    reports.Add(new PatternAnomalyReport(
                        "FrequencySpike",
                        $"Intent '{intentName}' {ratio:F1}x more frequent in bucket than baseline.",
                        bucketStart,
                        bucketStart,
                        bucketEnd,
                        Math.Min(1, ratio / 20),
                        new Dictionary<string, object>
                        {
                            ["IntentName"] = intentName,
                            ["BucketCount"] = bucketCount,
                            ["ExpectedInBucket"] = expectedInBucket,
                            ["Ratio"] = ratio
                        }));
                }
            }
        }

        return reports.OrderByDescending(r => r.Severity).ToList();
    }

    /// <inheritdoc />
    public IReadOnlyList<TemplateMatch> MatchTemplates(
        IReadOnlyList<BehaviorPattern> patterns,
        IReadOnlyList<IntentTemplate> templates)
    {
        var matches = new List<TemplateMatch>();
        foreach (var template in templates)
        {
            var bestScore = 0.0;
            var bestMatched = new List<string>();
            var expected = template.ExpectedIntentNames;
            foreach (var seq in patterns.Select(p => p.Sequence))
            {
                var overlap = seq.Count(s => expected.Any(e => string.Equals(e, s, StringComparison.OrdinalIgnoreCase)));
                var score = expected.Count > 0 ? overlap / (double)expected.Count : 0;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMatched = seq.Where(s => expected.Any(e => string.Equals(e, s, StringComparison.OrdinalIgnoreCase))).ToList();
                }
            }
            if (bestScore > 0)
                matches.Add(new TemplateMatch(template.Name, bestScore, bestMatched));
        }
        return matches.OrderByDescending(m => m.Score).ToList();
    }

    private static DateTimeOffset TruncateToBucket(DateTimeOffset value, TimeSpan bucketSize)
    {
        var ticks = value.UtcTicks - (value.UtcTicks % bucketSize.Ticks);
        return new DateTimeOffset(ticks, TimeSpan.Zero);
    }
}
