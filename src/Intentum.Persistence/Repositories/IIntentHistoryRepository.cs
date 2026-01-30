using Intentum.Core.Intents;
using Intentum.Runtime.Policy;

namespace Intentum.Persistence.Repositories;

/// <summary>
/// Repository interface for storing intent inference results and policy decisions.
/// </summary>
public interface IIntentHistoryRepository
{
    /// <summary>
    /// Saves an intent inference result with policy decision.
    /// </summary>
    /// <param name="behaviorSpaceId">Identifier of the behavior space.</param>
    /// <param name="intent">The inferred intent.</param>
    /// <param name="decision">The policy decision.</param>
    /// <param name="metadata">Optional metadata (e.g. EventsSummary, Source) to show where the inference came from.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<string> SaveAsync(
        string behaviorSpaceId,
        Intent intent,
        PolicyDecision decision,
        IReadOnlyDictionary<string, object>? metadata = null,
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
