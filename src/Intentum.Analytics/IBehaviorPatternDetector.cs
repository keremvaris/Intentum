using Intentum.Analytics.Models;

namespace Intentum.Analytics;

/// <summary>
/// Detects behavior patterns and frequency anomalies from intent history, and matches patterns to intent templates.
/// </summary>
public interface IBehaviorPatternDetector
{
    /// <summary>
    /// Gets recurring intent-name sequences (e.g. "A then B") in the time window. Min sequence length 2, max 5.
    /// </summary>
    Task<IReadOnlyList<BehaviorPattern>> GetBehaviorPatternsAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        int minSequenceLength = 2,
        int maxSequenceLength = 5,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Detects frequency anomalies (e.g. intent X 10x more frequent than baseline in a bucket).
    /// </summary>
    Task<IReadOnlyList<PatternAnomalyReport>> GetPatternAnomaliesAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        TimeSpan bucketSize,
        double frequencyMultiplierThreshold = 10.0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Matches detected patterns to predefined intent templates; returns best matches per pattern.
    /// </summary>
    IReadOnlyList<TemplateMatch> MatchTemplates(
        IReadOnlyList<BehaviorPattern> patterns,
        IReadOnlyList<IntentTemplate> templates);
}
