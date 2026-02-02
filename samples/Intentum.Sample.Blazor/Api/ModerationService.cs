using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Models;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

namespace Intentum.Sample.Blazor.Api;

/// <summary>
/// Stateless service: infers moderation intent from variant message sequence.
/// </summary>
public sealed class ModerationService
{
    private static IIntentModel BuildModel()
    {
        var rules = new List<Func<BehaviorSpace, RuleMatch?>>
        {
            space =>
            {
                var replies = space.Events.Where(e => e.Action == "Reply").ToList();
                var lowTone = space.Events.Count(e => e.Metadata?.TryGetValue("ToneScore", out var t) == true && t is double and < -0.5);
                var multipleTargets = replies.Select(e => e.Metadata?.TryGetValue("TargetUserId", out var u) == true ? u.ToString() : null).Where(x => x != null).Distinct().Count();
                if (lowTone >= 2 && multipleTargets >= 2)
                    return new RuleMatch("DeliberateProvocation_DerailingTechnicalDiscussion", 0.88, "Low tone + multiple targets");
                return null;
            },
            space =>
            {
                var replies = space.Events.Where(e => e.Action == "Reply").ToList();
                if (replies.Count < 2) return null;
                var targets = replies.Select(e => e.Metadata?.TryGetValue("TargetUserId", out var u) == true ? u.ToString() : null).Where(x => x != null).Distinct().ToList();
                var tones = space.Events.Where(e => e.Metadata?.TryGetValue("ToneScore", out _) == true).Select(e => Convert.ToDouble(e.Metadata!["ToneScore"])).ToList();
                if (targets.Count == 1 && tones.Count >= 2 && tones[^1] < tones[0])
                    return new RuleMatch("VeeringOffTopicIntoPersonalAttack", 0.85, "Reply chain same target, tone decreasing");
                return null;
            },
            space =>
            {
                var post = space.Events.FirstOrDefault(e => e.Action == "Post_Create");
                if (post == null) return null;
                var hasQuestions = post.Metadata?.TryGetValue("QuestionMarksCount", out var q) == true && q is int and >= 3;
                var longPost = post.Metadata?.TryGetValue("WordCount", out var w) == true && w is int and > 100;
                var replyTone = space.Events.Where(e => e.Action == "Reply").Any(e => e.Metadata?.TryGetValue("ToneScore", out var t) == true && t is double and < -0.3);
                if (hasQuestions && longPost && replyTone)
                    return new RuleMatch("GenuinelySeekingHelpButFrustrated", 0.84, "Long question post + frustrated reply");
                return null;
            },
            space =>
            {
                var hasCode = space.Events.Any(e => e.Metadata?.TryGetValue("ContainsCode", out var c) == true && c is true);
                var avgTone = space.Events.Where(e => e.Metadata?.TryGetValue("ToneScore", out _) == true).Select(e => Convert.ToDouble(e.Metadata!["ToneScore"])).DefaultIfEmpty(0).Average();
                if (hasCode && avgTone >= 0.3)
                    return new RuleMatch("ConstructiveTechnicalDebate", 0.82, "ContainsCode + positive tone");
                return null;
            }
        };
        return new RuleBasedIntentModel(rules);
    }

    private static readonly IntentPolicy ModerationPolicy = new IntentPolicyBuilder()
        .Warn("Trolling", i => i.Name.Contains("DeliberateProvocation", StringComparison.OrdinalIgnoreCase) || i.Name.Contains("Derailing", StringComparison.OrdinalIgnoreCase))
        .Warn("PersonalAttack", i => i.Name.Contains("PersonalAttack", StringComparison.OrdinalIgnoreCase))
        .Observe("SeekingHelp", i => i.Name.Contains("SeekingHelp", StringComparison.OrdinalIgnoreCase))
        .Allow("Constructive", i => i.Name.Contains("Constructive", StringComparison.OrdinalIgnoreCase))
        .Observe("Default", _ => true)
        .Build();

    private readonly IIntentModel _model = BuildModel();

    public ModerationInferResult Infer(string variant)
    {
        var baseTime = DateTimeOffset.UtcNow;
        var space = ModerationVariants.BuildSpace(variant, baseTime);
        var intent = _model.Infer(space);
        var decision = intent.Decide(ModerationPolicy);
        var events = ModerationVariants.GetEvents(variant, baseTime);
        var warningMessage = ModerationVariants.GetWarningMessage(variant);
        var observeNextCount = (decision.ToString() == "Warn" || decision.ToString() == "Observe") && !string.IsNullOrEmpty(warningMessage) ? 3 : 0;
        return new ModerationInferResult(
            intent.Name,
            intent.Confidence.Level,
            intent.Confidence.Score,
            decision.ToString(),
            warningMessage,
            observeNextCount,
            events.Select(e => e.Summary).ToList()
        );
    }
}

/// <summary>Response for POST /api/moderation/infer.</summary>
public sealed record ModerationInferResult(
    string IntentName,
    string ConfidenceLevel,
    double ConfidenceScore,
    string Decision,
    string WarningMessageTemplate,
    int ObserveNextCount,
    IReadOnlyList<string> EventsSummary
);

/// <summary>Request body for POST /api/moderation/infer.</summary>
public sealed record ModerationInferRequest(string? Variant);
