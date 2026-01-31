using Intentum.Core.Intents;

namespace Intentum.Runtime.Policy;

/// <summary>
/// Context for policy evaluation: system load, region, recent intents, and custom data.
/// Used by context-aware policy rules to decide based on more than intent alone.
/// </summary>
/// <param name="Intent">The inferred intent being evaluated.</param>
/// <param name="SystemLoad">Optional system load (0–1 or metric). Used e.g. to Escalate when load is high.</param>
/// <param name="Region">Optional region or geography. Used e.g. for region-specific rules.</param>
/// <param name="RecentIntents">Optional recent intent summaries for this entity. Used e.g. to detect patterns.</param>
/// <param name="CustomContext">Optional custom key-value context from the application.</param>
public sealed record PolicyContext(
    Intent Intent,
    double? SystemLoad = null,
    string? Region = null,
    IReadOnlyList<IntentSummary>? RecentIntents = null,
    IReadOnlyDictionary<string, object>? CustomContext = null);

/// <summary>
/// Summary of a past intent (name, confidence) for context-aware policies.
/// </summary>
public sealed record IntentSummary(
    string Name,
    string ConfidenceLevel,
    double ConfidenceScore);
