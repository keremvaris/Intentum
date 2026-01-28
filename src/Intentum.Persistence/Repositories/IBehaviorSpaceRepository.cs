using Intentum.Core.Behavior;

namespace Intentum.Persistence.Repositories;

/// <summary>
/// Repository interface for persisting and querying behavior spaces.
/// </summary>
public interface IBehaviorSpaceRepository
{
    /// <summary>
    /// Saves a behavior space.
    /// </summary>
    Task<string> SaveAsync(BehaviorSpace behaviorSpace, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a behavior space by ID.
    /// </summary>
    Task<BehaviorSpace?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets behavior spaces by metadata key-value pair.
    /// </summary>
    Task<IReadOnlyList<BehaviorSpace>> GetByMetadataAsync(
        string key,
        object value,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets behavior spaces within a time window.
    /// </summary>
    Task<IReadOnlyList<BehaviorSpace>> GetByTimeWindowAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a behavior space by ID.
    /// </summary>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
