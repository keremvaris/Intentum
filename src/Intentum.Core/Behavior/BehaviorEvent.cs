namespace Intentum.Core.Behavior;

/// <summary>
/// Single observed behavior event in the system.
/// </summary>
public sealed record BehaviorEvent(
    string Actor,
    string Action,
    DateTimeOffset OccurredAt,
    IReadOnlyDictionary<string, object>? Metadata = null
);
