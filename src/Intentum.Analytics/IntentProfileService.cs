using Intentum.Analytics.Models;

namespace Intentum.Analytics;

/// <summary>
/// Builds an intent-based profile for an entity from timeline data (aggregate intent names, confidence distribution → labels).
/// </summary>
public sealed class IntentProfileService : IIntentProfileService
{
    private readonly IIntentAnalytics _analytics;

    public IntentProfileService(IIntentAnalytics analytics)
    {
        _analytics = analytics ?? throw new ArgumentNullException(nameof(analytics));
    }

    /// <inheritdoc />
    public async Task<IntentProfile> GetProfileAsync(
        string entityId,
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default)
    {
        var timeline = await _analytics.GetIntentTimelineAsync(entityId, start, end, cancellationToken);
        var points = timeline.Points;

        if (points.Count == 0)
        {
            return new IntentProfile(
                entityId,
                start,
                end,
                [],
                new Dictionary<string, int>(),
                0,
                0,
                0);
        }

        var intentCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var totalScore = 0.0;
        var highCount = 0;

        foreach (var p in points)
        {
            intentCounts[p.IntentName] = intentCounts.GetValueOrDefault(p.IntentName, 0) + 1;
            totalScore += p.ConfidenceScore;
            if (p.ConfidenceLevel is "High" or "Certain")
                highCount++;
        }

        var avgScore = totalScore / points.Count;
        var highPct = 100.0 * highCount / points.Count;

        var topIntents = intentCounts
            .OrderByDescending(kv => kv.Value)
            .Take(10)
            .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);

        var labels = new List<string>();

        if (points.Count >= 5)
            labels.Add("active");
        if (avgScore >= 0.7)
            labels.Add("high_confidence");
        else if (avgScore < 0.4)
            labels.Add("low_confidence");
        if (highPct >= 70)
            labels.Add("quick_decider");
        if (intentCounts.Count >= 3 && points.Count >= 5)
            labels.Add("diverse_intents");

        var topIntentName = intentCounts.OrderByDescending(kv => kv.Value).FirstOrDefault().Key;
        if (!string.IsNullOrEmpty(topIntentName) && intentCounts[topIntentName] >= Math.Max(2, points.Count / 2))
            labels.Add($"frequent_{topIntentName.ToLowerInvariant().Replace(" ", "_")}");

        return new IntentProfile(
            entityId,
            start,
            end,
            labels,
            topIntents,
            avgScore,
            highPct,
            points.Count);
    }
}
