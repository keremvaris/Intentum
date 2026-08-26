namespace Intentum.Sample.Blazor.Components.Services.ClimateData;

public sealed class GadmGeoJsonService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<GadmGeoJsonService> _logger;
    private readonly Dictionary<string, string> _cache = new();

    public GadmGeoJsonService(IWebHostEnvironment env, ILogger<GadmGeoJsonService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<string?> GetGeoJsonAsync(string iso3, int level = 0, CancellationToken ct = default)
    {
        var key = $"{iso3}_{level}";
        if (_cache.TryGetValue(key, out var cached)) return cached;

        var fileName = $"gadm41_{iso3}_{level}.json";
        var path = Path.Combine(_env.WebRootPath, "data", "gadm", fileName);

        if (!File.Exists(path))
        {
            _logger.LogWarning("GADM GeoJSON not found: {Path}", path);
            return null;
        }

        var json = await File.ReadAllTextAsync(path, ct);
        _cache[key] = json;
        return json;
    }

    public async Task<List<string>> GetAvailableCountriesAsync(CancellationToken ct = default)
    {
        var gadmDir = Path.Combine(_env.WebRootPath, "data", "gadm");
        if (!Directory.Exists(gadmDir)) return [];

        return Directory.GetFiles(gadmDir, "gadm41_*_0.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(x => x != null)
            .Select(x => x!.Replace("gadm41_", "").Replace("_0", ""))
            .OrderBy(x => x)
            .ToList();
    }

    public async Task<List<string>> GetAvailableLevelsAsync(string iso3, CancellationToken ct = default)
    {
        var gadmDir = Path.Combine(_env.WebRootPath, "data", "gadm");
        if (!Directory.Exists(gadmDir)) return [];

        return Directory.GetFiles(gadmDir, $"gadm41_{iso3}_*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(x => x != null)
            .Select(x => x!.Replace($"gadm41_{iso3}_", ""))
            .OrderBy(x => x)
            .ToList();
    }
}
