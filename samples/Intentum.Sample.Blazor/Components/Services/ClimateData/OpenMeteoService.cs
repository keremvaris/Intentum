using System.Net.Http.Json;
using System.Text.Json;

namespace Intentum.Sample.Blazor.Components.Services.ClimateData;

public sealed class OpenMeteoService
{
    private readonly HttpClient _http;
    private readonly ILogger<OpenMeteoService> _logger;

    public OpenMeteoService(HttpClient http, ILogger<OpenMeteoService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<ClimateProjection?> GetProjectionAsync(
        double latitude, double longitude,
        string model = "EC_Earth3P_HR",
        string startDate = "2030-01-01",
        string endDate = "2030-12-31",
        CancellationToken ct = default)
    {
        try
        {
            var url = $"https://climate-api.open-meteo.com/v1/climate" +
                      $"?latitude={latitude}" +
                      $"&longitude={longitude}" +
                      $"&start_date={startDate}" +
                      $"&end_date={endDate}" +
                      $"&models={model}" +
                      $"&daily=temperature_2m_max,temperature_2m_min,precipitation_sum,wind_speed_10m_max,relative_humidity_2m_mean";

            var response = await _http.GetFromJsonAsync<JsonElement>(url, ct);

            var daily = response.GetProperty("daily");
            return new ClimateProjection
            {
                Time = daily.GetProperty("time").EnumerateArray().Select(x => x.GetString() ?? "").ToArray(),
                TempMax = daily.GetProperty("temperature_2m_max").EnumerateArray().Select(x => x.GetDouble()).ToArray(),
                TempMin = daily.GetProperty("temperature_2m_min").EnumerateArray().Select(x => x.GetDouble()).ToArray(),
                Precipitation = daily.GetProperty("precipitation_sum").EnumerateArray().Select(x => x.GetDouble()).ToArray(),
                WindMax = daily.GetProperty("wind_speed_10m_max").EnumerateArray().Select(x => x.GetDouble()).ToArray(),
                Humidity = daily.GetProperty("relative_humidity_2m_mean").EnumerateArray().Select(x => x.GetDouble()).ToArray()
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch Open-Meteo projection for {Lat},{Lng}", latitude, longitude);
            return null;
        }
    }

    public async Task<CurrentWeather?> GetCurrentAsync(
        double latitude, double longitude, CancellationToken ct = default)
    {
        try
        {
            var url = $"https://api.open-meteo.com/v1/forecast" +
                      $"?latitude={latitude}&longitude={longitude}" +
                      $"&current=temperature_2m,relative_humidity_2m,precipitation,wind_speed_10m";

            return await _http.GetFromJsonAsync<CurrentWeather>(url, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch current weather for {Lat},{Lng}", latitude, longitude);
            return null;
        }
    }
}

public sealed class ClimateProjection
{
    public string[] Time { get; set; } = [];
    public double[] TempMax { get; set; } = [];
    public double[] TempMin { get; set; } = [];
    public double[] Precipitation { get; set; } = [];
    public double[] WindMax { get; set; } = [];
    public double[] Humidity { get; set; } = [];

    public double AvgTempMax => TempMax.Length > 0 ? TempMax.Average() : 0;
    public double AvgPrecipitation => Precipitation.Length > 0 ? Precipitation.Average() : 0;
    public double TotalPrecipitation => Precipitation.Sum();
}

public sealed class CurrentWeather
{
    public CurrentData? current { get; set; }
}

public sealed class CurrentData
{
    public double temperature_2m { get; set; }
    public double relative_humidity_2m { get; set; }
    public double precipitation { get; set; }
    public double wind_speed_10m { get; set; }
}
