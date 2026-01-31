namespace Intentum.Analytics.Models;

/// <summary>
/// Intent timeline for an entity: time-ordered list of intent inference points.
/// </summary>
/// <param name="EntityId">Entity identifier (e.g. userId, behaviorSpaceId).</param>
/// <param name="Start">Start of the time window.</param>
/// <param name="End">End of the time window.</param>
/// <param name="Points">Time-ordered intent timeline points.</param>
public sealed record IntentTimeline(
    string EntityId,
    DateTimeOffset Start,
    DateTimeOffset End,
    IReadOnlyList<IntentTimelinePoint> Points);
