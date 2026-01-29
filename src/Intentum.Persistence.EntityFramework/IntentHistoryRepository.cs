using Intentum.Core.Intents;
using Intentum.Persistence.EntityFramework.Entities;
using Intentum.Persistence.Repositories;
using Intentum.Runtime.Policy;
using Microsoft.EntityFrameworkCore;

namespace Intentum.Persistence.EntityFramework;

/// <summary>
/// Entity Framework Core implementation of IIntentHistoryRepository.
/// </summary>
public sealed class IntentHistoryRepository : IIntentHistoryRepository
{
    private readonly IntentumDbContext _context;

    public IntentHistoryRepository(IntentumDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<string> SaveAsync(
        string behaviorSpaceId,
        Intent intent,
        PolicyDecision decision,
        CancellationToken cancellationToken = default)
    {
        var record = new IntentHistoryRecord(
            Id: Guid.NewGuid().ToString(),
            BehaviorSpaceId: behaviorSpaceId,
            IntentName: intent.Name,
            ConfidenceLevel: intent.Confidence.Level,
            ConfidenceScore: intent.Confidence.Score,
            Decision: decision,
            RecordedAt: DateTimeOffset.UtcNow);

        var entity = IntentHistoryEntity.FromRecord(record);
        _context.IntentHistory.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public async Task<IReadOnlyList<IntentHistoryRecord>> GetByBehaviorSpaceIdAsync(
        string behaviorSpaceId,
        CancellationToken cancellationToken = default)
    {
        var entities = await _context.IntentHistory
            .Where(h => h.BehaviorSpaceId == behaviorSpaceId)
            .OrderByDescending(h => h.RecordedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(e => e.ToRecord()).ToList();
    }

    public async Task<IReadOnlyList<IntentHistoryRecord>> GetByConfidenceLevelAsync(
        string confidenceLevel,
        CancellationToken cancellationToken = default)
    {
        var entities = await _context.IntentHistory
            .Where(h => h.ConfidenceLevel == confidenceLevel)
            .OrderByDescending(h => h.RecordedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(e => e.ToRecord()).ToList();
    }

    public async Task<IReadOnlyList<IntentHistoryRecord>> GetByDecisionAsync(
        PolicyDecision decision,
        CancellationToken cancellationToken = default)
    {
        var entities = await _context.IntentHistory
            .Where(h => h.Decision == decision.ToString())
            .OrderByDescending(h => h.RecordedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(e => e.ToRecord()).ToList();
    }

    public async Task<IReadOnlyList<IntentHistoryRecord>> GetByTimeWindowAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default)
    {
        var entities = await _context.IntentHistory
            .Where(h => h.RecordedAt >= start && h.RecordedAt <= end)
            .OrderByDescending(h => h.RecordedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(e => e.ToRecord()).ToList();
    }
}
