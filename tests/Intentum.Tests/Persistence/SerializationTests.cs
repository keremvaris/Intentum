using System.Text.Json;
using Intentum.Core.Behavior;
using Intentum.Core.Intents;
using Intentum.Persistence.Repositories;
using Intentum.Persistence.Serialization;
using Intentum.Runtime.Policy;

namespace Intentum.Tests.Persistence;

public sealed class SerializationTests
{
    [Fact]
    public void BehaviorSpaceDocument_RoundTrip_PreservesMetadata()
    {
        var space = new BehaviorSpace();
        space.SetMetadata("session", "abc123");
        space.SetMetadata("count", 42.0);
        space.Observe(new BehaviorEvent("user", "login", DateTimeOffset.UtcNow));

        var doc = BehaviorSpaceDocument.From(space, "id-1", DateTimeOffset.UtcNow);
        var roundTripped = doc.ToBehaviorSpace();

        Assert.Equal("abc123", roundTripped.Metadata["session"]);
        Assert.Equal(42.0, roundTripped.Metadata["count"]);
    }

    [Fact]
    public void BehaviorSpaceDocument_RoundTrip_PreservesEvents()
    {
        var time = DateTimeOffset.UtcNow;
        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("user1", "login", time));
        space.Observe(new BehaviorEvent("user1", "click", time.AddSeconds(1)));

        var doc = BehaviorSpaceDocument.From(space, "id-1", DateTimeOffset.UtcNow);
        var roundTripped = doc.ToBehaviorSpace();

