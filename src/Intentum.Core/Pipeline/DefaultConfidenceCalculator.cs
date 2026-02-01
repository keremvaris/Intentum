using Intentum.Core.Intents;

namespace Intentum.Core.Pipeline;

/// <summary>
/// Default confidence calculator: uses IntentConfidence.FromScore (Low/Medium/High/Certain thresholds).
/// </summary>
public sealed class DefaultConfidenceCalculator : IConfidenceCalculator
{
    /// <inheritdoc />
    public IntentConfidence FromScore(double score)
        => IntentConfidence.FromScore(score);
}
