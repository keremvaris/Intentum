using System.Text.Json;
using Intentum.Core.Behavior;
using Intentum.Persistence.Repositories;
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
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public RedisBehaviorSpaceRepository(IConnectionMultiplexer redis, string keyPrefix = "intentum:behaviorspace:")
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _keyPrefix = keyPrefix;
    }

    public async Task<string> SaveAsync(BehaviorSpace behaviorSpace, CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid().ToString();
        var db = _redis.GetDatabase();
        var dto = BehaviorSpaceDto.From(behaviorSpace, id);
        var json = JsonSerializer.Serialize(dto, JsonOptions);
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
        var dto = JsonSerializer.Deserialize<BehaviorSpaceDto>(json!.ToString());
        return dto?.ToBehaviorSpace();
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

    private sealed class BehaviorSpaceDto
    {
        public string Id { get; set; } = "";
        public string MetadataJson { get; set; } = "{}";
        public List<BehaviorEventDto> Events { get; set; } = [];

        public static BehaviorSpaceDto From(BehaviorSpace space, string id)
        {
            return new BehaviorSpaceDto
            {
                Id = id,
                MetadataJson = JsonSerializer.Serialize(space.Metadata, JsonOptions),
                Events = space.Events.Select(BehaviorEventDto.From).ToList()
            };
        }

        public BehaviorSpace ToBehaviorSpace()
        {
            var space = new BehaviorSpace();
            var metadata = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(MetadataJson);
            if (metadata != null)
            {
                foreach (var kv in metadata)
                    space.SetMetadata(kv.Key, kv.Value.ValueKind == JsonValueKind.Number ? kv.Value.GetDouble() : kv.Value.GetString() ?? (object)kv.Value.ToString());
            }
            foreach (var evt in Events.OrderBy(e => e.OccurredAt))
                space.Observe(evt.ToBehaviorEvent());
            return space;
        }
    }

    private sealed class BehaviorEventDto
    {
        public string Actor { get; set; } = "";
        public string Action { get; set; } = "";
        public DateTimeOffset OccurredAt { get; set; }
        public string MetadataJson { get; set; } = "{}";

        public static BehaviorEventDto From(BehaviorEvent e)
        {
            return new BehaviorEventDto
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
