using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;

namespace Intentum.Core.Pipeline;

/// <summary>
/// Runs the intent resolution pipeline: signal → vector → inference → confidence.
/// Each step is pluggable (ISignalToVector, IIntentInferenceStep, IConfidenceCalculator).
/// Implements IIntentModel so it can be used wherever an intent model is required.
/// </summary>
public sealed class IntentResolutionPipeline : IIntentModel
{
    private readonly ISignalToVector _signalToVector;
    private readonly IIntentInferenceStep _inferenceStep;
    private readonly IConfidenceCalculator _confidenceCalculator;

    /// <summary>
    /// Creates a pipeline with the given steps. Pass null for any step to use the default.
    /// </summary>
    public IntentResolutionPipeline(
        IIntentInferenceStep inferenceStep,
        ISignalToVector? signalToVector = null,
        IConfidenceCalculator? confidenceCalculator = null)
    {
        _inferenceStep = inferenceStep ?? throw new ArgumentNullException(nameof(inferenceStep));
        _signalToVector = signalToVector ?? new DefaultSignalToVector();
        _confidenceCalculator = confidenceCalculator ?? new DefaultConfidenceCalculator();
    }

    /// <inheritdoc />
    public Intent Infer(BehaviorSpace behaviorSpace, BehaviorVector? precomputedVector = null)
    {
        var vector = precomputedVector ?? _signalToVector.ToVector(behaviorSpace);

        var result = _inferenceStep.Infer(behaviorSpace, vector);

        var score = Math.Clamp(result.Score, 0, 1);
        var confidence = _confidenceCalculator.FromScore(score);

        return new Intent(
            Name: result.Name,
            Signals: result.Signals,
            Confidence: confidence,
            Reasoning: result.Reasoning
        );
    }
}
