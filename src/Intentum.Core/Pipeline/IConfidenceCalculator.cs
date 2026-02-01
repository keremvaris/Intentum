using Intentum.Core.Intents;

namespace Intentum.Core.Pipeline;

/// <summary>
/// Maps a raw confidence score to an IntentConfidence (score + level).
/// Override this step to customize thresholds (e.g. Low/Medium/High/Certain boundaries).
/// </summary>
public interface IConfidenceCalculator
{
    /// <summary>Maps a numeric score (typically 0–1) to IntentConfidence with level.</summary>
    IntentConfidence FromScore(double score);
}
