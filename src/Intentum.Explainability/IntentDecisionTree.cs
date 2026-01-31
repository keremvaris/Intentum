using Intentum.Runtime.Policy;

namespace Intentum.Explainability;

/// <summary>
/// Root of an intent decision tree: the final policy decision and matched rule.
/// </summary>
/// <param name="Decision">Policy decision for the intent.</param>
/// <param name="MatchedRuleName">Name of the rule that matched, or null if default (Observe).</param>
/// <param name="Intent">Summary of the inferred intent (name, confidence).</param>
/// <param name="Signals">Contributing signals (behavior dimensions / intent signals).</param>
/// <param name="BehaviorSummary">Optional summary of behavior events that led to the intent (e.g. from metadata).</param>
public sealed record IntentDecisionTree(
    PolicyDecision Decision,
    string? MatchedRuleName,
    IntentTreeIntentSummary Intent,
    IReadOnlyList<IntentTreeSignalNode> Signals,
    string? BehaviorSummary = null);

/// <summary>
/// Summary of the inferred intent for the tree (name and confidence).
/// </summary>
public sealed record IntentTreeIntentSummary(
    string Name,
    string ConfidenceLevel,
    double ConfidenceScore);

/// <summary>
/// A signal node in the intent tree (source, description, weight).
/// </summary>
public sealed record IntentTreeSignalNode(
    string Source,
    string Description,
    double Weight);
