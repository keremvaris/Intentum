using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Models;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

namespace Intentum.Sample.Blazor.Api;

/// <summary>
/// Stateless service: infers customer journey intent from variant events.
/// Uses RuleBasedIntentModel for deterministic demo intents.
/// </summary>
public sealed class CustomerJourneyService
{
    private static IIntentModel BuildModel()
    {
        var rules = new List<Func<BehaviorSpace, RuleMatch?>>
        {
            space =>
            {
                var hasCartAbandon = space.Events.Any(e => e.Action == "Cart_Abandon");
                var hasSupportChat = space.Events.Any(e => e.Action == "SupportChat_Open" && e.Metadata?.TryGetValue("PreviousTicketsCount", out var c) == true && c is int and >= 2);
                if (hasCartAbandon && hasSupportChat)
                    return new RuleMatch("OnTheVergeOfAbandoningBrandDueToFrustration", 0.88, "Cart_Abandon + SupportChat high tickets");
                return null;
            },
            space =>
            {
                var hasVideo = space.Events.Any(e => e.Action == "Video_Play" && e.Metadata?.TryGetValue("Topic", out var t) == true && string.Equals(t.ToString(), "education", StringComparison.OrdinalIgnoreCase));
                var hasBlog = space.Events.Any(e => e.Action == "BlogPost_View");
                if (hasVideo && hasBlog)
                    return new RuleMatch("ResearchingForAcademicPurpose", 0.85, "Video education + BlogPost");
                return null;
            },
            space =>
            {
                var hasCompare = space.Events.Any(e => e.Action == "PricingPage_ClickCompare");
                var hasEnterprise = space.Events.Any(e => e.Action == "PricingPage_View" && e.Metadata?.TryGetValue("PlanHovered", out var p) == true && string.Equals(p.ToString(), "Enterprise", StringComparison.OrdinalIgnoreCase));
                var hasLongBlog = space.Events.Any(e => e.Action == "BlogPost_View" && e.Metadata?.TryGetValue("DurationSeconds", out var d) == true && d is int and >= 60);
                if (hasCompare && (hasEnterprise || hasLongBlog))
                    return new RuleMatch("TechnicalDecisionMaker_ComparingEnterpriseSolutions", 0.90, "PricingPage Compare + Enterprise or long blog");
                return null;
            },
            space =>
            {
                var hasCartAdd = space.Events.Any(e => e.Action == "Cart_Add");
                var hasReviewScroll = space.Events.Any(e => e.Action == "ReviewSection_Scroll");
                var hasStarter = space.Events.Any(e => e.Action == "PricingPage_View" && e.Metadata?.TryGetValue("PlanHovered", out var p) == true && string.Equals(p.ToString(), "Starter", StringComparison.OrdinalIgnoreCase));
                if (hasCartAdd && (hasReviewScroll || hasStarter))
                    return new RuleMatch("ComparingPricesAggressively", 0.82, "Cart_Add + ReviewScroll or Starter");
                return null;
            }
        };
        return new RuleBasedIntentModel(rules);
    }

    private static readonly IntentPolicy CustomerJourneyPolicy = new IntentPolicyBuilder()
        .RequireAuth("TechnicalDemo", i => i.Name.Contains("Technical", StringComparison.OrdinalIgnoreCase) || i.Name.Contains("Enterprise", StringComparison.OrdinalIgnoreCase))
        .RequireAuth("AcademicLicense", i => i.Name.Contains("Academic", StringComparison.OrdinalIgnoreCase))
        .Escalate("Abandoning", i => i.Name.Contains("Abandoning", StringComparison.OrdinalIgnoreCase))
        .Allow("Default", _ => true)
        .Build();

    private readonly IIntentModel _model = BuildModel();

    public CustomerJourneyInferResult Infer(string variant)
    {
        var baseTime = DateTimeOffset.UtcNow;
        var space = CustomerJourneyVariants.BuildSpace(variant, baseTime);
        var intent = _model.Infer(space);
        var decision = intent.Decide(CustomerJourneyPolicy);
        var events = CustomerJourneyVariants.GetEvents(variant, baseTime);
        var journeyStage = variant switch
        {
            CustomerJourneyVariants.VariantA => "Comparison",
            CustomerJourneyVariants.VariantB => "Decision",
            CustomerJourneyVariants.VariantC => "Consideration",
            CustomerJourneyVariants.VariantD => "Decision",
            _ => "Awareness"
        };
        var suggestedContentType = decision.ToString() == "RequireAuth"
            ? (intent.Name.Contains("Technical", StringComparison.OrdinalIgnoreCase) ? "WhitepaperAndDemo" : "AcademicLicense")
            : (intent.Name.Contains("Abandoning", StringComparison.OrdinalIgnoreCase) ? "EscalateToSuccess" : "DefaultPopup");
        return new CustomerJourneyInferResult(
            intent.Name,
            intent.Confidence.Level,
            intent.Confidence.Score,
            decision.ToString(),
            journeyStage,
            suggestedContentType,
            events.Select(e => e.Summary).ToList()
        );
    }
}

/// <summary>Response for POST /api/customer-journey/infer.</summary>
public sealed record CustomerJourneyInferResult(
    string IntentName,
    string ConfidenceLevel,
    double ConfidenceScore,
    string Decision,
    string JourneyStage,
    string SuggestedContentType,
    IReadOnlyList<string> EventsSummary
);

/// <summary>Request body for POST /api/customer-journey/infer.</summary>
public sealed record CustomerJourneyInferRequest(string? Variant);
