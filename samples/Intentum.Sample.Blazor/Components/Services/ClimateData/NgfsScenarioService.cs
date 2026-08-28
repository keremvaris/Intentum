using System.Globalization;
using System.Text.Json;

namespace Intentum.Sample.Blazor.Components.Services.ClimateData;

/// <summary>
/// NGFS Phase 5 senaryo verilerini okur ve sorgulanabilir hale getirir.
/// Veri: IIASA Scenario Explorer CSV formatı veya seed JSON.
/// </summary>
public sealed class NgfsScenarioService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<NgfsScenarioService> _logger;
    private List<NgfsScenarioData>? _cache;

    public NgfsScenarioService(IWebHostEnvironment env, ILogger<NgfsScenarioService> logger)
    {
        _env = env;
        _logger = logger;
    }

    /// <summary>NGFS verisini yükler (CSV veya JSON seed).</summary>
    public async Task<List<NgfsScenarioData>> LoadAsync(CancellationToken ct = default)
    {
        if (_cache != null) return _cache;

        // Öncelikle CSV'yi dene, yoksa JSON seed kullan.
        var csvPath = Path.Combine(_env.WebRootPath, "data", "ngfs", "ngfs_phase5.csv");
        if (File.Exists(csvPath))
        {
            _cache = await LoadCsvAsync(csvPath, ct);
            return _cache;
        }

        var jsonPath = Path.Combine(_env.WebRootPath, "data", "ngfs", "ngfs_seed.json");
        if (File.Exists(jsonPath))
        {
            var json = await File.ReadAllTextAsync(jsonPath, ct);
            _cache = JsonSerializer.Deserialize<List<NgfsScenarioData>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? [];
            return _cache;
        }

        _logger.LogWarning("NGFS data not found at {CsvPath} or {JsonPath}", csvPath, jsonPath);
        return [];
    }

    /// <summary>Bölge + senaryo için değişken değerini getir.</summary>
    public async Task<double?> GetValueAsync(string region, string scenario, string variable, int year, CancellationToken ct = default)
    {
        var data = await LoadAsync(ct);
        return data
            .FirstOrDefault(d =>
                d.Region.Equals(region, StringComparison.OrdinalIgnoreCase) &&
                d.Scenario.Equals(scenario, StringComparison.OrdinalIgnoreCase) &&
                d.Variable.Equals(variable, StringComparison.OrdinalIgnoreCase))
            ?.YearlyValues.GetValueOrDefault(year);
    }

    /// <summary>Bölge + senaryo için tüm değişkenlerin özetini getir (belirli yıl).</summary>
    public async Task<NgfsMacroSnapshot?> GetSnapshotAsync(string region, string scenario, int year, CancellationToken ct = default)
    {
        var data = await LoadAsync(ct);
        var filtered = data.Where(d =>
            d.Region.Equals(region, StringComparison.OrdinalIgnoreCase) &&
            d.Scenario.Equals(scenario, StringComparison.OrdinalIgnoreCase)).ToList();

        if (filtered.Count == 0) return null;

        return new NgfsMacroSnapshot
        {
            Region = region,
            Scenario = scenario,
            Year = year,
            GdpChange = filtered.FirstOrDefault(d => d.Variable.Contains("GDP", StringComparison.OrdinalIgnoreCase))?.YearlyValues.GetValueOrDefault(year),
            CarbonPrice = filtered.FirstOrDefault(d => d.Variable.Contains("Carbon Price", StringComparison.OrdinalIgnoreCase))?.YearlyValues.GetValueOrDefault(year),
            TemperatureChange = filtered.FirstOrDefault(d => d.Variable.Contains("Temperature", StringComparison.OrdinalIgnoreCase))?.YearlyValues.GetValueOrDefault(year),
            EnergyInvestment = filtered.FirstOrDefault(d => d.Variable.Contains("Investment", StringComparison.OrdinalIgnoreCase))?.YearlyValues.GetValueOrDefault(year),
            EmploymentChange = filtered.FirstOrDefault(d => d.Variable.Contains("Employment", StringComparison.OrdinalIgnoreCase))?.YearlyValues.GetValueOrDefault(year)
        };
    }

    /// <summary>Bir bölge için tüm senaryoların özetlerini getir (karşılaştırma).</summary>
    public async Task<List<NgfsMacroSnapshot>> GetComparisonAsync(string region, int year, CancellationToken ct = default)
    {
        var data = await LoadAsync(ct);
        var scenarios = data.Where(d =>
            d.Region.Equals(region, StringComparison.OrdinalIgnoreCase))
            .Select(d => d.Scenario).Distinct().ToList();

        var results = new List<NgfsMacroSnapshot>();
        foreach (var scenario in scenarios)
        {
            var snapshot = await GetSnapshotAsync(region, scenario, year, ct);
            if (snapshot != null) results.Add(snapshot);
        }
        return results;
    }

    /// <summary>Bölge için kullanılabilir senaryo listesini getir.</summary>
    public async Task<List<string>> GetAvailableScenariosAsync(string region, CancellationToken ct = default)
    {
        var data = await LoadAsync(ct);
        return data.Where(d => d.Region.Equals(region, StringComparison.OrdinalIgnoreCase))
            .Select(d => d.Scenario).Distinct().OrderBy(s => s).ToList();
    }

    /// <summary>CSV formatında NGFS verisi oku (IAMC formatı).</summary>
    private static async Task<List<NgfsScenarioData>> LoadCsvAsync(string path, CancellationToken ct)
    {
        var lines = await File.ReadAllLinesAsync(path, ct);
        if (lines.Length < 2) return [];

        // IAMC formatı: model,scenario,region,variable,unit,2020,2025,2030,...
        var header = lines[0].Split(',');
        var yearColumns = new Dictionary<int, int>();

        // Yıl sütunlarını bul (ilk 5 sütun değilse)
        for (int i = 5; i < header.Length; i++)
        {
            if (int.TryParse(header[i].Trim(), out var year))
                yearColumns[year] = i;
        }

        var result = new List<NgfsScenarioData>();

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var cols = lines[i].Split(',');

            if (cols.Length < 6) continue;

            var entry = new NgfsScenarioData
            {
                Model = cols[0].Trim(),
                Scenario = cols[1].Trim(),
                Region = cols[2].Trim(),
                Variable = cols[3].Trim(),
                Unit = cols[4].Trim()
            };

            foreach (var (year, colIdx) in yearColumns)
            {
                if (colIdx < cols.Length &&
                    double.TryParse(cols[colIdx].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var val))
                {
                    entry.YearlyValues[year] = val;
                }
            }

            result.Add(entry);
        }

        return result;
    }
}
