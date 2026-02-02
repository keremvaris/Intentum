using Intentum.Core.Behavior;

namespace Intentum.Sample.Blazor.Api;

/// <summary>
/// Adaptive Tutor (EdTech) demo variants: learning events and expected intent.
/// </summary>
public static class AdaptiveTutorVariants
{
    public const string VariantA = "A"; // Conceptual block
    public const string VariantB = "B"; // Surface level
    public const string VariantC = "C"; // Losing motivation
    public const string VariantD = "D"; // Ready

    public static string GetExpectedIntent(string variant) => variant switch
    {
        VariantA => "ConceptualBlock_NeedsAlternativeExplanationAndPractice",
        VariantB => "SurfaceLevelUnderstanding_SeekingQuickAnswer",
        VariantC => "LosingMotivationDueToPace",
        VariantD => "ReadyForNextModule",
        _ => "Unknown"
    };

    public static string GetLabel(string variant) => variant switch
    {
        VariantA => "Kavramsal blok",
        VariantB => "Yüzeysel / hızlı cevap",
        VariantC => "Motivasyon düşüşü",
        VariantD => "Hazır",
        _ => "Unknown"
    };

    public static int GetStepCount(string variant) => variant switch
    {
        VariantA => 3,
        VariantB => 3,
        VariantC => 3,
        VariantD => 2,
        _ => 3
    };

    public static BehaviorSpace BuildSpace(string variant, DateTimeOffset baseTime)
    {
        var space = new BehaviorSpace();
        space.SetMetadata("StudentId", "Ali");
        space.SetMetadata("ModuleId", "Python_Loops_3");
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
                (new BehaviorEvent("Ali", "Video_Play", baseTime, new Dictionary<string, object> { ["ModuleId"] = "Python_Loops_3", ["LoopCount"] = 2, ["WatchPercent"] = 100 }), "Video_Play (LoopCount=2)"),
                (new BehaviorEvent("Ali", "Quiz_Attempt", baseTime.AddMinutes(5), new Dictionary<string, object> { ["ModuleId"] = "Python_Loops_3", ["OutcomeScore"] = 0.4, ["StrugglePattern"] = "Logic_Flaw" }), "Quiz_Attempt (0.4, Logic_Flaw)"),
                (new BehaviorEvent("Ali", "Forum_Post", baseTime.AddMinutes(10), new Dictionary<string, object> { ["ModuleId"] = "Python_Loops_3", ["Topic"] = "For döngüsündeki indeks neden 1'den başlamıyor?" }), "Forum_Post (kavramsal soru)")
            },
            VariantB => new List<(BehaviorEvent, string)>
            {
                (new BehaviorEvent("Ali", "Video_Play", baseTime, new Dictionary<string, object> { ["ModuleId"] = "Python_Loops_3", ["WatchPercent"] = 30 }), "Video_Play (WatchPercent=30)"),
                (new BehaviorEvent("Ali", "Quiz_Attempt", baseTime.AddMinutes(2), new Dictionary<string, object> { ["ModuleId"] = "Python_Loops_3", ["OutcomeScore"] = 0.3, ["StrugglePattern"] = "Timeout" }), "Quiz_Attempt (Timeout, düşük puan)"),
                (new BehaviorEvent("Ali", "Resource_Open", baseTime.AddMinutes(4), new Dictionary<string, object> { ["DurationSeconds"] = 15, ["ResourceType"] = "Doc" }), "Resource_Open (kısa süre)")
            },
            VariantC => new List<(BehaviorEvent, string)>
            {
                (new BehaviorEvent("Ali", "Session_Idle", baseTime, new Dictionary<string, object> { ["IdleMinutes"] = 20, ["LastActivityType"] = "Quiz_Attempt" }), "Session_Idle (20 dk)"),
                (new BehaviorEvent("Ali", "Video_Play", baseTime.AddMinutes(25), new Dictionary<string, object> { ["WatchPercent"] = 15 }), "Video_Play (WatchPercent=15)"),
                (new BehaviorEvent("Ali", "Quiz_Attempt", baseTime.AddMinutes(30), new Dictionary<string, object> { ["OutcomeScore"] = 0.2, ["StrugglePattern"] = "PartialAnswer" }), "Quiz_Attempt (atlanan sorular)")
            },
            VariantD => new List<(BehaviorEvent, string)>
            {
                (new BehaviorEvent("Ali", "Module_Complete", baseTime, new Dictionary<string, object> { ["ModuleId"] = "Python_Loops_2", ["QuizScore"] = 0.95 }), "Module_Complete (yüksek skor)"),
                (new BehaviorEvent("Ali", "Quiz_Attempt", baseTime.AddMinutes(2), new Dictionary<string, object> { ["ModuleId"] = "Python_Loops_3", ["OutcomeScore"] = 0.9 }), "Quiz_Attempt (tek denemede geçti)")
            },
            _ => GetEvents(VariantA, baseTime)
        };
    }
}
