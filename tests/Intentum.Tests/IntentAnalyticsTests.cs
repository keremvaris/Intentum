using Intentum.Analytics;
using Intentum.Core.Intents;
using Intentum.Persistence.Repositories;
using Intentum.Runtime.Policy;

namespace Intentum.Tests;

public sealed class IntentAnalyticsTests
{
    private static async Task<IIntentAnalytics> CreateAnalyticsWithSampleDataAsync()
    {
        var repo = new InMemoryIntentHistoryRepository();

        await repo.SaveAsync("bs1", CreateIntent("High", 0.9), PolicyDecision.Allow);
        await repo.SaveAsync("bs1", CreateIntent("Medium", 0.5), PolicyDecision.Observe);
        await repo.SaveAsync("bs2", CreateIntent("Low", 0.2), PolicyDecision.Block);
        await repo.SaveAsync("bs2", CreateIntent("High", 0.85), PolicyDecision.Allow);
        await repo.SaveAsync("bs3", CreateIntent("Medium", 0.6), PolicyDecision.Warn);

        return new IntentAnalytics(repo);
    }

    private static Intent CreateIntent(string _, double score)
    {
        return new Intent(
            "Test",
            Array.Empty<IntentSignal>(),
            IntentConfidence.FromScore(score));
    }

    [Fact]
    public async Task GetConfidenceTrendsAsync_ReturnsBucketedTrends()
    {
        var analytics = await CreateAnalyticsWithSampleDataAsync();
        var now = DateTimeOffset.UtcNow;
        var start = now.AddDays(-1);
        var end = now.AddDays(1);

        var trends = await analytics.GetConfidenceTrendsAsync(start, end, TimeSpan.FromDays(1));

        Assert.NotEmpty(trends);
        Assert.Contains(trends, t => t.ConfidenceLevel == "High");
        Assert.Contains(trends, t => t.ConfidenceLevel == "Medium");
        Assert.Contains(trends, t => t.ConfidenceLevel == "Low");
    }

    [Fact]
    public async Task GetDecisionDistributionAsync_ReturnsCountsPerDecision()
    {
        var analytics = await CreateAnalyticsWithSampleDataAsync();
        var now = DateTimeOffset.UtcNow;
        var start = now.AddDays(-1);
        var end = now.AddDays(1);

        var report = await analytics.GetDecisionDistributionAsync(start, end);

        Assert.Equal(5, report.TotalCount);
        Assert.True(report.CountByDecision[PolicyDecision.Allow] >= 2);
        Assert.True(report.CountByDecision[PolicyDecision.Block] >= 1);
        Assert.True(report.CountByDecision[PolicyDecision.Observe] >= 1);
        Assert.True(report.CountByDecision[PolicyDecision.Warn] >= 1);
    }

    [Fact]
    public async Task GetSummaryAsync_ReturnsFullSummary()
    {
        var analytics = await CreateAnalyticsWithSampleDataAsync();
        var now = DateTimeOffset.UtcNow;
        var start = now.AddDays(-1);
        var end = now.AddDays(1);

        var summary = await analytics.GetSummaryAsync(start, end, TimeSpan.FromDays(1));

        Assert.Equal(5, summary.TotalInferences);
        Assert.Equal(3, summary.UniqueBehaviorSpaces);
        Assert.NotNull(summary.ConfidenceTrend);
        Assert.NotNull(summary.DecisionDistribution);
        Assert.NotNull(summary.Anomalies);
    }

    [Fact]
    public async Task ExportToJsonAsync_ReturnsValidJson()
    {
        var analytics = await CreateAnalyticsWithSampleDataAsync();
        var now = DateTimeOffset.UtcNow;
        var start = now.AddDays(-1);
        var end = now.AddDays(1);

        var json = await analytics.ExportToJsonAsync(start, end);

        Assert.NotEmpty(json);
        Assert.StartsWith("[", json.TrimStart());
    }

    [Fact]
    public async Task ExportToCsvAsync_ReturnsCsvWithHeader()
    {
        var analytics = await CreateAnalyticsWithSampleDataAsync();
        var now = DateTimeOffset.UtcNow;
        var start = now.AddDays(-1);
        var end = now.AddDays(1);

        var csv = await analytics.ExportToCsvAsync(start, end);

        Assert.Contains("Id,BehaviorSpaceId,IntentName,ConfidenceLevel", csv);
        Assert.Contains("bs1", csv);
    }

    private sealed class InMemoryIntentHistoryRepository : IIntentHistoryRepository
    {
        private readonly List<IntentHistoryRecord> _records = new();
        private int _id;

        public Task<string> SaveAsync(string behaviorSpaceId, Intent intent, PolicyDecision decision, IReadOnlyDictionary<string, object>? metadata = null, CancellationToken cancellationToken = default)
        {
            var id = (++_id).ToString();
            _records.Add(new IntentHistoryRecord(
                id, behaviorSpaceId, intent.Name, intent.Confidence.Level, intent.Confidence.Score,
                decision, DateTimeOffset.UtcNow, metadata));
            return Task.FromResult(id);
        }

        public Task<IReadOnlyList<IntentHistoryRecord>> GetByBehaviorSpaceIdAsync(string behaviorSpaceId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<IntentHistoryRecord>>(_records.Where(r => r.BehaviorSpaceId == behaviorSpaceId).ToList());

        public Task<IReadOnlyList<IntentHistoryRecord>> GetByConfidenceLevelAsync(string confidenceLevel, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<IntentHistoryRecord>>(_records.Where(r => r.ConfidenceLevel == confidenceLevel).ToList());

        public Task<IReadOnlyList<IntentHistoryRecord>> GetByDecisionAsync(PolicyDecision decision, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<IntentHistoryRecord>>(_records.Where(r => r.Decision == decision).ToList());

        public Task<IReadOnlyList<IntentHistoryRecord>> GetByTimeWindowAsync(DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<IntentHistoryRecord>>(_records.Where(r => r.RecordedAt >= start && r.RecordedAt <= end).ToList());
    }
}
