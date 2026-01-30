namespace Intentum.Core.Intents;

/// <summary>
/// Inferred user/system intent derived from observed behavior.
/// </summary>
/// <param name="Name">Intent name (e.g. from rule or AI).</param>
/// <param name="Signals">Indicators that contributed to this intent.</param>
/// <param name="Confidence">Confidence score and level.</param>
/// <param name="Reasoning">Optional human-readable explanation (e.g. which rule matched, or short rationale).</param>
public sealed record Intent(
    string Name,
    IReadOnlyCollection<IntentSignal> Signals,
    IntentConfidence Confidence,
    string? Reasoning = null
);
