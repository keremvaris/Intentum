using Intentum.Core.Behavior;
using Intentum.Core.Intents;
using Intentum.Runtime.Policy;

namespace Intentum.Sample.Web.Features.GreenwashingDetection;

/// <summary>
/// Niyet ve politika kararına göre önerilen aksiyonları üretir.
/// Mock presets ile tüm dallar tetiklenebilir:
/// - Escalate + ActiveGreenwashing → "Greenwashing" preset
/// - Warn → "Borderline" (StrategicObfuscation / SelectiveDisclosure)
/// - metrics.without.proof → "Greenwashing", "press"
/// - baseline.manipulation → "Borderline" (karşılaştırma bazı)
/// - Score &gt; 0.3 ve başka aksiyon yok → "Sadece belirsiz iddialar" (çeyreklik izleme)
/// - Hiç aksiyon → "Genuine (temiz)"
/// </summary>
public static class SustainabilitySolutionGenerator
{
    public static IReadOnlyList<string> Suggest(Intent intent, BehaviorSpace space, PolicyDecision decision)
    {
        var actions = new List<string>();

        if (decision == PolicyDecision.Escalate && intent.Name == "ActiveGreenwashing")
        {
            actions.Add("ACİL: Çevresel pazarlama iddialarını askıya al");
            actions.Add("ACİL: İç inceleme başlat");
            actions.Add("24 SAAT: Kamuya açıklama hazırla");
        }

        if (decision == PolicyDecision.Warn)
        {
            actions.Add("Üçüncü taraf veri denetimi");
            actions.Add("Metodoloji incelemesi");
            actions.Add("Paydaş danışması");
        }

        if (space.Events.Any(e => e.Action == "metrics.without.proof"))
            actions.Add("Tüm metrik iddialar için destekleyici veri yayımla");

        if (space.Events.Any(e => e.Action == "baseline.manipulation"))
            actions.Add("Sektör standardı baz ile yeniden hesapla");

        if (intent.Confidence.Score > 0.3 && actions.Count == 0)
            actions.Add("Dil ve veri bütünlüğü için gelişmiş çeyreklik izleme");

        if (actions.Count == 0)
            actions.Add("Acil aksiyon yok; standart açıklamaya devam.");

        return actions;
    }
}
