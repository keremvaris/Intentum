namespace Intentum.Analytics.Models;

/// <summary>
/// Intent-based profile for an entity: aggregate intent names and confidence distribution → labels.
/// Use for "user/system profile" derived from timeline (e.g. frequent researcher, quick decider).
/// </summary>
/// <param name="EntityId">Entity identifier (e.g. userId).</param>
/// <param name="Start">Start of the time window used.</param>
/// <param name="End">End of the time window used.</param>
/// <param name="Labels">Derived labels (e.g. "frequent_researcher", "high_confidence").</param>
/// <param name="TopIntents">Intent name → count (most frequent intents in the window).</param>
/// <param name="AverageConfidenceScore">Average confidence score in the window.</param>
/// <param name="HighConfidencePercent">Percentage of points with High (or Certain) confidence.</param>
/// <param name="PointCount">Number of timeline points in the window.</param>
public sealed record IntentProfile(
    string EntityId,
    DateTimeOffset Start,
    DateTimeOffset End,
    IReadOnlyList<string> Labels,
    IReadOnlyDictionary<string, int> TopIntents,
    double AverageConfidenceScore,
    double HighConfidencePercent,
    int PointCount);
