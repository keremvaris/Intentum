using System.Text.Json;
using Intentum.Core.Behavior;
using Intentum.Persistence.Repositories;
using Intentum.Persistence.Serialization;
using StackExchange.Redis;

namespace Intentum.Persistence.Redis;

/// <summary>
/// Redis implementation of IBehaviorSpaceRepository.
/// Stores behavior spaces as JSON with key prefix.
/// </summary>
public sealed class RedisBehaviorSpaceRepository : IBehaviorSpaceRepository
{
    private readonly IConnectionMultiplexer _redis;
    private readonly string _keyPrefix;

    public RedisBehaviorSpaceRepository(IConnectionMultiplexer redis, string keyPrefix = "intentum:behaviorspace:")
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _keyPrefix = keyPrefix;
    }

    public async Task<string> SaveAsync(BehaviorSpace behaviorSpace, CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid().ToString();
        var db = _redis.GetDatabase();
        var doc = BehaviorSpaceDocument.From(behaviorSpace, id);
        var json = JsonSerializer.Serialize(doc, BehaviorSpaceSerialization.JsonOptions);
        var key = _keyPrefix + id;
        await db.StringSetAsync(key, json);
        await db.SetAddAsync(_keyPrefix + "ids", id);
        return id;
    }

    public async Task<BehaviorSpace?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var json = await db.StringGetAsync(_keyPrefix + id);
        if (json.IsNullOrEmpty)
            return null;
        var doc = JsonSerializer.Deserialize<BehaviorSpaceDocument>(json!.ToString(), BehaviorSpaceSerialization.JsonOptions);
        return doc?.ToBehaviorSpace();
    }

    public async Task<IReadOnlyList<BehaviorSpace>> GetByMetadataAsync(
        string key,
        object value,
        CancellationToken cancellationToken = default)
    {
        var ids = await _redis.GetDatabase().SetMembersAsync(_keyPrefix + "ids");
        var list = new List<BehaviorSpace>();
        foreach (var id in ids)
        {
            var space = await GetByIdAsync(id!, cancellationToken);
            if (space != null && space.GetMetadata<object>(key)?.ToString() == value?.ToString())
                list.Add(space);
        }
        return list;
    }

    public async Task<IReadOnlyList<BehaviorSpace>> GetByTimeWindowAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default)
    {
        var ids = await _redis.GetDatabase().SetMembersAsync(_keyPrefix + "ids");
        var list = new List<BehaviorSpace>();
        foreach (var id in ids)
        {
            var space = await GetByIdAsync(id!, cancellationToken);
            if (space == null)
                continue;
            var events = space.Events;
            if (events.Count > 0)
            {
                var minTime = events.Min(e => e.OccurredAt);
                if (minTime >= start && minTime <= end)
                    list.Add(space);
            }
        }
        return list;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var key = _keyPrefix + id;
        var removed = await db.KeyDeleteAsync(key);
        await db.SetRemoveAsync(_keyPrefix + "ids", id);
        return removed;
    }
}
