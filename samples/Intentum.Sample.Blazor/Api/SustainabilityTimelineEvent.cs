namespace Intentum.Sample.Blazor.Api;

/// <summary>
/// Single sustainability timeline event for SSE: simulated date, company, intent, confidence, granularity.
/// </summary>
public sealed record SustainabilityTimelineEvent(
    DateTimeOffset SimulatedAt,
    string CompanyId,
    string CompanyName,
    string IntentName,
    double ConfidenceScore,
    string Granularity);
