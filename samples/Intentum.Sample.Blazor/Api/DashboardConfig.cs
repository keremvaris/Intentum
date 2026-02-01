namespace Intentum.Sample.Blazor.Api;

/// <summary>
/// Dashboard runtime config (confidence threshold, sliding window, decay, provider).
/// </summary>
public sealed record DashboardConfig(
    double ConfidenceThreshold = 0.6,
    int SlidingWindowMinutes = 60,
    double DecayFactor = 0.95,
    string Provider = "Local");

/// <summary>
/// Request body for PUT /api/dashboard/config (all optional).
/// </summary>
public sealed record DashboardConfigRequest(
    double? ConfidenceThreshold = null,
    int? SlidingWindowMinutes = null,
    double? DecayFactor = null,
    string? Provider = null);
