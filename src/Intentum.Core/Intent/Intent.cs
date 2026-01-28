using Intentum.Core.Intent;

namespace Intentum.Core.Intents;

/// <summary>
/// Inferred user/system intent derived from observed behavior.
/// </summary>
public sealed record Intent(
    string Name,
    IReadOnlyCollection<IntentSignal> Signals,
    IntentConfidence Confidence
);
