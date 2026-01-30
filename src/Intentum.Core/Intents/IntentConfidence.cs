namespace Intentum.Core.Intents;

/// <summary>
/// Confidence score for inferred intent.
/// </summary>
public sealed record IntentConfidence(
    double Score,
    string Level
)
{
    /// <summary>Maps a numeric score (0–1) to a confidence level (Low, Medium, High, Certain).</summary>
    public static IntentConfidence FromScore(double score)
    {
        var level = score switch
        {
            < 0.3 => "Low",
            < 0.6 => "Medium",
            < 0.85 => "High",
            _ => "Certain"
        };

        return new IntentConfidence(score, level);
    }
}
