using Intentum.Core.Behavior;

namespace Intentum.Sample.Blazor.Api;

/// <summary>
/// Customer Journey (Chameleon Campaign) demo variants: event sequences and expected intent.
/// </summary>
public static class CustomerJourneyVariants
{
    public const string VariantA = "A"; // Technical decision maker
    public const string VariantB = "B"; // Price-sensitive hobbyist
    public const string VariantC = "C"; // Academic research
    public const string VariantD = "D"; // Abandoning

    public static string GetExpectedIntent(string variant) => variant switch
    {
        VariantA => "TechnicalDecisionMaker_ComparingEnterpriseSolutions",
        VariantB => "ComparingPricesAggressively",
        VariantC => "ResearchingForAcademicPurpose",
        VariantD => "OnTheVergeOfAbandoningBrandDueToFrustration",
        _ => "Unknown"
    };

    public static string GetLabel(string variant) => variant switch
    {
        VariantA => "Technical decision maker",
        VariantB => "Price-sensitive hobbyist",
        VariantC => "Academic research",
        VariantD => "Abandoning",
        _ => "Unknown"
    };

    /// <summary>Builds BehaviorSpace from variant events.</summary>
    public static BehaviorSpace BuildSpace(string variant, DateTimeOffset baseTime)
    {
        var space = new BehaviorSpace();
        space.SetMetadata("Variant", variant);
        var events = GetEvents(variant, baseTime);
        foreach (var (evt, _) in events)
            space.Observe(evt);
        return space;
    }

    public static IReadOnlyList<(BehaviorEvent Evt, string Summary)> GetEvents(string variant, DateTimeOffset baseTime)
    {
        return variant switch
        {
            VariantA => new List<(BehaviorEvent, string)>
            {
                (new BehaviorEvent("customer", "BlogPost_View", baseTime, new Dictionary<string, object> { ["DurationSeconds"] = 90, ["PostId"] = "perf-optimization", ["ScrollDepthPercent"] = 95 }), "BlogPost (teknik makale, 90s)"),
                (new BehaviorEvent("customer", "PricingPage_View", baseTime.AddSeconds(100), new Dictionary<string, object> { ["DurationSeconds"] = 45, ["PlanHovered"] = "Enterprise" }), "PricingPage (Enterprise hover 45s)"),
                (new BehaviorEvent("customer", "PricingPage_ClickCompare", baseTime.AddSeconds(150), new Dictionary<string, object> { ["PlanA"] = "Enterprise", ["PlanB"] = "Business" }), "ClickCompare (Enterprise vs Business)")
            },
            VariantB => new List<(BehaviorEvent, string)>
            {
                (new BehaviorEvent("customer", "PricingPage_View", baseTime, new Dictionary<string, object> { ["DurationSeconds"] = 10, ["PlanHovered"] = "Starter" }), "PricingPage (Starter, 10s)"),
                (new BehaviorEvent("customer", "Cart_Add", baseTime.AddSeconds(20), new Dictionary<string, object> { ["ProductId"] = "starter-monthly", ["PricePoint"] = "low" }), "Cart_Add (ucuz paket)"),
                (new BehaviorEvent("customer", "ReviewSection_Scroll", baseTime.AddSeconds(60), new Dictionary<string, object> { ["TimeOnReviews"] = 120 }), "ReviewSection_Scroll (uzun)")
            },
            VariantC => new List<(BehaviorEvent, string)>
            {
                (new BehaviorEvent("customer", "BlogPost_View", baseTime, new Dictionary<string, object> { ["PostId"] = "whitepaper-summary", ["DurationSeconds"] = 180 }), "BlogPost (whitepaper özeti)"),
                (new BehaviorEvent("customer", "Video_Play", baseTime.AddSeconds(200), new Dictionary<string, object> { ["VideoId"] = "edu-101", ["WatchPercent"] = 80, ["Topic"] = "education" }), "Video_Play (eğitim, 80%)"),
                (new BehaviorEvent("customer", "SupportChat_Open", baseTime.AddSeconds(400), new Dictionary<string, object> { ["PreviousTicketsCount"] = 0 }), "SupportChat_Open (soru)")
            },
            VariantD => new List<(BehaviorEvent, string)>
            {
                (new BehaviorEvent("customer", "Cart_Abandon", baseTime, new Dictionary<string, object> { ["StepAbandoned"] = "Payment", ["TimeInCartSeconds"] = 300 }), "Cart_Abandon (Payment adımında)"),
                (new BehaviorEvent("customer", "SupportChat_Open", baseTime.AddMinutes(1), new Dictionary<string, object> { ["PreviousTicketsCount"] = 3, ["WaitTimeSeconds"] = 120 }), "SupportChat_Open (3 önceki ticket)"),
                (new BehaviorEvent("customer", "PricingPage_View", baseTime.AddMinutes(2), new Dictionary<string, object> { ["DurationSeconds"] = 5 }), "PricingPage_View (kısa, tekrar)")
            },
            _ => GetEvents(VariantA, baseTime)
        };
    }
}