        Assert.Equal(2, roundTripped.Events.Count);
        Assert.Equal("user1", roundTripped.Events.First().Actor);
        Assert.Equal("login", roundTripped.Events.First().Action);
    }

    [Fact]
    public void BehaviorSpaceDocument_RoundTrip_PreservesId()
    {
        var space = new BehaviorSpace();
        var doc = BehaviorSpaceDocument.From(space, "custom-id-123");

        Assert.Equal("custom-id-123", doc.Id);
    }

    [Fact]
    public void BehaviorSpaceDocument_RoundTrip_PreservesCreatedAt()
    {
        var now = DateTimeOffset.UtcNow;
        var space = new BehaviorSpace();
        var doc = BehaviorSpaceDocument.From(space, "id-1", now);

        Assert.Equal(now, doc.CreatedAt);
    }

    [Fact]
    public void BehaviorSpaceDocument_RoundTrip_EmptySpace()
    {
        var space = new BehaviorSpace();
        var doc = BehaviorSpaceDocument.From(space, "id-1");
        var roundTripped = doc.ToBehaviorSpace();

        Assert.Empty(roundTripped.Metadata);
        Assert.Empty(roundTripped.Events);
    }

    [Fact]
    public void BehaviorEventDocument_RoundTrip_PreservesData()
    {
        var time = DateTimeOffset.UtcNow;
        var evt = new BehaviorEvent("actor1", "action1", time);

        var doc = BehaviorEventDocument.From(evt);
        var roundTripped = doc.ToBehaviorEvent();

        Assert.Equal("actor1", roundTripped.Actor);
        Assert.Equal("action1", roundTripped.Action);
        Assert.Equal(time, roundTripped.OccurredAt);
    }

    [Fact]
    public void BehaviorEventDocument_RoundTrip_WithMetadata()
    {
        var metadata = new Dictionary<string, object> { ["key"] = "value" };
        var time = DateTimeOffset.UtcNow;
        var evt = new BehaviorEvent("actor1", "action1", time, metadata);

        var doc = BehaviorEventDocument.From(evt);
        var roundTripped = doc.ToBehaviorEvent();

        Assert.NotNull(roundTripped.Metadata);
        Assert.Equal("value", roundTripped.Metadata!["key"]);
    }

    [Fact]
    public void IntentHistoryDocument_RoundTrip_PreservesAllFields()
    {
        var intent = new Intent("TestIntent", [], new IntentConfidence(0.8, "High"), "because");
        var metadata = new Dictionary<string, object> { ["source"] = "ai" };
        var record = IntentHistoryRecord.Create("bs-1", intent, PolicyDecision.Allow, metadata, "entity-1");

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
    public void IntentHistoryDocument_RoundTrip_PreservesMetadata()
    {
        var intent = new Intent("Test", [], new IntentConfidence(0.5, "Medium"));
        var metadata = new Dictionary<string, object> { ["k"] = "v", ["n"] = 42 };
        var record = IntentHistoryRecord.Create("bs-1", intent, PolicyDecision.Warn, metadata);

        var doc = IntentHistoryDocument.From(record);
        var roundTripped = doc.ToRecord();

        Assert.NotNull(roundTripped.Metadata);
        // Metadata values are deserialized as JsonElement from JSON
        var kValue = roundTripped.Metadata!["k"];
        Assert.Equal("v", kValue.ToString());
    }

    [Fact]
    public void IntentHistoryDocument_RoundTrip_EmptyMetadata_ProducesNull()
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

    [Fact]
    public void IntentHistoryDocument_RoundTrip_AllDecisionTypes()
    {
        foreach (PolicyDecision decision in Enum.GetValues<PolicyDecision>())
        {
            var intent = new Intent("Test", [], new IntentConfidence(0.5, "Medium"));
            var record = IntentHistoryRecord.Create("bs-1", intent, decision);

            var doc = IntentHistoryDocument.From(record);
            var roundTripped = doc.ToRecord();

            Assert.Equal(decision, roundTripped.Decision);
        }
    }

    [Fact]
    public void BehaviorSpaceDocument_JsonSerialization_ProducesValidJson()
    {
        var space = new BehaviorSpace();
        space.SetMetadata("key", "value");
        space.Observe(new BehaviorEvent("user", "action", DateTimeOffset.UtcNow));

        var doc = BehaviorSpaceDocument.From(space, "id-1");
        var json = JsonSerializer.Serialize(doc, BehaviorSpaceSerialization.JsonOptions);

        Assert.NotNull(json);
        Assert.Contains("id-1", json);
        Assert.Contains("key", json);
    }

    [Fact]
    public void IntentHistoryDocument_JsonSerialization_ProducesValidJson()
    {
        var intent = new Intent("Test", [], new IntentConfidence(0.8, "High"));
        var record = IntentHistoryRecord.Create("bs-1", intent, PolicyDecision.Allow);

        var doc = IntentHistoryDocument.From(record);
        var json = JsonSerializer.Serialize(doc, IntentHistorySerialization.JsonOptions);

        Assert.NotNull(json);
        Assert.Contains("bs-1", json);
        Assert.Contains("Test", json);
    }

    [Fact]
    public void BehaviorSpaceDocument_MultipleEvents_PreservedInOrder()
    {
        var t1 = DateTimeOffset.UtcNow.AddMinutes(-2);
        var t2 = DateTimeOffset.UtcNow.AddMinutes(-1);
        var t3 = DateTimeOffset.UtcNow;

        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("u", "a1", t1));
        space.Observe(new BehaviorEvent("u", "a2", t2));
        space.Observe(new BehaviorEvent("u", "a3", t3));

        var doc = BehaviorSpaceDocument.From(space, "id-1");
        var roundTripped = doc.ToBehaviorSpace();

        Assert.Equal(3, roundTripped.Events.Count);
        Assert.Equal("a1", roundTripped.Events.ElementAt(0).Action);
        Assert.Equal("a2", roundTripped.Events.ElementAt(1).Action);
        Assert.Equal("a3", roundTripped.Events.ElementAt(2).Action);
    }

    [Fact]
    public void IntentHistoryRecord_Create_GeneratesUniqueId()
    {
        var intent = new Intent("Test", [], new IntentConfidence(0.5, "Medium"));
        var record1 = IntentHistoryRecord.Create("bs-1", intent, PolicyDecision.Allow);
        var record2 = IntentHistoryRecord.Create("bs-1", intent, PolicyDecision.Allow);

        Assert.NotEqual(record1.Id, record2.Id);
    }

    [Fact]
    public void IntentHistoryRecord_Create_SetsRecordedAtToUtcNow()
    {
        var intent = new Intent("Test", [], new IntentConfidence(0.5, "Medium"));
        var before = DateTimeOffset.UtcNow;

        var record = IntentHistoryRecord.Create("bs-1", intent, PolicyDecision.Allow);

        var after = DateTimeOffset.UtcNow;
        Assert.InRange(record.RecordedAt, before, after);
    }
}
