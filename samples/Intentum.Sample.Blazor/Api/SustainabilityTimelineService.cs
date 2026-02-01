using System.Security.Cryptography;

namespace Intentum.Sample.Blazor.Api;

/// <summary>
/// Background service: advances simulated date, produces sustainability intent/scores from company profile, broadcasts via SSE.
/// </summary>
public sealed class SustainabilityTimelineService(
    SustainabilitySimulationState state,
    SustainabilityTimelineBroadcaster broadcaster,
    ILogger<SustainabilityTimelineService> logger)
    : BackgroundService
{
    private static readonly string[] Intents =
    {
        "GenuineSustainability",
        "UnintentionalMisrepresentation",
        "SelectiveDisclosure",
        "StrategicObfuscation",
        "ActiveGreenwashing"
    };

    /// <summary>ClientEarth company id -> display name (9 fossil fuel profiles).</summary>
    private static readonly IReadOnlyDictionary<string, string> CompanyNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["shell"] = "Shell",
        ["total"] = "Total",
        ["aramco"] = "Aramco",
        ["chevron"] = "Chevron",
        ["drax"] = "Drax",
        ["equinor"] = "Equinor",
        ["exxonmobil"] = "ExxonMobil",
        ["ineos"] = "INEOS",
        ["rwe"] = "RWE"
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (!state.Running)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                continue;
            }

            try
            {
                var simulatedAt = state.Advance();
                var companyId = state.CompanyId;
                var companyName = CompanyNames.GetValueOrDefault(companyId, companyId);
                var granularity = state.Granularity;

                // Year-based drift: 2020 more Genuine, later years more ActiveGreenwashing for fossil companies
                var yearIndex = Math.Max(0, simulatedAt.Year - 2020);
                var greenwashBias = Math.Min(0.7, yearIndex * 0.08);
                var genuineWeight = Math.Max(0.1, 0.6 - greenwashBias);
                var activeWeight = 0.1 + greenwashBias;
                var otherWeight = (1.0 - genuineWeight - activeWeight) / 3;

                var intentIndex = WeightedRandom(genuineWeight, otherWeight, otherWeight, otherWeight);
                var intentName = Intents[intentIndex];
                var score = 0.4 + SecureRandomDouble() * 0.5;

                var ev = new SustainabilityTimelineEvent(simulatedAt, companyId, companyName, intentName, score, granularity);
                broadcaster.Broadcast(ev);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Sustainability timeline step failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(1.5), stoppingToken);
        }
    }

    private static double SecureRandomDouble()
    {
        return RandomNumberGenerator.GetInt32(0, int.MaxValue) / (double)int.MaxValue;
    }

    private static int WeightedRandom(double w0, double w1, double w2, double w3)
    {
        var t = SecureRandomDouble();
        if (t < w0) return 0;
        t -= w0;
        if (t < w1) return 1;
        t -= w1;
        if (t < w2) return 2;
        t -= w2;
        if (t < w3) return 3;
        return 4;
    }
}
