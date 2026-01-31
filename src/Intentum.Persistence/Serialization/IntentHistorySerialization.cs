using System.Text.Json;
using Intentum.Persistence.Repositories;
using Intentum.Runtime.Policy;

namespace Intentum.Persistence.Serialization;

/// <summary>
/// Shared JSON options and document type for intent history across MongoDB and Redis.
/// </summary>
public static class IntentHistorySerialization
{
    public static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
}

/// <summary>
/// Document shape for intent history (MongoDB and Redis).
/// </summary>
public sealed class IntentHistoryDocument
{
    public string Id { get; set; } = "";
    public string BehaviorSpaceId { get; set; } = "";
    public string IntentName { get; set; } = "";
    public string ConfidenceLevel { get; set; } = "";
    public double ConfidenceScore { get; set; }
    public string Decision { get; set; } = "";
    public DateTimeOffset RecordedAt { get; set; }
    public string MetadataJson { get; set; } = "{}";

    public static IntentHistoryDocument From(IntentHistoryRecord record)
    {
        return new IntentHistoryDocument
        {
            Id = record.Id,
            BehaviorSpaceId = record.BehaviorSpaceId,
            IntentName = record.IntentName,
            ConfidenceLevel = record.ConfidenceLevel,
            ConfidenceScore = record.ConfidenceScore,
            Decision = record.Decision.ToString(),
            RecordedAt = record.RecordedAt,
            MetadataJson = record.Metadata != null ? JsonSerializer.Serialize(record.Metadata, IntentHistorySerialization.JsonOptions) : "{}"
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
