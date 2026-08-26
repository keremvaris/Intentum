using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Intentum.Sample.Blazor.Components.Services;
using Intentum.Sample.Blazor.Components.Services.ClimateData;

namespace Intentum.Sample.Blazor.Components.Pages.Examples;

public sealed partial class ClimateRiskDashboard : IAsyncDisposable
{
    private RiskInput _input = new();
    private RiskAssessment? _assessment;
    private ClimateBaselineTrends? _trends;
    private bool _running;
    private int _mapLevel;
    private string _currentCountryName = "";
    private string _currentIso3 = "";

    private int _tempSlider = 48;
    private int _precipSlider = -15;
    private int _seaSlider = 35;
    private int _carbonSlider = 85;
    private double _tempAnomaly = 2.4;
    private double _precipChange = -15;
    private double _seaLevelRise = 0.5;
    private int _carbonPrice = 85;

    private EChartsInterop? _pieEcharts;
    private EChartsInterop? _barEcharts;
    private EChartsInterop? _lineEcharts;
    private EChartsInterop? _gaugeEcharts;

    private ElementReference _fileInputRef;

    private IJSObjectReference? _climateMapRef;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _trends = await ClimateMonitor.GetBaselineTrendsAsync();
            StateHasChanged();

            // Initialize climate map with drill-down support
            _climateMapRef = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import", "/echarts-interop.js");
            await JSRuntime.InvokeAsync<bool>("initClimateGeoMap", "climate-geo-map");

