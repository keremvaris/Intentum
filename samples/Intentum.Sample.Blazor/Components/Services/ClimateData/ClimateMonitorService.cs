using System.Text.Json;

namespace Intentum.Sample.Blazor.Components.Services.ClimateData;

public sealed class ClimateMonitorService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ClimateMonitorService> _logger;
    private ClimateBaselineTrends? _cache;

    public ClimateMonitorService(IWebHostEnvironment env, ILogger<ClimateMonitorService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<ClimateBaselineTrends> GetBaselineTrendsAsync(CancellationToken ct = default)
    {
        if (_cache != null) return _cache;

        var path = Path.Combine(_env.WebRootPath, "data", "climate", "baseline_trends.json");
        if (!File.Exists(path))
        {
            _logger.LogWarning("Climate baseline trends not found at {Path}", path);
            return new ClimateBaselineTrends();
        }

        var json = await File.ReadAllTextAsync(path, ct);
        _cache = JsonSerializer.Deserialize<ClimateBaselineTrends>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new ClimateBaselineTrends();

        return _cache;
    }
}

public sealed class ClimateBaselineTrends
{
    public TrendData? co2 { get; set; }
    public TrendData? sea_level { get; set; }
    public TrendData? temperature_anomaly { get; set; }
    public IceMassData? ice_mass { get; set; }
}

public sealed class TrendData
{
    public string? unit { get; set; }
    public string? source { get; set; }
    public double current_value { get; set; }
    public string? trend { get; set; }
    public double? current_rate_mm_per_year { get; set; }
    public double? total_rise_since_1993_mm { get; set; }
    public double? annual_change_ppm { get; set; }
    public List<TrendPoint>? series { get; set; }
}

public sealed class TrendPoint
{
    public int year { get; set; }
    public double value { get; set; }
}

public sealed class IceMassData
{
    public IceMassBody? greenland { get; set; }
    public IceMassBody? antarctica { get; set; }
}

public sealed class IceMassBody
{
    public string? unit { get; set; }
    public double loss_rate { get; set; }
    public string? trend { get; set; }
}
