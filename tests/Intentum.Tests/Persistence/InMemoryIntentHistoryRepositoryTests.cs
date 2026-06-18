using Intentum.Core.Intents;
using Intentum.Persistence.Repositories;
using Intentum.Runtime.Policy;
using Intentum.Tests.Helpers;

namespace Intentum.Tests.Persistence;

public sealed class InMemoryIntentHistoryRepositoryTests
{
    private readonly InMemoryIntentHistoryRepository _repo = new();

    [Fact]
    public async Task SaveAsync_ReturnsGeneratedId()
    {
        var intent = new Intent("Test", [], new IntentConfidence(0.8, "High"));

        var id = await _repo.SaveAsync("bs-1", intent, PolicyDecision.Allow);

        Assert.NotNull(id);
        Assert.NotEmpty(id);
    }

    [Fact]
    public async Task SaveAsync_StoresRecord()
    {
        var intent = new Intent("Test", [], new IntentConfidence(0.8, "High"));

        await _repo.SaveAsync("bs-1", intent, PolicyDecision.Allow);

        var results = await _repo.GetByBehaviorSpaceIdAsync("bs-1");
        Assert.Single(results);
        Assert.Equal("Test", results[0].IntentName);
    }

    [Fact]
    public async Task GetByBehaviorSpaceIdAsync_ReturnsEmpty_WhenNoData()
    {
        var results = await _repo.GetByBehaviorSpaceIdAsync("nonexistent");

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetByBehaviorSpaceIdAsync_FiltersCorrectly()
    {
        var intent1 = new Intent("Intent1", [], new IntentConfidence(0.8, "High"));
        var intent2 = new Intent("Intent2", [], new IntentConfidence(0.5, "Medium"));
        await _repo.SaveAsync("bs-1", intent1, PolicyDecision.Allow);
        await _repo.SaveAsync("bs-2", intent2, PolicyDecision.Block);

        var results = await _repo.GetByBehaviorSpaceIdAsync("bs-1");

        Assert.Single(results);
        Assert.Equal("Intent1", results[0].IntentName);
    }

    [Fact]
    public async Task GetByConfidenceLevelAsync_ReturnsMatchingRecords()
    {
        var intent1 = new Intent("High1", [], new IntentConfidence(0.9, "High"));
        var intent2 = new Intent("Low1", [], new IntentConfidence(0.2, "Low"));
        await _repo.SaveAsync("bs-1", intent1, PolicyDecision.Allow);
        await _repo.SaveAsync("bs-1", intent2, PolicyDecision.Allow);

        var results = await _repo.GetByConfidenceLevelAsync("High");

        Assert.Single(results);
        Assert.Equal("High1", results[0].IntentName);
    }

    [Fact]
    public async Task GetByConfidenceLevelAsync_ReturnsEmpty_WhenNoMatch()
    {
        var intent = new Intent("Test", [], new IntentConfidence(0.5, "Medium"));
        await _repo.SaveAsync("bs-1", intent, PolicyDecision.Allow);

        var results = await _repo.GetByConfidenceLevelAsync("Certain");

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetByDecisionAsync_ReturnsMatchingRecords()
    {
        var intent1 = new Intent("Allow1", [], new IntentConfidence(0.8, "High"));
        var intent2 = new Intent("Block1", [], new IntentConfidence(0.8, "High"));
        await _repo.SaveAsync("bs-1", intent1, PolicyDecision.Allow);
        await _repo.SaveAsync("bs-1", intent2, PolicyDecision.Block);

        var results = await _repo.GetByDecisionAsync(PolicyDecision.Block);

        Assert.Single(results);
        Assert.Equal("Block1", results[0].IntentName);
    }

    [Fact]
    public async Task GetByDecisionAsync_ReturnsEmpty_WhenNoMatch()
    {
        var intent = new Intent("Test", [], new IntentConfidence(0.5, "Medium"));
        await _repo.SaveAsync("bs-1", intent, PolicyDecision.Allow);

        var results = await _repo.GetByDecisionAsync(PolicyDecision.Escalate);

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetByTimeWindowAsync_FiltersByTime()
    {
        var now = DateTimeOffset.UtcNow;
        var intent1 = new Intent("Old", [], new IntentConfidence(0.8, "High"));
        var intent2 = new Intent("New", [], new IntentConfidence(0.8, "High"));

        // Use the Create factory to set specific RecordedAt times
        var record1 = IntentHistoryRecord.Create("bs-1", intent1, PolicyDecision.Allow);
        var record2 = IntentHistoryRecord.Create("bs-1", intent2, PolicyDecision.Allow);

        // Manually add with different timestamps by using reflection or by saving and manipulating
        await _repo.SaveAsync("bs-1", intent1, PolicyDecision.Allow);
        await _repo.SaveAsync("bs-1", intent2, PolicyDecision.Allow);

        var start = DateTimeOffset.UtcNow.AddSeconds(-1);
        var end = DateTimeOffset.UtcNow.AddSeconds(1);
        var results = await _repo.GetByTimeWindowAsync(start, end);

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task GetByEntityIdAsync_ReturnsMatchingRecords()
    {
        var intent1 = new Intent("User1", [], new IntentConfidence(0.8, "High"));
        var intent2 = new Intent("User2", [], new IntentConfidence(0.8, "High"));
        await _repo.SaveAsync("bs-1", intent1, PolicyDecision.Allow, entityId: "user-1");
        await _repo.SaveAsync("bs-1", intent2, PolicyDecision.Allow, entityId: "user-2");

        var start = DateTimeOffset.UtcNow.AddMinutes(-1);
        var end = DateTimeOffset.UtcNow.AddMinutes(1);
        var results = await _repo.GetByEntityIdAsync("user-1", start, end);

        Assert.Single(results);
        Assert.Equal("User1", results[0].IntentName);
    }

    [Fact]
    public async Task GetByEntityIdAsync_ReturnsEmpty_WhenNoMatch()
    {
        var intent = new Intent("Test", [], new IntentConfidence(0.8, "High"));
        await _repo.SaveAsync("bs-1", intent, PolicyDecision.Allow, entityId: "user-1");

        var start = DateTimeOffset.UtcNow.AddMinutes(-1);
        var end = DateTimeOffset.UtcNow.AddMinutes(1);
        var results = await _repo.GetByEntityIdAsync("user-999", start, end);

        Assert.Empty(results);
    }

    [Fact]
    public async Task SaveAsync_StoresMetadata()
    {
        var intent = new Intent("Test", [], new IntentConfidence(0.8, "High"));
        var metadata = new Dictionary<string, object> { ["source"] = "rule-engine", ["version"] = "v1" };

        await _repo.SaveAsync("bs-1", intent, PolicyDecision.Allow, metadata: metadata);

        var results = await _repo.GetByBehaviorSpaceIdAsync("bs-1");
        Assert.NotNull(results[0].Metadata);
        Assert.Equal("rule-engine", results[0].Metadata!["source"]);
    }

    [Fact]
    public async Task SaveAsync_StoresEntityId()
    {
        var intent = new Intent("Test", [], new IntentConfidence(0.8, "High"));

        await _repo.SaveAsync("bs-1", intent, PolicyDecision.Allow, entityId: "entity-42");

        var results = await _repo.GetByBehaviorSpaceIdAsync("bs-1");
        Assert.Equal("entity-42", results[0].EntityId);
    }

    [Fact]
    public async Task SaveAsync_SetsRecordedAtToUtcNow()
    {
        var intent = new Intent("Test", [], new IntentConfidence(0.8, "High"));
        var before = DateTimeOffset.UtcNow;

        await _repo.SaveAsync("bs-1", intent, PolicyDecision.Allow);

        var after = DateTimeOffset.UtcNow;
        var results = await _repo.GetByBehaviorSpaceIdAsync("bs-1");
        Assert.InRange(results[0].RecordedAt, before, after);
    }

    [Fact]
    public async Task MultipleSaves_AllStored()
    {
        for (int i = 0; i < 10; i++)
        {
            var intent = new Intent($"Intent{i}", [], new IntentConfidence(0.5, "Medium"));
            await _repo.SaveAsync("bs-1", intent, PolicyDecision.Allow);
        }

        var results = await _repo.GetByBehaviorSpaceIdAsync("bs-1");
        Assert.Equal(10, results.Count);
    }

    [Fact]
    public async Task GetByBehaviorSpaceIdAsync_MultipleSpaces_Isolated()
    {
        var intent1 = new Intent("Space1", [], new IntentConfidence(0.8, "High"));
        var intent2 = new Intent("Space2", [], new IntentConfidence(0.8, "High"));
        await _repo.SaveAsync("bs-1", intent1, PolicyDecision.Allow);
        await _repo.SaveAsync("bs-2", intent2, PolicyDecision.Allow);

        var results1 = await _repo.GetByBehaviorSpaceIdAsync("bs-1");
        var results2 = await _repo.GetByBehaviorSpaceIdAsync("bs-2");

        Assert.Single(results1);
        Assert.Single(results2);
        Assert.Equal("Space1", results1[0].IntentName);
        Assert.Equal("Space2", results2[0].IntentName);
    }
}
