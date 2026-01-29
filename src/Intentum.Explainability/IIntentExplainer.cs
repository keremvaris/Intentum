using Intentum.Core.Intents;

namespace Intentum.Explainability;

/// <summary>
/// Explains how an intent was inferred (signal contributions, human-readable summary).
/// </summary>
public interface IIntentExplainer
{
    /// <summary>
    /// Gets signal contribution scores (each signal's share of the total weight).
    /// </summary>
    IReadOnlyList<SignalContribution> GetSignalContributions(Intent intent);

    /// <summary>
    /// Generates a short human-readable explanation of the intent inference.
    /// </summary>
    string GetExplanation(Intent intent, int maxSignals = 5);
}
