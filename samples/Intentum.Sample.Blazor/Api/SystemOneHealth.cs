using Intentum.AI.SystemOne;

namespace Intentum.Sample.Blazor.Api;

/// <summary>Health of each known System One engine: probed with a short-timeout GET, cached 30s.</summary>
internal sealed record SystemOneHealthItem(string Engine, string Status, string Hint);

internal static class SystemOneHealth
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan ProbeTimeout = TimeSpan.FromSeconds(2);
    private static readonly HttpClient PingClient = new() { Timeout = ProbeTimeout };
    private static readonly object Gate = new();
    private static readonly Dictionary<string, (DateTimeOffset At, SystemOneHealthItem Item)> Cache = new();
    private static readonly string[] PingEngines = ["jev", "kev", "laya", "tinyjev"];

    internal static async Task<IReadOnlyList<SystemOneHealthItem>> GetAsync()
    {
        var now = DateTimeOffset.UtcNow;
        var engines = SystemOneEngines.Names;
        var items = new SystemOneHealthItem?[engines.Count];
        var stale = new List<int>();

        lock (Gate)
        {
            for (var i = 0; i < engines.Count; i++)
            {
                if (Cache.TryGetValue(engines[i], out var entry) && now - entry.At < CacheTtl)
                    items[i] = entry.Item;
                else
                    stale.Add(i);
            }
        }

        var probed = await Task.WhenAll(stale.Select(async i =>
        {
            var engine = engines[i];
            var baseUrl = IsPingEngine(engine)
                // non-pinged engines must get null → "unknown", do not probe their preset URLs
                ? SystemOneEngines.FromName(engine).BaseUrl
                : null;
            var item = await ProbeAsync(engine, baseUrl).ConfigureAwait(false);
            lock (Gate)
                Cache[engine] = (DateTimeOffset.UtcNow, item);
            return (i, item);
        })).ConfigureAwait(false);

        foreach (var (i, item) in probed)
            items[i] = item;

        return items!;
    }

    /// <summary>Single engine probe. Any HTTP response (even 404) counts as up; network failure is down.</summary>
    internal static async Task<SystemOneHealthItem> ProbeAsync(string engine, string? baseUrl)
    {
        if (string.Equals(engine, "demo", StringComparison.OrdinalIgnoreCase))
            return new SystemOneHealthItem(engine, "up", "Uygulama içi demo motor — her zaman hazır, key gerekmez.");

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return IsPingEngine(engine)
                ? new SystemOneHealthItem(engine, "unknown", "Base URL tanımlı değil.")
                : new SystemOneHealthItem(
                    engine, "unknown",
                    "Kurulum doğrulanmadı — örnek komutlar: examples/system-one-decision/README.md");
        }

        try
        {
            using var cts = new CancellationTokenSource(ProbeTimeout);
            await PingClient.GetAsync($"{baseUrl.TrimEnd('/')}/health", cts.Token).ConfigureAwait(false);
            return new SystemOneHealthItem(engine, "up", HintFor(engine));
        }
        catch (Exception)
        {
            // Any failure (refused, DNS, timeout) means the engine is not usable right now.
            return new SystemOneHealthItem(engine, "down", HintFor(engine));
        }
    }

    private static bool IsPingEngine(string engine) =>
        PingEngines.Contains(engine, StringComparer.OrdinalIgnoreCase);

    private static string HintFor(string engine) => engine.ToLowerInvariant() switch
    {
        "jev" => "API Key gerekli: console.typesafe.ai/keys (SYSTEMONE_API_KEY veya API Key alanı).",
        "kev" => "Başlat: uv run --extra serve python -m kev.serve --run jaredpalmer/kev-4b --port 8009",
        "laya" => "Başlat: pip install \"laya[serve]\" && laya-serve",
        "tinyjev" => "Başlat: tinyjev serve",
        _ => "Örnek komutlar: examples/system-one-decision/README.md",
    };
}
