using Intentum.Core.Intents;

namespace Intentum.Runtime.Policy;

/// <summary>
/// A policy rule that evaluates both intent and context (e.g. system load, region).
/// </summary>
/// <param name="Name">Rule name for explainability.</param>
/// <param name="Condition">Condition over intent and policy context. First matching rule wins.</param>
/// <param name="Decision">Decision when the condition matches.</param>
public sealed record ContextAwarePolicyRule(
    string Name,
    Func<Intent, PolicyContext, bool> Condition,
    PolicyDecision Decision);
