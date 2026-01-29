using Intentum.Core.Behavior;
using System.Text.Json;

namespace Intentum.Persistence.EntityFramework.Entities;

/// <summary>
/// Entity Framework entity for BehaviorSpace.
/// </summary>
public sealed class BehaviorSpaceEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string MetadataJson { get; set; } = "{}";

    public ICollection<BehaviorEventEntity> Events { get; set; } = new List<BehaviorEventEntity>();

    public static BehaviorSpaceEntity FromBehaviorSpace(BehaviorSpace behaviorSpace)
    {
        var entity = new BehaviorSpaceEntity
        {
            CreatedAt = DateTimeOffset.UtcNow,
            MetadataJson = JsonSerializer.Serialize(behaviorSpace.Metadata)
        };

        foreach (var evt in behaviorSpace.Events)
        {
            entity.Events.Add(BehaviorEventEntity.FromBehaviorEvent(evt));
        }

        return entity;
    }

    public BehaviorSpace ToBehaviorSpace()
    {
        var space = new BehaviorSpace();

        // Restore metadata
        var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(MetadataJson)
            ?? new Dictionary<string, object>();
        foreach (var kvp in metadata)
        {
            space.SetMetadata(kvp.Key, kvp.Value);
        }

        // Restore events
        foreach (var evtEntity in Events.OrderBy(e => e.Sequence))
        {
            space.Observe(evtEntity.ToBehaviorEvent());
        }

        return space;
    }
}

/// <summary>
/// Entity Framework entity for BehaviorEvent.
/// </summary>
public sealed class BehaviorEventEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string BehaviorSpaceId { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; }
    public int Sequence { get; set; }
    public string MetadataJson { get; set; } = "{}";

    public BehaviorSpaceEntity BehaviorSpace { get; set; } = null!;

    public static BehaviorEventEntity FromBehaviorEvent(BehaviorEvent behaviorEvent, int sequence = 0)
    {
        return new BehaviorEventEntity
        {
            Actor = behaviorEvent.Actor,
            Action = behaviorEvent.Action,
            OccurredAt = behaviorEvent.OccurredAt,
            Sequence = sequence,
            MetadataJson = behaviorEvent.Metadata != null
                ? JsonSerializer.Serialize(behaviorEvent.Metadata)
                : "{}"
        };
    }

    public BehaviorEvent ToBehaviorEvent()
    {
        var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(MetadataJson);
        return new BehaviorEvent(
            Actor,
            Action,
            OccurredAt,
            metadata);
    }
}
