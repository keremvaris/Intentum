using Intentum.Core.Intents;
using Intentum.Runtime.Policy;
using Intent = Intentum.Core.Intents.Intent;

namespace Intentum.Persistence.Repositories;

/// <summary>
/// Repository interface for storing intent inference results and policy decisions.
/// </summary>
public interface IIntentHistoryRepository
{
    /// <summary>
    /// Saves an intent inference result with policy decision.
    /// </summary>
    Task<string> SaveAsync(
        string behaviorSpaceId,
        Intent intent,
        PolicyDecision decision,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets intent history by behavior space ID.
    /// </summary>
    Task<IReadOnlyList<IntentHistoryRecord>> GetByBehaviorSpaceIdAsync(
        string behaviorSpaceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets intent history by confidence level.
    /// </summary>
    Task<IReadOnlyList<IntentHistoryRecord>> GetByConfidenceLevelAsync(
        string confidenceLevel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets intent history by policy decision.
    /// </summary>
    Task<IReadOnlyList<IntentHistoryRecord>> GetByDecisionAsync(
        PolicyDecision decision,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets intent history within a time window.
    /// </summary>
    Task<IReadOnlyList<IntentHistoryRecord>> GetByTimeWindowAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Record representing a stored intent inference result.
/// </summary>
public sealed record IntentHistoryRecord(
    string Id,
    string BehaviorSpaceId,
    string IntentName,
    string ConfidenceLevel,
    double ConfidenceScore,
    PolicyDecision Decision,
    DateTimeOffset RecordedAt,
    IReadOnlyDictionary<string, object>? Metadata = null
);
