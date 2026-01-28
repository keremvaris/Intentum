namespace Intentum.Core.Intents;

/// <summary>
/// Intent indicators inferred from behavior, logs, or models.
/// </summary>
public sealed record IntentSignal(
    string Source,
    string Description,
    double Weight
);
