namespace Intentum.Sample.Blazor.Components.Services.ClimateData;

using System.Globalization;

public sealed class WriAqueductService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<WriAqueductService> _logger;
    private List<WriCountryBaseline>? _baselineCache;
    private List<WriCountryFuture>? _futureCache;

    public WriAqueductService(IWebHostEnvironment env, ILogger<WriAqueductService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<List<WriCountryBaseline>> GetBaselineAsync(CancellationToken ct = default)
    {
        if (_baselineCache != null) return _baselineCache;

        var path = Path.Combine(_env.WebRootPath, "data", "wri", "country_baseline.csv");
        if (!File.Exists(path))
        {
            _logger.LogWarning("WRI baseline CSV not found at {Path}", path);
            return [];
        }

        var lines = await File.ReadAllLinesAsync(path, ct);
        _baselineCache = lines.Skip(1)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(WriCountryBaseline.Parse)
            .ToList();
        return _baselineCache;
    }

    public async Task<List<WriCountryFuture>> GetFutureAsync(CancellationToken ct = default)
    {
        if (_futureCache != null) return _futureCache;

        var path = Path.Combine(_env.WebRootPath, "data", "wri", "country_future.csv");
        if (!File.Exists(path)) return [];

        var lines = await File.ReadAllLinesAsync(path, ct);
        _futureCache = lines.Skip(1)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(WriCountryFuture.Parse)
            .ToList();
        return _futureCache;
    }

    public async Task<WriCountryRisk?> GetCountryRiskAsync(string iso3, CancellationToken ct = default)
    {
        var baseline = await GetBaselineAsync(ct);
        var country = baseline.FirstOrDefault(x =>
            string.Equals(x.gid_0, iso3, StringComparison.OrdinalIgnoreCase) &&
            x.indicator_name == "bws" && x.weight == "Tot");

        if (country == null) return null;

        return new WriCountryRisk
        {
            Iso3 = country.gid_0,
            Name = country.name_0,
            WaterStress = country.score,
            WaterDepletion = 0,
            InterannualVariability = 0,
            SeasonalVariability = 0,
            DroughtRisk = 0,
            FloodRisk = 0,
            GroundwaterStress = 0,
            WaterStressLabel = country.label ?? ""
        };
    }

    public async Task<List<WriCountryRisk>> GetAllCountryRisksAsync(CancellationToken ct = default)
    {
        var baseline = await GetBaselineAsync(ct);
        var totals = baseline
            .Where(x => x.weight == "Tot" && x.indicator_name == "bws")
            .ToList();

        return totals.Select(x => new WriCountryRisk
        {
            Iso3 = x.gid_0,
            Name = x.name_0,
            WaterStress = x.score,
            WaterStressLabel = x.label ?? ""
        }).ToList();
    }
}

public sealed class WriCountryBaseline
{
    public string gid_0 { get; set; } = "";
    public string name_0 { get; set; } = "";
    public string indicator_name { get; set; } = "";
    public string weight { get; set; } = "";
    public double score { get; set; }
    public double score_ranked { get; set; }
    public int cat { get; set; }
    public string? label { get; set; }
    public string? un_region { get; set; }
    public string? wb_region { get; set; }

    public static WriCountryBaseline Parse(string line)
    {
        var parts = SplitCsvLine(line);
        return new WriCountryBaseline
        {
            gid_0 = Get(parts, 0),
            name_0 = Get(parts, 1),
            indicator_name = Get(parts, 2),
            weight = Get(parts, 3),
            score = double.TryParse(Get(parts, 4), NumberStyles.Float, CultureInfo.InvariantCulture, out var s) ? s : 0,
            score_ranked = double.TryParse(Get(parts, 5), NumberStyles.Float, CultureInfo.InvariantCulture, out var sr) ? sr : 0,
            cat = int.TryParse(Get(parts, 6), NumberStyles.Integer, CultureInfo.InvariantCulture, out var c) ? c : 0,
            label = GetOrNull(parts, 7),
            un_region = GetOrNull(parts, 8),
            wb_region = GetOrNull(parts, 9)
        };
    }

    internal static string[] SplitCsvLine(string line)
    {
        var result = new List<string>();
        var current = new System.Text.StringBuilder();
        bool inQuotes = false;
        foreach (var ch in line)
        {
            if (ch == '"') { inQuotes = !inQuotes; continue; }
            if (ch == ',' && !inQuotes) { result.Add(current.ToString()); current.Clear(); continue; }
            current.Append(ch);
        }
        result.Add(current.ToString());
        return result.ToArray();
    }

    private static string Get(string[] parts, int i) => i < parts.Length ? parts[i].Trim().Trim('"') : "";
    private static string? GetOrNull(string[] parts, int i) => i < parts.Length ? parts[i].Trim().Trim('"') : null;
}

public sealed class WriCountryFuture
{
    public string gid_0 { get; set; } = "";
    public string name_0 { get; set; } = "";
    public int year { get; set; }
    public string scenario { get; set; } = "";
    public string indicator_name { get; set; } = "";
    public string weight { get; set; } = "";
    public double score { get; set; }
    public double score_ranked { get; set; }
    public int cat { get; set; }
    public string? label { get; set; }

    public static WriCountryFuture Parse(string line)
    {
        var parts = WriCountryBaseline.SplitCsvLine(line);
        return new WriCountryFuture
        {
            gid_0 = Get(parts, 0),
            name_0 = Get(parts, 1),
            year = int.TryParse(Get(parts, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, out var y) ? y : 0,
            scenario = Get(parts, 3),
            indicator_name = Get(parts, 4),
            weight = Get(parts, 5),
            score = double.TryParse(Get(parts, 6), NumberStyles.Float, CultureInfo.InvariantCulture, out var s) ? s : 0,
            score_ranked = double.TryParse(Get(parts, 7), NumberStyles.Float, CultureInfo.InvariantCulture, out var sr) ? sr : 0,
            cat = int.TryParse(Get(parts, 8), NumberStyles.Integer, CultureInfo.InvariantCulture, out var c) ? c : 0,
            label = GetOrNull(parts, 9)
        };
    }

    private static string Get(string[] parts, int i) => i < parts.Length ? parts[i].Trim().Trim('"') : "";
    private static string? GetOrNull(string[] parts, int i) => i < parts.Length ? parts[i].Trim().Trim('"') : null;
}

public sealed class WriCountryRisk
{
    public string Iso3 { get; set; } = "";
    public string Name { get; set; } = "";
    public double WaterStress { get; set; }
    public double WaterDepletion { get; set; }
    public double InterannualVariability { get; set; }
    public double SeasonalVariability { get; set; }
    public double DroughtRisk { get; set; }
    public double FloodRisk { get; set; }
    public double GroundwaterStress { get; set; }
    public string WaterStressLabel { get; set; } = "";
}
