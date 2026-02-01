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
            [],
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

    [Fact]
    public async Task GetIntentTimelineAsync_ReturnsTimeOrderedPointsForEntity()
    {
        var analytics = await CreateAnalyticsWithSampleDataAsync();
        var now = DateTimeOffset.UtcNow;
        var start = now.AddDays(-1);
        var end = now.AddDays(1);

        var timeline = await analytics.GetIntentTimelineAsync("bs1", start, end);

        Assert.Equal("bs1", timeline.EntityId);
        Assert.Equal(start, timeline.Start);
        Assert.Equal(end, timeline.End);
        Assert.Equal(2, timeline.Points.Count);
        Assert.True(timeline.Points[0].RecordedAt <= timeline.Points[1].RecordedAt);
        Assert.Contains(timeline.Points, p => p.IntentName == "Test" && (p.ConfidenceLevel == "High" || p.ConfidenceLevel == "Certain"));
    }

    [Fact]
    public async Task GetIntentGraphSnapshotAsync_ReturnsNodesAndEdgesForEntity()
    {
        var analytics = await CreateAnalyticsWithSampleDataAsync();
        var now = DateTimeOffset.UtcNow;
        var start = now.AddDays(-1);
        var end = now.AddDays(1);

        var snapshot = await analytics.GetIntentGraphSnapshotAsync("bs1", start, end);

        Assert.Equal("bs1", snapshot.EntityId);
        Assert.Equal(start, snapshot.WindowStart);
        Assert.Equal(end, snapshot.WindowEnd);
        Assert.Equal(2, snapshot.Nodes.Count);
        Assert.Single(snapshot.Edges);
        Assert.Equal(snapshot.Nodes[0].Id, snapshot.Edges[0].FromNodeId);
        Assert.Equal(snapshot.Nodes[1].Id, snapshot.Edges[0].ToNodeId);
    }

    [Fact]
    public async Task IntentProfileService_GetProfileAsync_ReturnsLabelsAndTopIntents()
    {
        var analytics = await CreateAnalyticsWithSampleDataAsync();
        var profileService = new IntentProfileService(analytics);
        var now = DateTimeOffset.UtcNow;
        var start = now.AddDays(-1);
        var end = now.AddDays(1);

        var profile = await profileService.GetProfileAsync("bs1", start, end);

        Assert.Equal("bs1", profile.EntityId);
        Assert.Equal(2, profile.PointCount);
        Assert.True(profile.AverageConfidenceScore > 0);
        Assert.True(profile.TopIntents.ContainsKey("Test"));
        Assert.Equal(2, profile.TopIntents["Test"]);
        Assert.NotEmpty(profile.Labels);
    }

    [Fact]
    public async Task IntentProfileService_GetProfileAsync_WhenNoData_ReturnsEmptyProfile()
    {
        var analytics = await CreateAnalyticsWithSampleDataAsync();
        var profileService = new IntentProfileService(analytics);
        var now = DateTimeOffset.UtcNow;
        var start = now.AddDays(-1);
        var end = now.AddDays(1);

        var profile = await profileService.GetProfileAsync("nonexistent", start, end);

        Assert.Equal("nonexistent", profile.EntityId);
        Assert.Equal(0, profile.PointCount);
        Assert.Empty(profile.Labels);
        Assert.Empty(profile.TopIntents);
        Assert.Equal(0, profile.AverageConfidenceScore);
        Assert.Equal(0, profile.HighConfidencePercent);
    }

    private sealed class InMemoryIntentHistoryRepository : IIntentHistoryRepository
    {
        private readonly List<IntentHistoryRecord> _records = [];
        private int _id;

        public Task<string> SaveAsync(string behaviorSpaceId, Intent intent, PolicyDecision decision, IReadOnlyDictionary<string, object>? metadata = null, string? entityId = null, CancellationToken cancellationToken = default)
        {
            var id = (++_id).ToString();
            _records.Add(new IntentHistoryRecord(
                id, behaviorSpaceId, intent.Name, intent.Confidence.Level, intent.Confidence.Score,
                decision, DateTimeOffset.UtcNow, metadata, entityId));
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

        public Task<IReadOnlyList<IntentHistoryRecord>> GetByEntityIdAsync(string entityId, DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<IntentHistoryRecord>>(_records
                .Where(r => (r.EntityId == entityId || r.BehaviorSpaceId == entityId) && r.RecordedAt >= start && r.RecordedAt <= end)
                .OrderBy(r => r.RecordedAt)
                .ToList());
    }
}
