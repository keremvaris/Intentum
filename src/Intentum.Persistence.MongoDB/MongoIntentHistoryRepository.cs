using Intentum.Core.Intents;
using Intentum.Persistence.Repositories;
using Intentum.Persistence.Serialization;
using Intentum.Runtime.Policy;
using MongoDB.Driver;

namespace Intentum.Persistence.MongoDB;

/// <summary>
/// MongoDB implementation of IIntentHistoryRepository.
/// </summary>
public sealed class MongoIntentHistoryRepository : IIntentHistoryRepository
{
    private readonly IMongoCollection<IntentHistoryDocument> _collection;

    public MongoIntentHistoryRepository(IMongoDatabase database, string collectionName = "intenthistory")
    {
        _collection = database.GetCollection<IntentHistoryDocument>(collectionName);
    }

    public async Task<string> SaveAsync(
        string behaviorSpaceId,
        Intent intent,
        PolicyDecision decision,
        IReadOnlyDictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        var record = IntentHistoryRecord.Create(behaviorSpaceId, intent, decision, metadata);
        var doc = IntentHistoryDocument.From(record);
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
        var filter = Builders<IntentHistoryDocument>.Filter.And(
            Builders<IntentHistoryDocument>.Filter.Gte(d => d.RecordedAt, start),
            Builders<IntentHistoryDocument>.Filter.Lte(d => d.RecordedAt, end));
        var list = await _collection
            .Find(filter)
            .SortByDescending(d => d.RecordedAt)
            .ToListAsync(cancellationToken);
        return list.Select(d => d.ToRecord()).ToList();
    }
}
