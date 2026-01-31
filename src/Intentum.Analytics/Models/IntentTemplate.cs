namespace Intentum.Analytics.Models;

/// <summary>
/// Predefined intent template for matching detected patterns (e.g. "Purchase funnel", "Suspicious retry").
/// </summary>
/// <param name="Name">Template name.</param>
/// <param name="Description">Human-readable description.</param>
/// <param name="ExpectedIntentNames">Expected intent names (order can matter for sequence matching).</param>
public sealed record IntentTemplate(
    string Name,
    string Description,
    IReadOnlyList<string> ExpectedIntentNames);
