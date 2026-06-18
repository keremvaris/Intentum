using Intentum.Core.Behavior;
using Intentum.Persistence.Repositories;

namespace Intentum.Tests.Helpers;

internal sealed class InMemoryBehaviorSpaceRepository : IBehaviorSpaceRepository
{
    private readonly Dictionary<string, BehaviorSpace> _spaces = new();
    private int _counter;

    public Task<string> SaveAsync(BehaviorSpace behaviorSpace, CancellationToken cancellationToken = default)
    {
        var id = $"bs-{++_counter}";
        _spaces[id] = behaviorSpace;
        return Task.FromResult(id);
    }

    public Task<BehaviorSpace?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _spaces.TryGetValue(id, out var space);
        return Task.FromResult(space);
    }

    public Task<IReadOnlyList<BehaviorSpace>> GetByMetadataAsync(
        string key,
        object value,
        CancellationToken cancellationToken = default)
    {
        var results = _spaces.Values
            .Where(s => s.Metadata.TryGetValue(key, out var v) && v.Equals(value))
            .ToList();
        return Task.FromResult<IReadOnlyList<BehaviorSpace>>(results);
    }

    public Task<IReadOnlyList<BehaviorSpace>> GetByTimeWindowAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default)
    {
        var results = _spaces.Values
            .Where(s => s.Events.Any(e => e.OccurredAt >= start && e.OccurredAt <= end))
            .ToList();
        return Task.FromResult<IReadOnlyList<BehaviorSpace>>(results);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_spaces.Remove(id));
    }
}
