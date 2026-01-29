using System.Text.Json;
using Intentum.Core.Intents;
using Intentum.Persistence.Repositories;
using Intentum.Runtime.Policy;
using StackExchange.Redis;

namespace Intentum.Persistence.Redis;

/// <summary>
/// Redis implementation of IIntentHistoryRepository.
/// Stores intent history records as JSON with key prefix and secondary index by behavior space ID.
/// </summary>
public sealed class RedisIntentHistoryRepository : IIntentHistoryRepository
{
    private readonly IConnectionMultiplexer _redis;
    private readonly string _keyPrefix;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public RedisIntentHistoryRepository(IConnectionMultiplexer redis, string keyPrefix = "intentum:inthistory:")
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _keyPrefix = keyPrefix ?? "intentum:inthistory:";
    }

    public async Task<string> SaveAsync(
        string behaviorSpaceId,
        Intent intent,
        PolicyDecision decision,
        CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid().ToString();
        var record = new IntentHistoryRecord(
            Id: id,
            BehaviorSpaceId: behaviorSpaceId,
            IntentName: intent.Name,
            ConfidenceLevel: intent.Confidence.Level,
            ConfidenceScore: intent.Confidence.Score,
            Decision: decision,
            RecordedAt: DateTimeOffset.UtcNow);
        var db = _redis.GetDatabase();
        var json = JsonSerializer.Serialize(IntentHistoryDto.From(record), JsonOptions);
        await db.StringSetAsync(_keyPrefix + "record:" + id, json);
        await db.SetAddAsync(_keyPrefix + "bybehaviorspace:" + behaviorSpaceId, id);
        await db.SetAddAsync(_keyPrefix + "ids", id);
        return id;
    }

    public async Task<IReadOnlyList<IntentHistoryRecord>> GetByBehaviorSpaceIdAsync(
        string behaviorSpaceId,
        CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var ids = await db.SetMembersAsync(_keyPrefix + "bybehaviorspace:" + behaviorSpaceId);
        var list = new List<IntentHistoryRecord>();
        foreach (var id in ids)
        {
            var record = await GetByIdAsync(id!, cancellationToken);
            if (record != null)
                list.Add(record);
        }
        return list.OrderByDescending(r => r.RecordedAt).ToList();
    }

    public async Task<IReadOnlyList<IntentHistoryRecord>> GetByConfidenceLevelAsync(
        string confidenceLevel,
        CancellationToken cancellationToken = default)
    {
        var ids = await _redis.GetDatabase().SetMembersAsync(_keyPrefix + "ids");
        var list = new List<IntentHistoryRecord>();
        foreach (var id in ids)
        {
            var record = await GetByIdAsync(id!, cancellationToken);
            if (record != null && record.ConfidenceLevel == confidenceLevel)
                list.Add(record);
        }
        return list.OrderByDescending(r => r.RecordedAt).ToList();
    }

    public async Task<IReadOnlyList<IntentHistoryRecord>> GetByDecisionAsync(
        PolicyDecision decision,
        CancellationToken cancellationToken = default)
    {
        var ids = await _redis.GetDatabase().SetMembersAsync(_keyPrefix + "ids");
        var list = new List<IntentHistoryRecord>();
        foreach (var id in ids)
        {
            var record = await GetByIdAsync(id!, cancellationToken);
            if (record != null && record.Decision == decision)
                list.Add(record);
        }
        return list.OrderByDescending(r => r.RecordedAt).ToList();
    }

    public async Task<IReadOnlyList<IntentHistoryRecord>> GetByTimeWindowAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default)
    {
        var ids = await _redis.GetDatabase().SetMembersAsync(_keyPrefix + "ids");
        var list = new List<IntentHistoryRecord>();
        foreach (var id in ids)
        {
            var record = await GetByIdAsync(id!, cancellationToken);
            if (record != null && record.RecordedAt >= start && record.RecordedAt <= end)
                list.Add(record);
        }
        return list.OrderByDescending(r => r.RecordedAt).ToList();
    }

    private async Task<IntentHistoryRecord?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var db = _redis.GetDatabase();
        var json = await db.StringGetAsync(_keyPrefix + "record:" + id);
        if (json.IsNullOrEmpty)
            return null;
        var dto = JsonSerializer.Deserialize<IntentHistoryDto>(json!.ToString()!);
        return dto?.ToRecord();
    }

    private sealed class IntentHistoryDto
    {
        public string Id { get; set; } = "";
        public string BehaviorSpaceId { get; set; } = "";
        public string IntentName { get; set; } = "";
        public string ConfidenceLevel { get; set; } = "";
        public double ConfidenceScore { get; set; }
        public string Decision { get; set; } = "";
        public DateTimeOffset RecordedAt { get; set; }
        public string MetadataJson { get; set; } = "{}";

        public static IntentHistoryDto From(IntentHistoryRecord record)
        {
            return new IntentHistoryDto
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
            return new IntentHistoryRecord(
                Id,
                BehaviorSpaceId,
                IntentName,
                ConfidenceLevel,
                ConfidenceScore,
                Enum.Parse<PolicyDecision>(Decision),
                RecordedAt,
                Metadata: null);
        }
    }
}
