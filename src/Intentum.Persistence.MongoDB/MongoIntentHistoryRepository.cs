using System.Text.Json;
using Intentum.Core.Intents;
using Intentum.Persistence.Repositories;
using Intentum.Runtime.Policy;
using MongoDB.Driver;

namespace Intentum.Persistence.MongoDB;

/// <summary>
/// MongoDB implementation of IIntentHistoryRepository.
/// </summary>
public sealed class MongoIntentHistoryRepository : IIntentHistoryRepository
{
    private readonly IMongoCollection<IntentHistoryDoc> _collection;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public MongoIntentHistoryRepository(IMongoDatabase database, string collectionName = "intenthistory")
    {
        _collection = database.GetCollection<IntentHistoryDoc>(collectionName);
    }

    public async Task<string> SaveAsync(
        string behaviorSpaceId,
        Intent intent,
        PolicyDecision decision,
        IReadOnlyDictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        var record = IntentHistoryRecord.Create(behaviorSpaceId, intent, decision, metadata);
        var doc = IntentHistoryDoc.From(record);
        await _collection.InsertOneAsync(doc, cancellationToken: cancellationToken);
        return record.Id;
    }

    public async Task<IReadOnlyList<IntentHistoryRecord>> GetByBehaviorSpaceIdAsync(
        string behaviorSpaceId,
        CancellationToken cancellationToken = default)
    {
        var list = await _collection
            .Find(d => d.BehaviorSpaceId == behaviorSpaceId)
            .SortByDescending(d => d.RecordedAt)
            .ToListAsync(cancellationToken);
        return list.Select(d => d.ToRecord()).ToList();
    }

    public async Task<IReadOnlyList<IntentHistoryRecord>> GetByConfidenceLevelAsync(
        string confidenceLevel,
        CancellationToken cancellationToken = default)
    {
        var list = await _collection
            .Find(d => d.ConfidenceLevel == confidenceLevel)
            .SortByDescending(d => d.RecordedAt)
            .ToListAsync(cancellationToken);
        return list.Select(d => d.ToRecord()).ToList();
    }

    public async Task<IReadOnlyList<IntentHistoryRecord>> GetByDecisionAsync(
        PolicyDecision decision,
        CancellationToken cancellationToken = default)
    {
        var decisionStr = decision.ToString();
        var list = await _collection
            .Find(d => d.Decision == decisionStr)
            .SortByDescending(d => d.RecordedAt)
            .ToListAsync(cancellationToken);
        return list.Select(d => d.ToRecord()).ToList();
    }

    public async Task<IReadOnlyList<IntentHistoryRecord>> GetByTimeWindowAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<IntentHistoryDoc>.Filter.And(
            Builders<IntentHistoryDoc>.Filter.Gte(d => d.RecordedAt, start),
            Builders<IntentHistoryDoc>.Filter.Lte(d => d.RecordedAt, end));
        var list = await _collection
            .Find(filter)
            .SortByDescending(d => d.RecordedAt)
            .ToListAsync(cancellationToken);
        return list.Select(d => d.ToRecord()).ToList();
    }

    private sealed class IntentHistoryDoc
    {
        public string Id { get; set; } = "";
        public string BehaviorSpaceId { get; set; } = "";
        public string IntentName { get; set; } = "";
        public string ConfidenceLevel { get; set; } = "";
        public double ConfidenceScore { get; set; }
        public string Decision { get; set; } = "";
        public DateTimeOffset RecordedAt { get; set; }
        public string MetadataJson { get; set; } = "{}";

        public static IntentHistoryDoc From(IntentHistoryRecord record)
        {
            return new IntentHistoryDoc
            {
                Id = record.Id,
                BehaviorSpaceId = record.BehaviorSpaceId,
                IntentName = record.IntentName,
                ConfidenceLevel = record.ConfidenceLevel,
                ConfidenceScore = record.ConfidenceScore,
                Decision = record.Decision.ToString(),
                RecordedAt = record.RecordedAt,
                MetadataJson = record.Metadata != null ? JsonSerializer.Serialize(record.Metadata, JsonOptions) : "{}"
            };
        }

        public IntentHistoryRecord ToRecord()
        {
            var metadata = string.IsNullOrEmpty(MetadataJson) || MetadataJson == "{}"
                ? null
                : JsonSerializer.Deserialize<Dictionary<string, object>>(MetadataJson);
            return new IntentHistoryRecord(
                Id,
                BehaviorSpaceId,
                IntentName,
                ConfidenceLevel,
                ConfidenceScore,
                Enum.Parse<PolicyDecision>(Decision),
                RecordedAt,
                Metadata: metadata);
        }
    }
}
