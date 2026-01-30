using Intentum.Core.Intents;
using Intentum.Runtime.Policy;

namespace Intentum.Experiments;

/// <summary>
/// Result of running one behavior space through an experiment variant.
/// </summary>
/// <param name="VariantName">Name of the variant (e.g. control, test).</param>
/// <param name="BehaviorSpaceId">Optional behavior space identifier.</param>
/// <param name="Intent">Inferred intent.</param>
/// <param name="Decision">Policy decision.</param>
public sealed record ExperimentResult(
    string VariantName,
    string? BehaviorSpaceId,
    Intent Intent,
    PolicyDecision Decision);
