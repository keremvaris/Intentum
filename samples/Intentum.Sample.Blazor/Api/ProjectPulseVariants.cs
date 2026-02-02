namespace Intentum.Sample.Blazor.Api;

/// <summary>
/// Project Pulse demo variants (A/B/C/D): event sequences and expected intent/decision.
/// </summary>
public static class ProjectPulseVariants
{
    public const string VariantA = "A"; // Burnout risk
    public const string VariantB = "B"; // Healthy sprint
    public const string VariantC = "C"; // Scope creep
    public const string VariantD = "D"; // Dependency blocked

    /// <summary>Expected intent name per variant (for display).</summary>
    public static string GetExpectedIntent(string variant) => variant switch
    {
        VariantA => "TechnicalDebtCrisisAndTeamBurnoutImminent",
        VariantB => "TeamOnTrack",
        VariantC => "FeatureScopeBeginningToCreep",
        VariantD => "CriticalDependencyBeingNeglected",
        _ => "Unknown"
    };

    /// <summary>Expected decision per variant (for display).</summary>
    public static string GetExpectedDecision(string variant) => variant switch
    {
        VariantA => "Warn",
        VariantB => "Allow",
        VariantC => "RequireAuth",
        VariantD => "Escalate",
        _ => "Observe"
    };

    /// <summary>Number of steps (events) for the variant.</summary>
    public static int GetStepCount(string variant) => variant switch
    {
        VariantA => 4,
        VariantB => 3,
        VariantC => 4,
        _ => 3
    };

    /// <summary>Human-readable label for variant.</summary>
    public static string GetLabel(string variant) => variant switch
    {
        VariantA => "Burnout risk",
        VariantB => "Healthy sprint",
        VariantC => "Scope creep",
        VariantD => "Dependency blocked",
        _ => "Unknown"
    };
}

/// <summary>Request body for POST /api/project-pulse/start.</summary>
public sealed record ProjectPulseStartRequest(string? Variant);
