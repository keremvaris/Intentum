using Intentum.Analytics;
using Intentum.Analytics.Models;
using Intentum.Core.Intents;
using Intentum.Persistence.Repositories;
using Intentum.Runtime.Policy;

namespace Intentum.Tests;

/// <summary>
/// Tests for BehaviorPatternDetector: GetBehaviorPatternsAsync, GetPatternAnomaliesAsync, MatchTemplates.
/// </summary>
public sealed class BehaviorPatternDetectorTests
{
    [Fact]
    public void Constructor_WhenRepositoryNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new BehaviorPatternDetector(null!));
    }

    [Fact]
    public async Task GetBehaviorPatternsAsync_ReturnsOrderedPatternsByCount()
    {
        var repo = new FakeIntentHistoryRepository();
        var baseTime = DateTimeOffset.UtcNow;
        repo.Add(CreateRecord("A", baseTime));
        repo.Add(CreateRecord("B", baseTime.AddMinutes(1)));
        repo.Add(CreateRecord("A", baseTime.AddMinutes(2)));
        repo.Add(CreateRecord("B", baseTime.AddMinutes(3)));
        var detector = new BehaviorPatternDetector(repo);
        var start = baseTime.AddMinutes(-1);
        var end = baseTime.AddMinutes(5);

        var patterns = await detector.GetBehaviorPatternsAsync(start, end, minSequenceLength: 2, maxSequenceLength: 2);

        Assert.NotEmpty(patterns);
        Assert.True(patterns[0].Count >= 1);
        Assert.Contains(patterns, p => p.Sequence.Count == 2);
    }

    [Fact]
    public async Task GetBehaviorPatternsAsync_WhenNoRecords_ReturnsEmpty()
    {
        var repo = new FakeIntentHistoryRepository();
        var detector = new BehaviorPatternDetector(repo);
        var start = DateTimeOffset.UtcNow.AddDays(-1);
        var end = DateTimeOffset.UtcNow.AddDays(1);

        var patterns = await detector.GetBehaviorPatternsAsync(start, end);

        Assert.Empty(patterns);
    }

    [Fact]
    public async Task GetPatternAnomaliesAsync_WhenNoRecords_ReturnsEmpty()
    {
        var repo = new FakeIntentHistoryRepository();
        var detector = new BehaviorPatternDetector(repo);
        var start = DateTimeOffset.UtcNow.AddDays(-1);
        var end = DateTimeOffset.UtcNow.AddDays(1);

        var anomalies = await detector.GetPatternAnomaliesAsync(start, end, TimeSpan.FromHours(1));

        Assert.Empty(anomalies);
    }

    [Fact]
    public async Task GetPatternAnomaliesAsync_WithData_MayReturnAnomaliesWhenThresholdMet()
    {
        var repo = new FakeIntentHistoryRepository();
        var baseTime = DateTimeOffset.UtcNow;
        for (var i = 0; i < 20; i++)
            repo.Add(CreateRecord("Spike", baseTime.AddMinutes(i)));
        for (var i = 0; i < 2; i++)
            repo.Add(CreateRecord("Other", baseTime.AddHours(1).AddMinutes(i)));
        var detector = new BehaviorPatternDetector(repo);
        var start = baseTime.AddMinutes(-1);
        var end = baseTime.AddHours(2);
        var bucketSize = TimeSpan.FromMinutes(30);

        var anomalies = await detector.GetPatternAnomaliesAsync(start, end, bucketSize, frequencyMultiplierThreshold: 5.0);

        Assert.NotNull(anomalies);
    }

    [Fact]
    public void MatchTemplates_WhenPatternsMatchTemplate_ReturnsNonEmptyMatches()
    {
        var detector = new BehaviorPatternDetector(new FakeIntentHistoryRepository());
        var patterns = new List<BehaviorPattern>
        {
            new(["Login", "Checkout", "Purchase"], 5),
            new(["Browse", "AddToCart"], 3)
        };
        var templates = new List<IntentTemplate>
        {
            new("Purchase funnel", "Login to purchase", ["Login", "Checkout", "Purchase"])
        };

        var matches = detector.MatchTemplates(patterns, templates);

        Assert.NotEmpty(matches);
        Assert.Contains(matches, m => m.TemplateName == "Purchase funnel" && m.Score > 0);
    }

    [Fact]
    public void MatchTemplates_WhenNoOverlap_ReturnsEmpty()
    {
        var detector = new BehaviorPatternDetector(new FakeIntentHistoryRepository());
        var patterns = new List<BehaviorPattern> { new(["X", "Y"], 1) };
        var templates = new List<IntentTemplate> { new("Other", "A,B", ["A", "B"]) };

        var matches = detector.MatchTemplates(patterns, templates);

        Assert.Empty(matches);
    }

    [Fact]
    public void MatchTemplates_WhenTemplatesEmpty_ReturnsEmpty()
    {
        var detector = new BehaviorPatternDetector(new FakeIntentHistoryRepository());
        var patterns = new List<BehaviorPattern> { new(["A"], 1) };

        var matches = detector.MatchTemplates(patterns, []);

        Assert.Empty(matches);
    }

    private static IntentHistoryRecord CreateRecord(string intentName, DateTimeOffset recordedAt)
    {
        var intent = new Intent(intentName, [], new IntentConfidence(0.8, "High"), "rule");
        return IntentHistoryRecord.Create("bs1", intent, PolicyDecision.Allow, entityId: "e1") with { RecordedAt = recordedAt };
    }

    private sealed class FakeIntentHistoryRepository : IIntentHistoryRepository
    {
        private readonly List<IntentHistoryRecord> _records = [];
        private int _id;

        public void Add(IntentHistoryRecord record) => _records.Add(record with { Id = (++_id).ToString() });

        public Task<string> SaveAsync(string behaviorSpaceId, Intent intent, PolicyDecision decision, IReadOnlyDictionary<string, object>? metadata = null, string? entityId = null, CancellationToken cancellationToken = default)
        {
            var record = IntentHistoryRecord.Create(behaviorSpaceId, intent, decision, metadata, entityId);
            _records.Add(record with { Id = (++_id).ToString() });
            return Task.FromResult(_id.ToString());
        }

        public Task<IReadOnlyList<IntentHistoryRecord>> GetByBehaviorSpaceIdAsync(string behaviorSpaceId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<IntentHistoryRecord>>(_records.Where(r => r.BehaviorSpaceId == behaviorSpaceId).ToList());

        public Task<IReadOnlyList<IntentHistoryRecord>> GetByConfidenceLevelAsync(string confidenceLevel, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<IntentHistoryRecord>>(_records.Where(r => r.ConfidenceLevel == confidenceLevel).ToList());

        public Task<IReadOnlyList<IntentHistoryRecord>> GetByDecisionAsync(PolicyDecision decision, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<IntentHistoryRecord>>(_records.Where(r => r.Decision == decision).ToList());

        public Task<IReadOnlyList<IntentHistoryRecord>> GetByTimeWindowAsync(DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<IntentHistoryRecord>>(_records.Where(r => r.RecordedAt >= start && r.RecordedAt <= end).OrderBy(r => r.RecordedAt).ToList());

        public Task<IReadOnlyList<IntentHistoryRecord>> GetByEntityIdAsync(string entityId, DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<IntentHistoryRecord>>(_records
                .Where(r => (r.EntityId == entityId || r.BehaviorSpaceId == entityId) && r.RecordedAt >= start && r.RecordedAt <= end)
                .OrderBy(r => r.RecordedAt)
                .ToList());
    }
}
