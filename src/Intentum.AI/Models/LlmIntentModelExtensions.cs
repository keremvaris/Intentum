using Intentum.Core.Behavior;
using Intentum.Core.Intents;

namespace Intentum.AI.Models;

/// <summary>
/// Extension methods for <see cref="LlmIntentModel"/> to infer with <see cref="ToVectorOptions"/> (e.g. cap dimensions for large event sets).
/// </summary>
public static class LlmIntentModelExtensions
{
    /// <summary>
    /// Infers intent using a behavior vector built with the given options (e.g. <see cref="ToVectorOptions.CapPerDimension"/>).
    /// Use to reduce embedding calls and memory for large event sets.
    /// </summary>
    /// <param name="model">The LLM intent model.</param>
    /// <param name="behaviorSpace">The observed behavior space.</param>
    /// <param name="toVectorOptions">Options for building the vector (cap/normalization); when null, same as <see cref="LlmIntentModel.Infer"/> with no precomputed vector.</param>
    /// <param name="precomputedVector">Optional pre-computed vector; when non-null, <paramref name="toVectorOptions"/> is ignored.</param>
    public static Intent Infer(
        this LlmIntentModel model,
        BehaviorSpace behaviorSpace,
        ToVectorOptions? toVectorOptions,
        BehaviorVector? precomputedVector = null)
    {
        var vector = precomputedVector ?? behaviorSpace.ToVector(toVectorOptions);
        return model.Infer(behaviorSpace, vector);
    }
}
