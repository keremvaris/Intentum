using Intentum.Core.Intents;

namespace Intentum.Core.Pipeline;

/// <summary>
/// Raw result of intent inference before confidence level is applied.
/// Used by the resolution pipeline to separate inference from confidence calculation.
/// </summary>
/// <param name="Name">Intent name (e.g. from rule or AI).</param>
/// <param name="Score">Raw confidence score in [0, 1].</param>
/// <param name="Signals">Indicators that contributed to this intent.</param>
/// <param name="Reasoning">Optional human-readable explanation.</param>
public sealed record IntentInferenceResult(
    string Name,
    double Score,
    IReadOnlyCollection<IntentSignal> Signals,
    string? Reasoning = null
);
