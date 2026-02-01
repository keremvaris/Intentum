using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;

namespace Intentum.Sample.Blazor.Api;

/// <summary>
/// Wraps an intent model and downgrades confidence by one level (Certain→High, High→Medium, Medium→Low, Low→Low).
/// Used in the Playground to show how different "model strictness" leads to different policy decisions (Allow vs Observe/Block).
/// </summary>
internal sealed class StrictConfidenceIntentModel(IIntentModel inner) : IIntentModel
{
    private readonly IIntentModel _inner = inner ?? throw new ArgumentNullException(nameof(inner));

    public Intent Infer(BehaviorSpace behaviorSpace, BehaviorVector? precomputedVector = null)
    {
        var intent = _inner.Infer(behaviorSpace, precomputedVector);
        var downgraded = DowngradeConfidence(intent.Confidence);
        return intent with { Confidence = downgraded };
    }

    private static IntentConfidence DowngradeConfidence(IntentConfidence c)
    {
        var (score, level) = c;
        var newLevel = level switch
        {
            "Certain" => "High",
            "High" => "Medium",
            _ => "Low"
        };
        var newScore = level switch
        {
            "Certain" => 0.8,
            "High" => 0.55,
            "Medium" => 0.25,
            _ => Math.Min(score, 0.25)
        };
        return new IntentConfidence(newScore, newLevel);
    }
}
