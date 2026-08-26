using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Intentum.Sample.Blazor.Components.Services;
using Intentum.Sample.Blazor.Components.Services.ClimateData;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace Intentum.Sample.Blazor.Components.Pages.Examples;

public sealed partial class ClimateRiskDashboard
{
    private RiskInput _input = new();
    private RiskAssessment? _assessment;
    private ClimateBaselineTrends? _trends;
    private bool _running;
    private int _mapLevel;

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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _trends = await ClimateMonitor.GetBaselineTrendsAsync();
            StateHasChanged();

            await JSRuntime.InvokeAsync<bool>("initClimateGeoMap", new object?[] { "climate-geo-map" });
            await UpdateWorldMap();
        }
    }

    private async Task RunAnalysis()
    {
        _running = true;
        StateHasChanged();

        try
        {
            _assessment = await RiskEngine.AssessAsync(_input);
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
        var riskData = allRisks.Select(r => new { name = r.Name, value = r.WaterStress }).ToArray();
        await JSRuntime.InvokeAsync<bool>("updateClimateWorldMap", new object?[] { "climate-geo-map", riskData });
    }

    private void SetMapLevel(int level) { _mapLevel = level; StateHasChanged(); }
    private void UpdateTempAnomaly() { _tempAnomaly = _tempSlider / 10.0; }
    private void UpdatePrecipChange() { _precipChange = _precipSlider; }
    private void UpdateSeaLevel() { _seaLevelRise = _seaSlider / 100.0; }
    private void UpdateCarbonPrice() { _carbonPrice = _carbonSlider; }

    private void ZoomIn() { }
    private void ZoomOut() { }
    private void ResetZoom() { }

    private async Task TriggerFileUpload()
    {
        await JSRuntime.InvokeAsync<object?>("eval", new object?[] { "document.querySelector('#climate-file-input').click()" });
    }

    private async Task OnFileInputChange(ChangeEventArgs e)
    {
        var content = await JSRuntime.InvokeAsync<string>("eval", new object?[] { "(() => { const f = document.querySelector('#climate-file-input').files[0]; return f ? f.text() : ''; })()" });
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

    private static string GetRiskLabel(double score) => score switch
    {
        > 0.7 => "Yuksek",
        > 0.4 => "Orta",
        _ => "Dusuk"
    };

    private string GetPolicyDecision()
    {
        if (_assessment == null) return "—";
        return _assessment.OverallRisk switch
        {
            > 0.7 => "ESCALATE",
            > 0.5 => "WARN",
            > 0.3 => "OBSERVE",
            _ => "ALLOW"
        };
    }

    private string GetPolicyColor()
    {
        if (_assessment == null) return "#666";
        return _assessment.OverallRisk switch
        {
            > 0.7 => "#7b1fa2",
            > 0.5 => "#e65100",
            > 0.3 => "#1565c0",
            _ => "#2e7d32"
        };
    }

    private string GetPolicyDescription()
    {
        if (_assessment == null) return "";
        return _assessment.OverallRisk switch
        {
            > 0.7 => "Acil eylem gerekli, ust yonetim bilgilendirilmeli",
            > 0.5 => "Iklim uyum onlemleri planlanmali",
            > 0.3 => "Risk gostergeleri duzenli izlenmeli",
            _ => "Standart risk izleme prosedurlerine devam"
        };
    }

    public async ValueTask DisposeAsync()
    {
        if (_pieEcharts != null) await _pieEcharts.DisposeAsync();
        if (_barEcharts != null) await _barEcharts.DisposeAsync();
        if (_lineEcharts != null) await _lineEcharts.DisposeAsync();
        if (_gaugeEcharts != null) await _gaugeEcharts.DisposeAsync();
        try { await JSRuntime.InvokeAsync<object?>("IntentumECharts.dispose", new object?[] { "climate-geo-map" }); } catch { }
    }
}
