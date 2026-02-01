using System.Collections.Concurrent;
using Intentum.Sample.Blazor.Api;

namespace Intentum.Sample.Blazor.Features.GreenwashingDetection;

/// <summary>
/// Son greenwashing analizlerini tutar (in-memory, demo). Periyodik mock kayıt eklenebilir.
/// </summary>
public static class GreenwashingRecentStore
{
    private const int MaxItems = 50;
    private static readonly ConcurrentQueue<GreenwashingRecentItem> Items = new();
    private static readonly Random Rnd = new();

    public static void Add(GreenwashingRecentItem item)
    {
        Items.Enqueue(item);
        while (Items.Count > MaxItems && Items.TryDequeue(out _)) { /* drain until at or below MaxItems */ }
    }

    public static IReadOnlyList<GreenwashingRecentItem> GetRecent(int limit = 10)
    {
        var list = Items.ToArray();
        return list
            .OrderByDescending(x => x.AnalyzedAt)
            .Take(Math.Clamp(limit, 1, 50))
            .ToList();
    }

    /// <summary>
    /// Demo: rastgele bir mock analiz ekler (gerçek zamanlı akış hissi).
    /// </summary>
    public static void AddMockEntry()
    {
        var sources = new[] { "Report", "SocialMedia", "PressRelease", "InvestorPresentation" };
        var intents = new[] { "GenuineSustainability", "SelectiveDisclosure", "StrategicObfuscation", "ActiveGreenwashing" };
        var decisions = new[] { "Allow", "Observe", "Warn", "Escalate" };
        var langs = new[] { "TR", "EN", "DE" };
        var previews = new[]
        {
            "Sürdürülebilirlik raporu özeti…",
            "EcoCorp green transition…",
            "Nachhaltige Zukunft…"
        };
        var idx = Rnd.Next(0, sources.Length);
        var item = new GreenwashingRecentItem(
            Id: "0x" + Guid.NewGuid().ToString("N")[..16],
            ReportPreview: previews[Rnd.Next(0, previews.Length)],
            IntentName: intents[Rnd.Next(0, intents.Length)],
            Decision: decisions[Rnd.Next(0, decisions.Length)],
            SourceType: sources[idx],
            Language: langs[Rnd.Next(0, langs.Length)],
            AnalyzedAt: DateTimeOffset.UtcNow);
        Add(item);
    }
}
