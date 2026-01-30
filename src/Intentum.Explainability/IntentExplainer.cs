using Intentum.Core.Intents;

namespace Intentum.Explainability;

/// <summary>
/// Default implementation of intent explainability using signal weights.
/// </summary>
public sealed class IntentExplainer : IIntentExplainer
{
    /// <inheritdoc />
    public IReadOnlyList<SignalContribution> GetSignalContributions(Intent intent)
    {
        if (intent.Signals.Count == 0)
            return Array.Empty<SignalContribution>();

        var total = intent.Signals.Sum(s => s.Weight);
        if (total <= 0)
            return intent.Signals.Select(s => new SignalContribution(s.Source, s.Description, s.Weight, 0)).ToList();

        return intent.Signals
            .Select(s => new SignalContribution(
                s.Source,
                s.Description,
                s.Weight,
                100.0 * s.Weight / total))
            .OrderByDescending(c => c.ContributionPercent)
            .ToList();
    }

    /// <inheritdoc />
    public string GetExplanation(Intent intent, int maxSignals = 5)
    {
        var level = intent.Confidence.Level;
        var score = intent.Confidence.Score;
        var contributions = GetSignalContributions(intent).Take(maxSignals).ToList();

        var parts = new List<string>
        {
            $"Intent \"{intent.Name}\" inferred with confidence {level} ({score:F2})."
        };
        if (contributions.Count > 0)
        {
            var top = string.Join("; ", contributions.Select(c =>
                $"{c.Description} ({c.ContributionPercent:F0}%)"));
            parts.Add($"Top contributors: {top}.");
        }
        if (!string.IsNullOrWhiteSpace(intent.Reasoning))
            parts.Add($"Reasoning: {intent.Reasoning}");
        return string.Join(" ", parts);
    }
}
