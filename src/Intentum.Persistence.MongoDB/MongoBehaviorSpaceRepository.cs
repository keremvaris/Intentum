using Intentum.Core.Behavior;
using Intentum.Persistence.Repositories;
using Intentum.Persistence.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Intentum.Persistence.MongoDB;

/// <summary>
/// MongoDB implementation of IBehaviorSpaceRepository.
/// </summary>
public sealed class MongoBehaviorSpaceRepository : IBehaviorSpaceRepository
{
    private readonly IMongoCollection<BehaviorSpaceDocument> _collection;

    public MongoBehaviorSpaceRepository(IMongoDatabase database, string collectionName = "behaviorspaces")
    {
        _collection = database.GetCollection<BehaviorSpaceDocument>(collectionName);
    }

    public async Task<string> SaveAsync(BehaviorSpace behaviorSpace, CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid().ToString();
        var doc = BehaviorSpaceDocument.From(behaviorSpace, id, DateTimeOffset.UtcNow);
        await _collection.InsertOneAsync(doc, cancellationToken: cancellationToken);
        return id;
    }

    public async Task<BehaviorSpace?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var doc = await _collection.Find(d => d.Id == id).FirstOrDefaultAsync(cancellationToken);
        return doc?.ToBehaviorSpace();
    }

    public async Task<IReadOnlyList<BehaviorSpace>> GetByMetadataAsync(
        string key,
        object value,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<BehaviorSpaceDocument>.Filter.And(
            Builders<BehaviorSpaceDocument>.Filter.Exists("metadata." + key),
            Builders<BehaviorSpaceDocument>.Filter.Eq("metadata." + key, BsonValue.Create(value)));
        var list = await _collection.Find(filter).ToListAsync(cancellationToken);
        return list.Select(d => d.ToBehaviorSpace()).ToList();
    }

    public async Task<IReadOnlyList<BehaviorSpace>> GetByTimeWindowAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<BehaviorSpaceDocument>.Filter.And(
            Builders<BehaviorSpaceDocument>.Filter.Gte("createdAt", start.UtcDateTime),
            Builders<BehaviorSpaceDocument>.Filter.Lte("createdAt", end.UtcDateTime));
        var list = await _collection.Find(filter).ToListAsync(cancellationToken);
        return list.Select(d => d.ToBehaviorSpace()).ToList();
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var result = await _collection.DeleteOneAsync(d => d.Id == id, cancellationToken);
        return result.DeletedCount > 0;
    }
}
