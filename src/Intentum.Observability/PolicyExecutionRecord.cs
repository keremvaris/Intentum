using Intentum.Runtime.Policy;

namespace Intentum.Observability;

/// <summary>
/// Record of a single policy evaluation for logging and correlation (matched rule, intent name, decision, duration).
/// Use with <see cref="ObservablePolicyEngine.DecideWithExecutionLog"/> to log policy execution and failure trace.
/// </summary>
/// <param name="IntentName">Name of the intent that was evaluated.</param>
/// <param name="MatchedRuleName">Name of the rule that matched, or null if none.</param>
/// <param name="Decision">Policy decision (Allow, Block, Observe, etc.).</param>
/// <param name="DurationMs">Evaluation duration in milliseconds.</param>
/// <param name="Success">True if evaluation completed without exception.</param>
/// <param name="ExceptionMessage">Exception message when Success is false; null otherwise.</param>
/// <param name="ExceptionTrace">Stack trace when Success is false; null otherwise.</param>
public sealed record PolicyExecutionRecord(
    string IntentName,
    string? MatchedRuleName,
    PolicyDecision Decision,
    double DurationMs,
    bool Success = true,
    string? ExceptionMessage = null,
    string? ExceptionTrace = null);
