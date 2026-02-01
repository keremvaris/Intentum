using System.Text.RegularExpressions;
using Intentum.Core;
using Intentum.Core.Behavior;

namespace Intentum.Sample.Web.Features.GreenwashingDetection;

/// <summary>
/// Rapor metninden davranış sinyalleri üretir (belirsiz iddialar, kanıtsız metrikler, karşılaştırmalar).
/// Dil parametresi ile TR, EN, DE pattern setleri kullanılabilir; null/boş = tüm diller.
/// </summary>
public static class SustainabilityReporter
{
    private static readonly string[] VaguePatternsEn =
    [
        "sustainable future", "green transition", "eco-friendly", "clean production",
        "ecological balance", "carbon neutrality", "respect for nature", "net zero"
    ];

    private static readonly string[] VaguePatternsTr =
    [
        "sürdürülebilir", "sürdürülebilir gelecek", "yeşil dönüşüm", "eko", "temiz üretim", "ekolojik denge",
        "karbon nötr", "doğaya saygı", "çevre dostu", "yeşil"
    ];

    private static readonly string[] VaguePatternsDe =
    [
        "nachhaltige Zukunft", "grüner Wandel", "umweltfreundlich", "saubere Produktion",
        "ökologisches Gleichgewicht", "Klimaneutralität", "Respekt vor der Natur", "Netto-Null",
        "grün", "nachhaltig"
    ];

    private static readonly Regex MetricsPattern = new(
        @"%\s*(reduction|increase|improvement|azalım|artış|iyileştirme)|(%\d+)|(\d+\s*(ton|kg|kWh|CO2|CO₂|emisyon|su))",
        RegexOptions.IgnoreCase);

    private static readonly Regex UnsubstantiatedComparisonPattern = new(
        @"(more|less|better|greener)\s+(than|ever)|(daha\s+(az|çok|iyi|yeşil))\s+(su|enerji|ever)?",
        RegexOptions.IgnoreCase);

    private static readonly string[] BaselineManipulationPatterns =
    [
        "base year", "baz yılı", "baz yıl", "comparison base", "karşılaştırma bazı",
        "seçilmiş baz", "favourable baseline", "favorable baseline", "Referenzjahr", "Basisjahr"
    ];

    /// <param name="report"></param>
    /// <param name="language">"tr", "en", "de" veya null (tüm diller).</param>
    public static BehaviorSpace AnalyzeReport(string? report, string? language = null)
    {
        var space = new BehaviorSpace();
        if (string.IsNullOrWhiteSpace(report))
            return space;

        var vagueSets = string.IsNullOrWhiteSpace(language)
            ? [VaguePatternsEn, VaguePatternsTr, VaguePatternsDe]
            : language.ToLowerInvariant() switch
            {
                "tr" => new[] { VaguePatternsTr },
                "en" => new[] { VaguePatternsEn },
                "de" => new[] { VaguePatternsDe },
                _ => new[] { VaguePatternsEn, VaguePatternsTr }
            };

        foreach (var patterns in vagueSets)
        {
            foreach (var pattern in patterns)
            {
                var count = Regex.Count(report, Regex.Escape(pattern), RegexOptions.IgnoreCase);
                for (var i = 0; i < Math.Min(count, 10); i++)
                    space.Observe("language", "claim.vague");
            }
        }

        var hasMetrics = MetricsPattern.IsMatch(report);
        var hasProof = report.Contains("ISO", StringComparison.OrdinalIgnoreCase)
                      || report.Contains("verified", StringComparison.OrdinalIgnoreCase)
                      || report.Contains("doğrulanmış", StringComparison.OrdinalIgnoreCase)
                      || report.Contains("audit", StringComparison.OrdinalIgnoreCase)
                      || report.Contains("denetim", StringComparison.OrdinalIgnoreCase)
                      || report.Contains("third-party", StringComparison.OrdinalIgnoreCase)
                      || report.Contains("üçüncü taraf", StringComparison.OrdinalIgnoreCase);
        if (hasMetrics && !hasProof)
            space.Observe("data", "metrics.without.proof");

        if (UnsubstantiatedComparisonPattern.IsMatch(report))
            space.Observe("language", "comparison.unsubstantiated");

        foreach (var pattern in BaselineManipulationPatterns)
        {
            if (report.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                space.Observe("data", "baseline.manipulation");
                break;
            }
        }

        return space;
    }
}
