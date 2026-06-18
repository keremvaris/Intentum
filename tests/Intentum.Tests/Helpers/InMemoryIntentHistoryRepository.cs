using Intentum.Core.Intents;
using Intentum.Persistence.Repositories;
using Intentum.Runtime.Policy;

namespace Intentum.Tests.Helpers;

internal sealed class InMemoryIntentHistoryRepository : IIntentHistoryRepository
{
    private readonly List<IntentHistoryRecord> _records = [];

    public Task<string> SaveAsync(
        string behaviorSpaceId,
        Intent intent,
        PolicyDecision decision,
        IReadOnlyDictionary<string, object>? metadata = null,
        string? entityId = null,
        CancellationToken cancellationToken = default)
    {
        var record = IntentHistoryRecord.Create(behaviorSpaceId, intent, decision, metadata, entityId);
        _records.Add(record);
        return Task.FromResult(record.Id);
    }

    public Task<IReadOnlyList<IntentHistoryRecord>> GetByBehaviorSpaceIdAsync(
        string behaviorSpaceId,
        CancellationToken cancellationToken = default)
    {
        var results = _records.Where(r => r.BehaviorSpaceId == behaviorSpaceId).ToList();
        return Task.FromResult<IReadOnlyList<IntentHistoryRecord>>(results);
    }

    public Task<IReadOnlyList<IntentHistoryRecord>> GetByConfidenceLevelAsync(
        string confidenceLevel,
        CancellationToken cancellationToken = default)
    {
        var results = _records.Where(r => r.ConfidenceLevel == confidenceLevel).ToList();
        return Task.FromResult<IReadOnlyList<IntentHistoryRecord>>(results);
    }

    public Task<IReadOnlyList<IntentHistoryRecord>> GetByDecisionAsync(
        PolicyDecision decision,
        CancellationToken cancellationToken = default)
    {
        var results = _records.Where(r => r.Decision == decision).ToList();
        return Task.FromResult<IReadOnlyList<IntentHistoryRecord>>(results);
    }

    public Task<IReadOnlyList<IntentHistoryRecord>> GetByTimeWindowAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default)
    {
        var results = _records.Where(r => r.RecordedAt >= start && r.RecordedAt <= end).ToList();
        return Task.FromResult<IReadOnlyList<IntentHistoryRecord>>(results);
    }

    public Task<IReadOnlyList<IntentHistoryRecord>> GetByEntityIdAsync(
        string entityId,
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default)
    {
        var results = _records.Where(r =>
            r.EntityId == entityId &&
            r.RecordedAt >= start &&
            r.RecordedAt <= end).ToList();
        return Task.FromResult<IReadOnlyList<IntentHistoryRecord>>(results);
    }
}
