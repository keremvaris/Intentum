using Intentum.Core.Behavior;
using Intentum.Persistence.EntityFramework.Entities;
using Intentum.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Intentum.Persistence.EntityFramework;

/// <summary>
/// Entity Framework Core implementation of IBehaviorSpaceRepository.
/// </summary>
public sealed class BehaviorSpaceRepository : IBehaviorSpaceRepository
{
    private readonly IntentumDbContext _context;

    public BehaviorSpaceRepository(IntentumDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<string> SaveAsync(BehaviorSpace behaviorSpace, CancellationToken cancellationToken = default)
    {
        var entity = BehaviorSpaceEntity.FromBehaviorSpace(behaviorSpace);
        _context.BehaviorSpaces.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public async Task<BehaviorSpace?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.BehaviorSpaces
            .Include(bs => bs.Events)
            .FirstOrDefaultAsync(bs => bs.Id == id, cancellationToken);

        return entity?.ToBehaviorSpace();
    }

    public async Task<IReadOnlyList<BehaviorSpace>> GetByMetadataAsync(
        string key,
        object value,
        CancellationToken cancellationToken = default)
    {
        // Load all and filter in memory (EF Core doesn't support JSON queries easily without extensions)
        var entities = await _context.BehaviorSpaces
            .Include(bs => bs.Events)
            .ToListAsync(cancellationToken);

        var filtered = entities.Where(bs =>
        {
            try
            {
                var metadata = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(bs.MetadataJson);
                return metadata != null &&
                       metadata.ContainsKey(key) &&
                       metadata[key].ToString() == value.ToString();
            }
            catch
            {
                return false;
            }
        }).ToList();

        return filtered.Select(e => e.ToBehaviorSpace()).ToList();
    }

    public async Task<IReadOnlyList<BehaviorSpace>> GetByTimeWindowAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default)
    {
        var entities = await _context.BehaviorSpaces
            .Include(bs => bs.Events)
            .Where(bs => bs.CreatedAt >= start && bs.CreatedAt <= end)
            .ToListAsync(cancellationToken);

        return entities.Select(e => e.ToBehaviorSpace()).ToList();
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.BehaviorSpaces.FindAsync([id], cancellationToken);
        if (entity == null)
            return false;

        _context.BehaviorSpaces.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
