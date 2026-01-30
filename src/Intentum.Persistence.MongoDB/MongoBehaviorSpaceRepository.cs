using System.Text.Json;
using Intentum.Core.Behavior;
using Intentum.Persistence.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Intentum.Persistence.MongoDB;

/// <summary>
/// MongoDB implementation of IBehaviorSpaceRepository.
/// </summary>
public sealed class MongoBehaviorSpaceRepository : IBehaviorSpaceRepository
{
    private readonly IMongoCollection<BehaviorSpaceDoc> _collection;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public MongoBehaviorSpaceRepository(IMongoDatabase database, string collectionName = "behaviorspaces")
    {
        _collection = database.GetCollection<BehaviorSpaceDoc>(collectionName);
    }

    public async Task<string> SaveAsync(BehaviorSpace behaviorSpace, CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid().ToString();
        var doc = BehaviorSpaceDoc.From(behaviorSpace, id);
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
        var filter = Builders<BehaviorSpaceDoc>.Filter.And(
            Builders<BehaviorSpaceDoc>.Filter.Exists("metadata." + key),
            Builders<BehaviorSpaceDoc>.Filter.Eq("metadata." + key, BsonValue.Create(value)));
        var list = await _collection.Find(filter).ToListAsync(cancellationToken);
        return list.Select(d => d.ToBehaviorSpace()).ToList();
    }

    public async Task<IReadOnlyList<BehaviorSpace>> GetByTimeWindowAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<BehaviorSpaceDoc>.Filter.And(
            Builders<BehaviorSpaceDoc>.Filter.Gte("createdAt", start.UtcDateTime),
            Builders<BehaviorSpaceDoc>.Filter.Lte("createdAt", end.UtcDateTime));
        var list = await _collection.Find(filter).ToListAsync(cancellationToken);
        return list.Select(d => d.ToBehaviorSpace()).ToList();
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var result = await _collection.DeleteOneAsync(d => d.Id == id, cancellationToken);
        return result.DeletedCount > 0;
    }

    private sealed class BehaviorSpaceDoc
    {
        public string Id { get; set; } = "";
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public string MetadataJson { get; set; } = "{}";
        public List<BehaviorEventDoc> Events { get; set; } = [];

        public static BehaviorSpaceDoc From(BehaviorSpace space, string id)
        {
            return new BehaviorSpaceDoc
            {
                Id = id,
                CreatedAt = DateTimeOffset.UtcNow,
                MetadataJson = JsonSerializer.Serialize(space.Metadata, JsonOptions),
                Events = space.Events.Select(BehaviorEventDoc.From).ToList()
            };
        }

        public BehaviorSpace ToBehaviorSpace()
        {
            var space = new BehaviorSpace();
            var metadata = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(MetadataJson);
            if (metadata != null)
            {
                foreach (var kv in metadata)
                    space.SetMetadata(kv.Key, kv.Value.ValueKind == JsonValueKind.Number ? kv.Value.GetDouble() : kv.Value.GetString() ?? kv.Value.ToString());
            }
            foreach (var evt in Events.OrderBy(e => e.OccurredAt))
                space.Observe(evt.ToBehaviorEvent());
            return space;
        }
    }

    private sealed class BehaviorEventDoc
    {
        public string Actor { get; set; } = "";
        public string Action { get; set; } = "";
        public DateTimeOffset OccurredAt { get; set; }
        public string MetadataJson { get; set; } = "{}";

        public static BehaviorEventDoc From(BehaviorEvent e)
        {
            return new BehaviorEventDoc
            {
                Actor = e.Actor,
                Action = e.Action,
                OccurredAt = e.OccurredAt,
                MetadataJson = e.Metadata != null ? JsonSerializer.Serialize(e.Metadata, JsonOptions) : "{}"
            };
        }

        public BehaviorEvent ToBehaviorEvent()
        {
            var metadata = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(MetadataJson);
            object? meta = null;
            if (metadata != null && metadata.Count > 0)
            {
                var dict = new Dictionary<string, object>();
                foreach (var kv in metadata)
                    dict[kv.Key] = kv.Value.ValueKind == JsonValueKind.Number ? kv.Value.GetDouble() : kv.Value.GetString() ?? kv.Value.ToString();
                meta = dict;
            }
            return new BehaviorEvent(Actor, Action, OccurredAt, (IReadOnlyDictionary<string, object>?)meta);
        }
    }
}
