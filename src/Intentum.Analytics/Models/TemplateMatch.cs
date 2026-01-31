namespace Intentum.Analytics.Models;

/// <summary>
/// Result of matching a pattern to an intent template.
/// </summary>
/// <param name="TemplateName">Name of the template.</param>
/// <param name="Score">Match score 0–1 (e.g. overlap or sequence similarity).</param>
/// <param name="MatchedIntentNames">Intent names from the pattern that matched.</param>
public sealed record TemplateMatch(
    string TemplateName,
    double Score,
    IReadOnlyList<string> MatchedIntentNames);
