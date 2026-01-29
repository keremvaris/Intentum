using Intentum.Core.Contracts;
using Intentum.Runtime.Policy;

namespace Intentum.Experiments;

/// <summary>
/// A single variant in an A/B experiment (model + policy).
/// </summary>
/// <param name="Name">Variant name (e.g. control, test).</param>
/// <param name="Model">Intent model for this variant.</param>
/// <param name="Policy">Policy for this variant.</param>
public sealed record ExperimentVariant(
    string Name,
    IIntentModel Model,
    IntentPolicy Policy);
