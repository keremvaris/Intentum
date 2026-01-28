using Intentum.Core.Intents;

namespace Intentum.Runtime.Policy;

/// <summary>
/// Single rule applied to an inferred intent.
/// </summary>
public sealed record PolicyRule(
    string Name,
    Func<Intent, bool> Condition,
    PolicyDecision Decision
);
