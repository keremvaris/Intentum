using Intentum.Core.Behavior;

namespace Intentum.Sample.Blazor.Api;

/// <summary>
/// Moderation (Context Guardian) demo variants: message sequences and expected intent.
/// </summary>
public static class ModerationVariants
{
    public const string VariantA = "A"; // Trolling
    public const string VariantB = "B"; // Personal attack
    public const string VariantC = "C"; // Seeking help
    public const string VariantD = "D"; // Constructive

    public static string GetExpectedIntent(string variant) => variant switch
    {
        VariantA => "DeliberateProvocation_DerailingTechnicalDiscussion",
        VariantB => "VeeringOffTopicIntoPersonalAttack",
        VariantC => "GenuinelySeekingHelpButFrustrated",
        VariantD => "ConstructiveTechnicalDebate",
        _ => "Unknown"
    };

    public static string GetWarningMessage(string variant) => variant switch
    {
        VariantA => "Tartışmayı teknik zeminde tutmanızı rica ediyoruz. Bir sonraki benzer katkınız otomatik olarak gizlenecektir.",
        VariantB => "Kişisel saldırı yerine konuya odaklanmanızı rica ediyoruz.",
        VariantC => "Yardım ekibiniz sorunuzu incelemek üzere yönlendirildi.",
        _ => ""
    };

    public static string GetLabel(string variant) => variant switch
    {
        VariantA => "Kasıtlı provokasyon (Trolling)",
        VariantB => "Konudan sapma / kişisel saldırı",
        VariantC => "Yardım arayan ama sinirli",
        VariantD => "Yapıcı tartışma",
        _ => "Unknown"
    };

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
                (new BehaviorEvent("User_X", "Post_Create", baseTime, new Dictionary<string, object> { ["ThreadId"] = "crypto-1", ["WordCount"] = 25, ["ToneScore"] = -0.3 }), "Post: 'Bu protokolün güvenlik açığı bariz, kaynakları incelemediniz mi?'"),
                (new BehaviorEvent("User_X", "Reply", baseTime.AddMinutes(5), new Dictionary<string, object> { ["TargetUserId"] = "User_A", ["ToneScore"] = -0.6 }), "Reply to User_A: 'Cevap veremiyorsun çünkü dediğim doğru.'"),
                (new BehaviorEvent("User_X", "Reply", baseTime.AddMinutes(10), new Dictionary<string, object> { ["TargetUserId"] = "User_B", ["ToneScore"] = -0.8 }), "Reply to User_B: 'Senin gibi fanatikler ilerlemeyi engelliyor.'")
            },
            VariantB => new List<(BehaviorEvent, string)>
            {
                (new BehaviorEvent("User_X", "Reply", baseTime, new Dictionary<string, object> { ["TargetUserId"] = "User_A", ["ToneScore"] = -0.4 }), "Reply (ToneScore -0.4)"),
                (new BehaviorEvent("User_X", "Reply", baseTime.AddMinutes(2), new Dictionary<string, object> { ["TargetUserId"] = "User_A", ["ToneScore"] = -0.7 }), "Reply (ToneScore -0.7)"),
                (new BehaviorEvent("User_X", "Reply", baseTime.AddMinutes(5), new Dictionary<string, object> { ["TargetUserId"] = "User_A", ["ToneScore"] = -0.9 }), "Reply (ToneScore -0.9)")
            },
            VariantC => new List<(BehaviorEvent, string)>
            {
                (new BehaviorEvent("User_X", "Post_Create", baseTime, new Dictionary<string, object> { ["WordCount"] = 150, ["ToneScore"] = 0.1, ["QuestionMarksCount"] = 5 }), "Post: Uzun soru (5 soru işareti)"),
                (new BehaviorEvent("User_X", "Reply", baseTime.AddMinutes(10), new Dictionary<string, object> { ["ToneScore"] = -0.4 }), "Reply: Kısa cevap alınca hayal kırıklığı")
            },
            VariantD => new List<(BehaviorEvent, string)>
            {
                (new BehaviorEvent("User_X", "Post_Create", baseTime, new Dictionary<string, object> { ["ContainsCode"] = true, ["ToneScore"] = 0.5 }), "Post: Kod paylaşımı, dengeli ton"),
                (new BehaviorEvent("User_X", "Reply", baseTime.AddMinutes(5), new Dictionary<string, object> { ["ToneScore"] = 0.4 }), "Reply: Yapıcı")
            },
            _ => GetEvents(VariantA, baseTime)
        };
    }
}
