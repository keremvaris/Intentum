using Intentum.Core.Intents;
using Intentum.Persistence.Repositories;
using Intentum.Runtime.Policy;
using System.Text.Json;

namespace Intentum.Persistence.EntityFramework.Entities;

/// <summary>
/// Entity Framework entity for IntentHistoryRecord.
/// </summary>
public sealed class IntentHistoryEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string BehaviorSpaceId { get; set; } = string.Empty;
    public string IntentName { get; set; } = string.Empty;
    public string ConfidenceLevel { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public string Decision { get; set; } = string.Empty;
    public DateTimeOffset RecordedAt { get; set; } = DateTimeOffset.UtcNow;
    public string MetadataJson { get; set; } = "{}";

    public static IntentHistoryEntity FromRecord(IntentHistoryRecord record)
    {
        return new IntentHistoryEntity
        {
            Id = record.Id,
            BehaviorSpaceId = record.BehaviorSpaceId,
            IntentName = record.IntentName,
            ConfidenceLevel = record.ConfidenceLevel,
            ConfidenceScore = record.ConfidenceScore,
            Decision = record.Decision.ToString(),
            RecordedAt = record.RecordedAt,
            MetadataJson = record.Metadata != null 
                ? JsonSerializer.Serialize(record.Metadata) 
                : "{}"
        };
    }

    public IntentHistoryRecord ToRecord()
    {
        var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(MetadataJson);
        return new IntentHistoryRecord(
            Id,
            BehaviorSpaceId,
            IntentName,
            ConfidenceLevel,
            ConfidenceScore,
            Enum.Parse<PolicyDecision>(Decision),
            RecordedAt,
            metadata);
    }
}
