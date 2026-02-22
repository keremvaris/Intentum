using Intentum.Core.Behavior;

namespace Intentum.Core.UBA;

/// <summary>
/// Tracks a user's baseline behavior for deviation detection.
/// </summary>
public sealed class UserProfile
{
    public string UserId { get; }
    public DateTimeOffset CreatedAt { get; }
    public int TotalSessions { get; private set; }
    public Dictionary<string, int> ActionFrequency { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<DateTimeOffset> ActivityTimestamps { get; } = [];

    public UserProfile(string userId)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Learns from a behavior space, updating the baseline profile.
    /// </summary>
    public void Learn(BehaviorSpace space)
    {
        TotalSessions++;
        foreach (var evt in space.Events)
        {
            ActionFrequency.TryGetValue(evt.Action, out var count);
            ActionFrequency[evt.Action] = count + 1;
            ActivityTimestamps.Add(evt.OccurredAt);
        }
    }

    /// <summary>
    /// Calculates a deviation score [0-1] between the current behavior and the baseline.
    /// Higher values indicate more unusual behavior.
    /// </summary>
    public double CalculateDeviation(BehaviorSpace currentSpace)
    {
        if (TotalSessions == 0 || ActionFrequency.Count == 0)
            return 0;

        var totalActions = ActionFrequency.Values.Sum();
        var unknownActionCount = 0;
        var knownActionDeviation = 0.0;

        foreach (var evt in currentSpace.Events)
        {
            if (!ActionFrequency.TryGetValue(evt.Action, out var baselineCount))
            {
                unknownActionCount++;
                continue;
            }

            var normalizedBaseline = baselineCount / (double)totalActions;
            knownActionDeviation += Math.Abs(1.0 / currentSpace.Events.Count - normalizedBaseline);
        }

        if (currentSpace.Events.Count == 0)
            return 0;

        var unknownRatio = unknownActionCount / (double)currentSpace.Events.Count;
        var deviation = (unknownRatio * 0.7) + (Math.Min(knownActionDeviation, 1.0) * 0.3);

        return Math.Clamp(deviation, 0, 1);
    }
}
