namespace Intentum.Analytics.Models;

/// <summary>
/// An edge in the intent graph: transition from one intent node to another.
/// Used for node-edge visualization and flow analysis.
/// </summary>
/// <param name="FromNodeId">Source node identifier.</param>
/// <param name="ToNodeId">Target node identifier.</param>
/// <param name="TransitionAt">When the transition occurred.</param>
/// <param name="Weight">Optional weight (e.g. count of such transitions).</param>
public sealed record IntentGraphEdge(
    string FromNodeId,
    string ToNodeId,
    DateTimeOffset TransitionAt,
    double Weight = 1.0);
