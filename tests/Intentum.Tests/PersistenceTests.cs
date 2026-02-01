using Intentum.Core.Behavior;
using Intentum.Core.Intents;
using Intentum.Persistence.Repositories;
using Intentum.Persistence.Serialization;
using Intentum.Runtime.Audit;
using Intentum.Runtime.Policy;

namespace Intentum.Tests;

/// <summary>
/// Tests for Persistence: NoOpIntentAuditStore, BehaviorSpaceSerialization, BehaviorSpaceDocument, IntentHistorySerialization.
/// </summary>
public sealed class PersistenceTests
{
    [Fact]
    public async Task NoOpIntentAuditStore_AppendAsync_CompletesWithoutException()
    {
        var store = new NoOpIntentAuditStore();
        var evt = new IntentAuditEvent(
            InputHash: "abc",
            ModelVersion: "v1",
            PolicyVersion: "p1",
            UserOverride: false,
            RecordedAt: DateTimeOffset.UtcNow,
            IntentName: "Test",
            Decision: PolicyDecision.Allow);

        var task = store.AppendAsync(evt);
        await task;

        Assert.True(task.IsCompletedSuccessfully);
    }

    [Fact]
    public void BehaviorSpaceDocument_From_ToBehaviorSpace_RoundTrip_PreservesData()
    {
        var space = new BehaviorSpace();
        space.SetMetadata("key1", "value1");
        space.SetMetadata("key2", 42.0);
        space.Observe(new BehaviorEvent("actor1", "action1", DateTimeOffset.UtcNow.AddMinutes(-1)));
        space.Observe(new BehaviorEvent("actor2", "action2", DateTimeOffset.UtcNow));

        var doc = BehaviorSpaceDocument.From(space, "id-1", DateTimeOffset.UtcNow);
        var roundTripped = doc.ToBehaviorSpace();

        Assert.Equal(space.Metadata.Count, roundTripped.Metadata.Count);
        Assert.Equal(space.Events.Count, roundTripped.Events.Count);
        Assert.Equal("value1", roundTripped.Metadata.GetValueOrDefault("key1"));
        Assert.Equal(42.0, roundTripped.Metadata.GetValueOrDefault("key2"));
        Assert.Equal("actor1", roundTripped.Events.First().Actor);
        Assert.Equal("action1", roundTripped.Events.First().Action);
        Assert.Equal("actor2", roundTripped.Events.ElementAt(1).Actor);
    }

    [Fact]
    public void BehaviorSpaceDocument_ToBehaviorSpace_WhenMetadataJsonEmpty_ProducesEmptyMetadata()
    {
        var doc = new BehaviorSpaceDocument
        {
            Id = "x",
            MetadataJson = "{}",
            Events = []
        };

        var space = doc.ToBehaviorSpace();

        Assert.Empty(space.Metadata);
        Assert.Empty(space.Events);
    }

    [Fact]
    public void BehaviorSpaceDocument_ToBehaviorSpace_WhenMetadataJsonInvalid_Throws()
    {
        var doc = new BehaviorSpaceDocument
        {
            Id = "x",
            MetadataJson = "not valid json",
            Events = []
        };

        Assert.Throws<System.Text.Json.JsonException>(() => doc.ToBehaviorSpace());
    }

    [Fact]
    public void BehaviorEventDocument_From_ToBehaviorEvent_RoundTrip_PreservesData()
    {
        var evt = new BehaviorEvent("user", "login", DateTimeOffset.UtcNow);

        var doc = BehaviorEventDocument.From(evt);
        var roundTripped = doc.ToBehaviorEvent();

        Assert.Equal(evt.Actor, roundTripped.Actor);
        Assert.Equal(evt.Action, roundTripped.Action);
        Assert.Equal(evt.OccurredAt, roundTripped.OccurredAt);
    }

    [Fact]
    public void IntentHistoryDocument_From_ToRecord_RoundTrip_PreservesData()
    {
        var intent = new Intent("TestIntent", [], new IntentConfidence(0.8, "High"), "rule");
        var record = IntentHistoryRecord.Create("bs-1", intent, PolicyDecision.Allow, metadata: new Dictionary<string, object> { ["k"] = "v" }, entityId: "entity-1");

        var doc = IntentHistoryDocument.From(record);
        var roundTripped = doc.ToRecord();

        Assert.Equal(record.Id, roundTripped.Id);
        Assert.Equal(record.BehaviorSpaceId, roundTripped.BehaviorSpaceId);
        Assert.Equal(record.IntentName, roundTripped.IntentName);
        Assert.Equal(record.ConfidenceLevel, roundTripped.ConfidenceLevel);
        Assert.Equal(record.ConfidenceScore, roundTripped.ConfidenceScore);
        Assert.Equal(record.Decision, roundTripped.Decision);
        Assert.Equal(record.RecordedAt, roundTripped.RecordedAt);
        Assert.Equal(record.EntityId, roundTripped.EntityId);
    }

    [Fact]
    public void IntentHistoryDocument_ToRecord_WhenMetadataJsonEmpty_ProducesNullMetadata()
    {
        var doc = new IntentHistoryDocument
        {
            Id = "id1",
            BehaviorSpaceId = "bs1",
            IntentName = "I",
            ConfidenceLevel = "High",
            ConfidenceScore = 0.9,
            Decision = "Allow",
            RecordedAt = DateTimeOffset.UtcNow,
            MetadataJson = "{}"
        };

        var record = doc.ToRecord();

        Assert.Null(record.Metadata);
    }
}