            // Register .NET interop for map callbacks
            var dotNetRef = DotNetObjectReference.Create(this);
            await JSRuntime.InvokeVoidAsync("eval", $@"
                window.DotNetClimateMap = {dotNetRef};
            ");

            await UpdateWorldMap();
        }
    }

    [JSInvokable]
    public async Task OnCountryDrillDown(string iso3, string countryName)
    {
        _mapLevel = 1;
        _currentIso3 = iso3;
        _currentCountryName = countryName;
        StateHasChanged();
    }

    [JSInvokable]
    public void OnMapBackToWorld()
    {
        _mapLevel = 0;
        _currentIso3 = "";
        _currentCountryName = "";
        StateHasChanged();
    }

    [JSInvokable]
    public void OnProvinceClicked(string provinceName)
    {
        // Province click - could show detailed info in future
    }

    private async Task RunAnalysis()
    {
        _running = true;
        StateHasChanged();

        try
        {
            _input.TempAnomaly = _tempAnomaly;
            _input.PrecipChange = _precipChange;
            _input.SeaLevelRise = _seaLevelRise;
            _input.CarbonPrice = _carbonPrice;
            _input.CountryIso3 = DetectCountry(_input.Latitude, _input.Longitude);

            _assessment = await RiskEngine.AssessAsync(_input);

            // Update factory marker on map
            if (_input.Latitude != 0 || _input.Longitude != 0)
            {
                var riskColor = _assessment.Decision switch
                {
                    "REJECT" => "#ef4444",
                    "REVIEW" => "#f59e0b",
                    _ => "#22c55e"
                };
                await JSRuntime.InvokeAsync<bool>("setFactoryMarkerOnMap",
                    _input.Latitude, _input.Longitude, _input.RadiusKm,
                    _input.LocationName ?? "Fabrika", riskColor);
            }

            await UpdateAllCharts();
            await UpdateWorldMap();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Analysis error: {ex.Message}");
        }
        finally
        {
            _running = false;
            StateHasChanged();
        }
    }

    private async Task UpdateAllCharts()
    {
        if (_assessment == null) return;

        _pieEcharts ??= new EChartsInterop(JSRuntime, "climate-risk-pie");
        if (await _pieEcharts.InitAsync())
        {
            await _pieEcharts.SetOptionAsync(new
            {
                tooltip = new { trigger = "item", formatter = "{b}: {c} ({d}%)" },
                series = new[] { new
                {
                    type = "pie",
                    radius = new[] { "45%", "70%" },
                    center = new[] { "40%", "50%" },
                    data = new object[]
                    {
                        new { name = "Fiziksel", value = Math.Round(_assessment.PhysicalRisk * 100), itemStyle = new { color = "#ff9800" } },
                        new { name = "Gecis", value = Math.Round(_assessment.TransitionRisk * 100), itemStyle = new { color = "#9c27b0" } },
                        new { name = "Ekonomi", value = Math.Round((1 - _assessment.OverallRisk) * 100), itemStyle = new { color = "#4caf50" } }
                    }
                }}
            });
        }

        _barEcharts ??= new EChartsInterop(JSRuntime, "climate-economic-bar");
        if (await _barEcharts.InitAsync())
        {
            var ei = _assessment.EconomicImpact;
            await _barEcharts.SetOptionAsync(new
            {
                tooltip = new { trigger = "axis", axisPointer = new { type = "shadow" } },
                grid = new { left = "15%", right = "5%", bottom = "5%", top = "5%", containLabel = true },
                xAxis = new { type = "value" },
                yAxis = new { type = "category", data = new[] { "MDP", "CAPEX", "Sigorta", "Borc", "Operasyon" }, inverse = true },
                series = new[] { new
                {
                    type = "bar",
                    data = new object[]
                    {
                        new { value = Math.Round(ei.MdpLoss, 1), itemStyle = new { color = "#ff5722" } },
                        new { value = Math.Round(ei.CapexIncrease, 1), itemStyle = new { color = "#ff9800" } },
                        new { value = Math.Round(ei.InsuranceCost, 1), itemStyle = new { color = "#4caf50" } },
                        new { value = Math.Round(ei.BorrowingCost, 1), itemStyle = new { color = "#2196f3" } },
                        new { value = Math.Round(ei.OperationalCost, 1), itemStyle = new { color = "#9c27b0" } }
                    }
                }}
            });
        }

        _lineEcharts ??= new EChartsInterop(JSRuntime, "climate-scenario-line");
        if (await _lineEcharts.InitAsync())
        {
            await _lineEcharts.SetOptionAsync(new
            {
                tooltip = new { trigger = "axis" },
                grid = new { left = "5%", right = "10%", bottom = "5%", top = "5%", containLabel = true },
                xAxis = new { type = "category", data = new[] { "2025", "2035", "2050", "2075", "2100" } },
                yAxis = new { type = "value", min = 0, max = 1 },
                series = new object[]
                {
                    new { name = "SSP1", type = "line", data = new[] { 0.2, 0.25, 0.3, 0.35, 0.38 }, smooth = true, lineStyle = new { color = "#22c55e" }, itemStyle = new { color = "#22c55e" } },
                    new { name = "SSP2", type = "line", data = new[] { 0.2, 0.35, 0.5, 0.65, 0.72 }, smooth = true, lineStyle = new { color = "#ff9800" }, itemStyle = new { color = "#ff9800" } },
                    new { name = "SSP3", type = "line", data = new[] { 0.2, 0.45, 0.68, 0.85, 0.92 }, smooth = true, lineStyle = new { color = "#f44336" }, itemStyle = new { color = "#f44336" } },
                    new { name = "SSP5", type = "line", data = new[] { 0.2, 0.5, 0.75, 0.92, 0.98 }, smooth = true, lineStyle = new { color = "#9c27b0" }, itemStyle = new { color = "#9c27b0" } }
                }
            });
        }

        _gaugeEcharts ??= new EChartsInterop(JSRuntime, "climate-water-gauge");
        if (await _gaugeEcharts.InitAsync())
        {
            await _gaugeEcharts.SetOptionAsync(new
            {
                series = new[] { new
                {
                    type = "gauge",
                    startAngle = 180, endAngle = 0, min = 0, max = 5,
                    progress = new { show = true, width = 14 },
                    detail = new { valueAnimation = true, formatter = "{value}", offsetCenter = new[] { "0%", "65%" }, fontSize = 16, fontWeight = "bold" },
                    data = new[] { new { value = _assessment.WaterStress, name = "Su Stresi" } },
                    axisLine = new { lineStyle = new { width = 14, color = new object[] {
                        new object[] { 0.4, "#22c55e" },
                        new object[] { 0.7, "#ff9800" },
                        new object[] { 1.0, "#ef4444" }
                    }}},
                    pointer = new { show = false },
                    axisTick = new { show = false },
                    splitLine = new { show = false },
                    axisLabel = new { show = false }
                }}
            });
        }
    }

    private async Task UpdateWorldMap()
    {
        var allRisks = await WriAqueduct.GetAllCountryRisksAsync();
        // Build risk data with ISO3 for drill-down
        var riskData = allRisks.Select(r => new
        {
            name = r.Name,
            value = r.WaterStress,
            iso3 = GetIso3FromName(r.Name)
        }).ToArray();
        await JSRuntime.InvokeAsync<bool>("updateClimateWorldMap", "climate-geo-map", riskData);
    }

    private static string GetIso3FromName(string name) => name switch
    {
        "Turkey" or "Türkiye" => "TUR",
        "United States of America" or "United States" => "USA",
        "United Kingdom" => "GBR",
        "Germany" => "DEU",
        "France" => "FRA",
        "Italy" => "ITA",
        "China" => "CHN",
        "India" => "IND",
        "Japan" => "JPN",
        "Brazil" => "BRA",
        _ => ""
    };

    private async Task MapBackToWorld()
    {
        _mapLevel = 0;
        _currentIso3 = "";
        _currentCountryName = "";
        await JSRuntime.InvokeAsync<bool>("goBackToWorld");
        StateHasChanged();
    }

    private async Task ZoomIn() => await JSRuntime.InvokeAsync<bool>("zoomClimateMapIn");
    private async Task ZoomOut() => await JSRuntime.InvokeAsync<bool>("zoomClimateMapOut");
    private async Task ResetZoom() => await JSRuntime.InvokeAsync<bool>("resetClimateMapZoom");

    private void SetMapLevel(int level) { _mapLevel = level; StateHasChanged(); }
    private void UpdateTempAnomaly() { _tempAnomaly = _tempSlider / 10.0; }
    private void UpdatePrecipChange() { _precipChange = _precipSlider; }
    private void UpdateSeaLevel() { _seaLevelRise = _seaSlider / 100.0; }
    private void UpdateCarbonPrice() { _carbonPrice = _carbonSlider; }

    private async Task TriggerFileUpload()
    {
        await JSRuntime.InvokeAsync<object?>("eval", "document.querySelector('#climate-file-input').click()");
    }

    private async Task OnFileInputChange(ChangeEventArgs e)
    {
        var content = await JSRuntime.InvokeAsync<string>("eval", "(() => { const f = document.querySelector('#climate-file-input').files[0]; return f ? f.text() : ''; })()");
        if (string.IsNullOrEmpty(content)) return;

        if (content.TrimStart().StartsWith("{"))
        {
            try
            {
                var data = JsonSerializer.Deserialize<JsonElement>(content);
                if (data.TryGetProperty("latitude", out var lat) && data.TryGetProperty("longitude", out var lng))
                {
                    _input.Latitude = lat.GetDouble();
                    _input.Longitude = lng.GetDouble();
                    if (data.TryGetProperty("name", out var name)) _input.LocationName = name.GetString() ?? "";
                    if (data.TryGetProperty("radius_km", out var radius)) _input.RadiusKm = radius.GetDouble();
                    StateHasChanged();
                }
            }
            catch { }
        }
    }

    private static string DetectCountry(double lat, double lng)
    {
        if (lat > 36 && lat < 42 && lng > 26 && lng < 45) return "TUR";
        if (lat > 24 && lat < 50 && lng > -130 && lng < -65) return "USA";
        if (lat > 49 && lat < 61 && lng > -11 && lng < 2) return "GBR";
        if (lat > 47 && lat < 56 && lng > 5 && lng < 16) return "DEU";
        if (lat > 42 && lat < 52 && lng > -5 && lng < 10) return "FRA";
        if (lat > 36 && lat < 48 && lng > 6 && lng < 19) return "ITA";
        if (lat > 18 && lat < 54 && lng > 73 && lng < 135) return "CHN";
        if (lat > 6 && lat < 38 && lng > 68 && lng < 98) return "IND";
        if (lat > 30 && lat < 46 && lng > 128 && lng < 146) return "JPN";
        if (lat > -35 && lat < 6 && lng > -75 && lng < -34) return "BRA";
        return "TUR";
    }

    private static string GetRiskLabel(double score) => score switch
    {
        > 0.7 => "Yuksek",
        > 0.4 => "Orta",
        _ => "Dusuk"
    };

    private string GetPolicyDecision()
    {
        if (_assessment == null) return "—";
        return _assessment.Decision switch
        {
            "REJECT" => "REJECT (Red)",
            "REVIEW" => "REVIEW (Inceleme)",
            _ => "ALLOW (Izin)"
        };
    }

    private string GetPolicyColor()
    {
        if (_assessment == null) return "#666";
        return _assessment.Decision switch
        {
            "REJECT" => "#7b1fa2",
            "REVIEW" => "#e65100",
            _ => "#2e7d32"
        };
    }

    private string GetPolicyDescription()
    {
        if (_assessment == null) return "";
        return _assessment.Decision switch
        {
            "REJECT" => "Acil eylem gerekli, ust yonetim bilgilendirilmeli",
            "REVIEW" => "Iklim uyum onlemleri planlanmali, detayli degerlendirme gerekli",
            _ => "Standart risk izleme prosedurlerine devam"
        };
    }

    public async ValueTask DisposeAsync()
    {
        if (_pieEcharts != null) await _pieEcharts.DisposeAsync();
        if (_barEcharts != null) await _barEcharts.DisposeAsync();
        if (_lineEcharts != null) await _lineEcharts.DisposeAsync();
        if (_gaugeEcharts != null) await _gaugeEcharts.DisposeAsync();
        try { await JSRuntime.InvokeAsync<object?>("IntentumECharts.dispose", "climate-geo-map"); } catch { }
        try { await JSRuntime.InvokeVoidAsync("eval", "window.DotNetClimateMap = null;"); } catch { }
    }
}
