using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Models;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

namespace Intentum.Sample.Blazor.Api;

/// <summary>
/// Stateless service: infers adaptive tutor (EdTech) intent from variant learning events.
/// </summary>
public sealed class AdaptiveTutorService
{
    private static IIntentModel BuildModel()
    {
        var rules = new List<Func<BehaviorSpace, RuleMatch?>>
        {
            space =>
            {
                var moduleComplete = space.Events.Any(e => e.Action == "Module_Complete" && e.Metadata?.TryGetValue("QuizScore", out var s) == true && Convert.ToDouble(s) >= 0.85);
                var quizHigh = space.Events.Any(e => e.Action == "Quiz_Attempt" && e.Metadata?.TryGetValue("OutcomeScore", out var o) == true && Convert.ToDouble(o) >= 0.85);
                if (moduleComplete || quizHigh)
                    return new RuleMatch("ReadyForNextModule", 0.88, "Module_Complete or Quiz high score");
                return null;
            },
            space =>
            {
                var idle = space.Events.Any(e => e.Action == "Session_Idle" && e.Metadata?.TryGetValue("IdleMinutes", out var m) == true && Convert.ToInt32(m) >= 15);
                var lowWatch = space.Events.Any(e => e.Action == "Video_Play" && e.Metadata?.TryGetValue("WatchPercent", out var p) == true && Convert.ToInt32(p) < 30);
                if (idle && lowWatch)
                    return new RuleMatch("LosingMotivationDueToPace", 0.84, "Session_Idle + low WatchPercent");
                return null;
            },
            space =>
            {
                var videoLoop = space.Events.Any(e => e.Action == "Video_Play" && e.Metadata?.TryGetValue("LoopCount", out var l) == true && Convert.ToInt32(l) >= 2);
                var logicFlaw = space.Events.Any(e => e.Action == "Quiz_Attempt" && e.Metadata?.TryGetValue("StrugglePattern", out var sp) == true && string.Equals(sp.ToString(), "Logic_Flaw", StringComparison.OrdinalIgnoreCase));
                var forumPost = space.Events.Any(e => e.Action == "Forum_Post");
                if (videoLoop && logicFlaw && forumPost)
                    return new RuleMatch("ConceptualBlock_NeedsAlternativeExplanationAndPractice", 0.91, "Video Loop + Logic_Flaw + Forum_Post");
                return null;
            },
            space =>
            {
                var timeout = space.Events.Any(e => e.Action == "Quiz_Attempt" && e.Metadata?.TryGetValue("StrugglePattern", out var sp) == true && string.Equals(sp.ToString(), "Timeout", StringComparison.OrdinalIgnoreCase));
                var lowWatch = space.Events.Any(e => e.Action == "Video_Play" && e.Metadata?.TryGetValue("WatchPercent", out var p) == true && Convert.ToInt32(p) < 50);
                if (timeout && lowWatch)
                    return new RuleMatch("SurfaceLevelUnderstanding_SeekingQuickAnswer", 0.82, "Timeout + low WatchPercent");
                return null;
            }
        };
        return new RuleBasedIntentModel(rules);
    }

    private static readonly IntentPolicy AdaptiveTutorPolicy = new IntentPolicyBuilder()
        .Block("ConceptualBlock", i => i.Name.Contains("ConceptualBlock", StringComparison.OrdinalIgnoreCase))
        .RequireAuth("ExtraModule", i => i.Name.Contains("ConceptualBlock", StringComparison.OrdinalIgnoreCase) || i.Name.Contains("LosingMotivation", StringComparison.OrdinalIgnoreCase))
        .Warn("Educator", i => i.Name.Contains("ConceptualBlock", StringComparison.OrdinalIgnoreCase) || i.Name.Contains("SurfaceLevel", StringComparison.OrdinalIgnoreCase) || i.Name.Contains("LosingMotivation", StringComparison.OrdinalIgnoreCase))
        .Allow("Ready", i => i.Name.Contains("ReadyForNextModule", StringComparison.OrdinalIgnoreCase))
        .Observe("Default", _ => true)
        .Build();

    private readonly IIntentModel _model = BuildModel();

    public AdaptiveTutorInferResult Infer(string variant)
    {
        var baseTime = DateTimeOffset.UtcNow;
        var space = AdaptiveTutorVariants.BuildSpace(variant, baseTime);
        var intent = _model.Infer(space);
        var decision = intent.Decide(AdaptiveTutorPolicy);
        var events = AdaptiveTutorVariants.GetEvents(variant, baseTime);
        var blockedQuiz = decision.ToString() == "Block" ? "Quiz 4" : null;
        var suggestedModule = (decision.ToString() == "RequireAuth" || decision.ToString() == "Block") && intent.Name.Contains("ConceptualBlock", StringComparison.OrdinalIgnoreCase) ? "Görsel Akış Diyagramları" : null;
        return new AdaptiveTutorInferResult(
            intent.Name,
            intent.Confidence.Level,
            intent.Confidence.Score,
            decision.ToString(),
            blockedQuiz,
            suggestedModule,
            events.Select(e => e.Summary).ToList()
        );
    }
}

/// <summary>Response for POST /api/adaptive-tutor/infer.</summary>
public sealed record AdaptiveTutorInferResult(
    string IntentName,
    string ConfidenceLevel,
    double ConfidenceScore,
    string Decision,
    string? BlockedQuiz,
    string? SuggestedModule,
    IReadOnlyList<string> EventsSummary
);

/// <summary>Request body for POST /api/adaptive-tutor/infer.</summary>
public sealed record AdaptiveTutorInferRequest(string? Variant);
