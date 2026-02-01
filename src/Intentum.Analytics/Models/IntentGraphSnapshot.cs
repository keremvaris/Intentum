namespace Intentum.Analytics.Models;

/// <summary>
/// A time-based snapshot of the intent graph: nodes and edges within a time window.
/// Used for dashboard visualization and historical analysis.
/// </summary>
/// <param name="EntityId">Entity identifier (e.g. userId, sessionId).</param>
/// <param name="WindowStart">Start of the snapshot window.</param>
/// <param name="WindowEnd">End of the snapshot window.</param>
/// <param name="Nodes">Intent nodes in this snapshot.</param>
/// <param name="Edges">Transitions between nodes in this snapshot.</param>
/// <param name="SnapshotAt">When this snapshot was taken.</param>
public sealed record IntentGraphSnapshot(
    string EntityId,
    DateTimeOffset WindowStart,
    DateTimeOffset WindowEnd,
    IReadOnlyList<IntentGraphNode> Nodes,
    IReadOnlyList<IntentGraphEdge> Edges,
    DateTimeOffset SnapshotAt);
