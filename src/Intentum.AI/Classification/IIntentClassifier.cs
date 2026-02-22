using Intentum.Core.Behavior;

namespace Intentum.AI.Classification;

/// <summary>
/// Classifies intent from behavior using an external LLM or classification service.
/// Returns structured results with intent name, confidence, and reasoning.
/// </summary>
public interface IIntentClassifier
{
    Task<IntentClassificationResult> ClassifyAsync(
        BehaviorSpace behaviorSpace,
        IReadOnlyList<string>? candidateIntents = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Structured result from an LLM-based intent classification.
/// </summary>
public sealed record IntentClassificationResult(
    string IntentName,
    double Confidence,
    string? Reasoning = null
);
