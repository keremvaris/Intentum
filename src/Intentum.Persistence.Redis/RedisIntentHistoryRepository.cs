using System.Text.Json;
using Intentum.Core.Intents;
using Intentum.Persistence.Repositories;
using Intentum.Persistence.Serialization;
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

    public RedisIntentHistoryRepository(IConnectionMultiplexer redis, string keyPrefix = "intentum:inthistory:")
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _keyPrefix = keyPrefix;
    }

    public async Task<string> SaveAsync(
        string behaviorSpaceId,
        Intent intent,
        PolicyDecision decision,
        IReadOnlyDictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        var record = IntentHistoryRecord.Create(behaviorSpaceId, intent, decision, metadata);
        var db = _redis.GetDatabase();
        var doc = IntentHistoryDocument.From(record);
        var json = JsonSerializer.Serialize(doc, IntentHistorySerialization.JsonOptions);
        await db.StringSetAsync(_keyPrefix + "record:" + record.Id, json);
        await db.SetAddAsync(_keyPrefix + "bybehaviorspace:" + behaviorSpaceId, record.Id);
        await db.SetAddAsync(_keyPrefix + "ids", record.Id);
        return record.Id;
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
            var record = await GetByIdAsync(id!);
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
            var record = await GetByIdAsync(id!);
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
            var record = await GetByIdAsync(id!);
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
            var record = await GetByIdAsync(id!);
            if (record != null && record.RecordedAt >= start && record.RecordedAt <= end)
                list.Add(record);
        }
        return list.OrderByDescending(r => r.RecordedAt).ToList();
    }

    private async Task<IntentHistoryRecord?> GetByIdAsync(string id)
    {
        var db = _redis.GetDatabase();
        var json = await db.StringGetAsync(_keyPrefix + "record:" + id);
        if (json.IsNullOrEmpty)
            return null;
        var doc = JsonSerializer.Deserialize<IntentHistoryDocument>(json!.ToString(), IntentHistorySerialization.JsonOptions);
        return doc?.ToRecord();
    }
}
