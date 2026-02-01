namespace Intentum.Analytics.Models;

/// <summary>
/// A node in the intent graph: one intent at a point in time with confidence metadata.
/// Used for node-edge visualization and time-based snapshots.
/// </summary>
/// <param name="Id">Unique node identifier (e.g. entityId:timestamp or inference id).</param>
/// <param name="IntentName">Name of the inferred intent.</param>
/// <param name="ConfidenceScore">Numeric confidence score (0–1).</param>
/// <param name="ConfidenceLevel">Confidence level (e.g. Low, Medium, High, Certain).</param>
/// <param name="RecordedAt">When the intent was recorded.</param>
public sealed record IntentGraphNode(
    string Id,
    string IntentName,
    double ConfidenceScore,
    string ConfidenceLevel,
    DateTimeOffset RecordedAt);
