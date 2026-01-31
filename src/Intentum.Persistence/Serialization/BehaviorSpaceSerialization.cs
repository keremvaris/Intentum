using System.Text.Json;
using Intentum.Core.Behavior;

namespace Intentum.Persistence.Serialization;

/// <summary>
/// Shared JSON options for behavior space serialization across MongoDB and Redis.
/// </summary>
public static class BehaviorSpaceSerialization
{
    public static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
}

/// <summary>
/// Document shape for behavior space (MongoDB and Redis).
/// </summary>
public sealed class BehaviorSpaceDocument
{
    public string Id { get; set; } = "";
    public DateTimeOffset? CreatedAt { get; set; }
    public string MetadataJson { get; set; } = "{}";
    public List<BehaviorEventDocument> Events { get; set; } = [];

    public static BehaviorSpaceDocument From(BehaviorSpace space, string id, DateTimeOffset? createdAt = null)
    {
        return new BehaviorSpaceDocument
        {
            Id = id,
            CreatedAt = createdAt,
            MetadataJson = JsonSerializer.Serialize(space.Metadata, BehaviorSpaceSerialization.JsonOptions),
            Events = space.Events.Select(BehaviorEventDocument.From).ToList()
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

/// <summary>
/// Document shape for a single behavior event (MongoDB and Redis).
/// </summary>
public sealed class BehaviorEventDocument
{
    public string Actor { get; set; } = "";
    public string Action { get; set; } = "";
    public DateTimeOffset OccurredAt { get; set; }
    public string MetadataJson { get; set; } = "{}";

    public static BehaviorEventDocument From(BehaviorEvent e)
    {
        return new BehaviorEventDocument
        {
            Actor = e.Actor,
            Action = e.Action,
            OccurredAt = e.OccurredAt,
            MetadataJson = e.Metadata != null ? JsonSerializer.Serialize(e.Metadata, BehaviorSpaceSerialization.JsonOptions) : "{}"
        };
    }

    public BehaviorEvent ToBehaviorEvent()
    {
        var metadata = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(MetadataJson);
        object? meta = null;
        if (metadata is { Count: > 0 })
        {
            var dict = new Dictionary<string, object>();
            foreach (var kv in metadata)
                dict[kv.Key] = kv.Value.ValueKind == JsonValueKind.Number ? kv.Value.GetDouble() : kv.Value.GetString() ?? kv.Value.ToString();
            meta = dict;
        }
        return new BehaviorEvent(Actor, Action, OccurredAt, (IReadOnlyDictionary<string, object>?)meta);
    }
}
