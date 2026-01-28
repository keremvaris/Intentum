using Intentum.Runtime.Policy;

namespace Intentum.Analytics.Models;

/// <summary>
/// Distribution of policy decisions in a time window.
/// </summary>
public sealed record DecisionDistributionReport(
    DateTimeOffset Start,
    DateTimeOffset End,
    int TotalCount,
    IReadOnlyDictionary<PolicyDecision, int> CountByDecision
);
