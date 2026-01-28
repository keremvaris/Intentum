namespace Intentum.Explainability;

/// <summary>
/// Contribution of a single signal to the inferred intent.
/// </summary>
/// <param name="Source">Signal source (e.g. behavior key).</param>
/// <param name="Description">Signal description.</param>
/// <param name="Weight">Raw weight.</param>
/// <param name="ContributionPercent">Percentage of total weight (0–100).</param>
public sealed record SignalContribution(
    string Source,
    string Description,
    double Weight,
    double ContributionPercent);
