using Intentum.Runtime.Policy;

namespace Intentum.Analytics.Models;

/// <summary>
/// A single point on an intent timeline (one recorded inference at a point in time).
/// </summary>
/// <param name="RecordedAt">When the intent was recorded.</param>
/// <param name="IntentName">Name of the inferred intent.</param>
/// <param name="ConfidenceLevel">Confidence level (e.g. High, Medium, Low).</param>
/// <param name="ConfidenceScore">Numeric confidence score.</param>
/// <param name="Decision">Policy decision for this intent.</param>
public sealed record IntentTimelinePoint(
    DateTimeOffset RecordedAt,
    string IntentName,
    string ConfidenceLevel,
    double ConfidenceScore,
    PolicyDecision Decision);
