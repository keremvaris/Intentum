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
    private string _currentProvinceName = "";

    private List<CompanyProfile> _profiles = new();
    private CompanyProfile? _selectedProfile;
    private List<ScenarioComparisonResult> _scenarioResults = new();
    private bool _drawerOpen;
    private List<NgfsMacroSnapshot> _ngfsComparison = new();
    private ClimateVarResult? _varResult;
    private CompanyProfile? _editingProfile;
    private bool _isNewProfile;

    private string _selectedProvince = "";
    private List<(string Name, double Score)> _selectedProvinceRisks = [];
    private string _selectedProvinceCountry = "";
    private bool _showHelp;

    private string _importMessage = "";
    private bool _importError;

    protected override async Task OnInitializedAsync()
    {
        _profiles = ProfileService.GetAll().ToList();
    }

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
    private EChartsInterop? _hazardExposureEcharts;
    private EChartsInterop? _scenarioHeatEcharts;

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
            await JSRuntime.InvokeVoidAsync("registerDotNetClimateMap", DotNetObjectReference.Create(this));

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
        _currentProvinceName = "";
        StateHasChanged();
    }

    [JSInvokable]
    public void OnMapBackToCountry()
    {
        _mapLevel = 1;
        _currentProvinceName = "";
        StateHasChanged();
    }

    [JSInvokable]
    public void OnDistrictDrillDown(string provinceName)
    {
        _mapLevel = 2;
        _currentProvinceName = provinceName;
        StateHasChanged();
    }

    [JSInvokable]
    public void OnProvinceClicked(string provinceName, string riskValue, string countryName)
    {
        _selectedProvince = provinceName;
        _selectedProvinceCountry = countryName;
        _selectedProvinceRisks = BuildProvinceRisks(provinceName);
        StateHasChanged();
    }

    // Seçilen il için deterministik (isim-hash tabanlı) risk profili üretir.
    private List<(string Name, double Score)> BuildProvinceRisks(string provinceName)
    {
        var hash = 0;
        foreach (var c in provinceName) { hash = ((hash << 5) - hash) + c; hash &= hash; }
        var norm = (Math.Abs(hash % 1000) / 1000.0); // 0..1

        // Ana riskler: ülke bazlı skorlar il ismine göre hafif varyasyonla türetilir.
        double WaterStress() => Clamp01(3.4 + (norm - 0.5) * 2);
        double Flood() => Clamp01(1.5 + (((norm * 7) % 1) - 0.5) * 3);
        double Drought() => Clamp01(2.3 + (((norm * 13) % 1) - 0.5) * 4);
        double Heat() => Clamp01(2.8 + (((norm * 17) % 1) - 0.5) * 3);

        return
        [
            ("Su Stresi", WaterStress()),
            ("Sel Riski", Flood()),
            ("Kuraklık", Drought()),
            ("Sıcaklık", Heat())
        ];
    }

    private static double Clamp01(double v) => Math.Clamp(v, 0, 5);

    private async Task RunAnalysis()
    {
        // Validate coordinates
        if (Math.Abs(_input.Latitude) < 0.01 && Math.Abs(_input.Longitude) < 0.01)
        {
            // Default to Ankara if no coordinates entered
            _input.Latitude = 39.93;
            _input.Longitude = 32.86;
            _input.LocationName = string.IsNullOrWhiteSpace(_input.LocationName) ? "Ankara" : _input.LocationName;
        }

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

            if (_selectedProfile != null)
            {
                _scenarioResults = await ComparisonEngine.CompareAllAsync(_selectedProfile, _input);
            }

            // NGFS karşılaştırma verisi.
            _ngfsComparison = await NgfsService.GetComparisonAsync(_input.CountryIso3, _input.Horizon);

            // Climate VaR
            var ngfsScenarioIds = _ngfsComparison.Select(s => s.Scenario).Distinct().ToList();
            if (ngfsScenarioIds.Count > 0 && _selectedProfile != null)
            {
                _varResult = await VarEngine.CalculateAsync(_selectedProfile, _input, ngfsScenarioIds);
            }

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
                    _input.LocationName ?? _selectedProfile?.Name ?? "Konum", riskColor);
            }

            BuildRiskMatrices();
            await UpdateAllCharts();
            await UpdateWorldMap();

            // Şirket profili seçiliyse lokasyonu otomatik "seçili il" yap — manuel harita tıklaması gerekmesin.
            if (_selectedProfile != null)
            {
                _selectedProvince = _selectedProfile.LocationName;
                _selectedProvinceRisks = BuildProvinceRisks(_selectedProfile.LocationName);
                _selectedProvinceCountry = _currentCountryName;
            }
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

    // IPCC risk çerçevesi: Tehlike × Maruziyet × Kırılganlık matrix'lerini hesaplar.
    private void BuildRiskMatrices()
    {
        if (_assessment == null) return;

        // Tehlike skoru: RiskFactors listesindeki faktör adını RiskMatrixEngine tehlikeleriyle eşleştir.
        double Hazard(string hazardName)
        {
            var factor = _assessment.RiskFactors.FirstOrDefault(f => f.Name == hazardName);
            return factor?.Score ?? 0;
        }

        if (_selectedProfile != null)
        {
            _assessment.HazardExposureMatrix = MatrixEngine.ComputeHazardExposureMatrix(_selectedProfile, Hazard);

            var risksByScenario = _scenarioResults.ToDictionary(
                r => r.Scenario,
                r => r.Assessment.OverallRisk);
            _assessment.ScenarioMatrix = MatrixEngine.ComputeScenarioMatrix(_selectedProfile, risksByScenario, Hazard);
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
                xAxis = new { type = "category", data = _scenarioResults.Select(r => (object)r.Scenario).ToArray() },
                yAxis = new { type = "value", min = 0, max = 1 },
                series = new object[]
                {
                    new { name = "Fiziksel Risk", type = "bar", data = _scenarioResults.Select(r => (object)Math.Round(r.Assessment.PhysicalRisk, 2)).ToArray(), itemStyle = new { color = "#38bdf8" } },
                    new { name = "Gecis Riski", type = "bar", data = _scenarioResults.Select(r => (object)Math.Round(r.Assessment.TransitionRisk, 2)).ToArray(), itemStyle = new { color = "#fb923c" } }
                }
            });
        }

        // NGFS makro karşılaştırma grafiği.
        if (_ngfsComparison.Count > 0)
        {
            var ngfsEcharts = new EChartsInterop(JSRuntime, "climate-ngfs-comparison");
            if (await ngfsEcharts.InitAsync())
            {
                var scenarios = _ngfsComparison.Select(s => s.Scenario).Distinct().ToList();
                var categories = scenarios.Select(s => (object)s).ToArray();
                var gdpData = scenarios.Select(s => (object)Math.Round(_ngfsComparison.FirstOrDefault(x => x.Scenario == s && x.GdpChange != null)?.GdpChange ?? 0, 1)).ToArray();
                var carbonData = scenarios.Select(s => (object)Math.Round(_ngfsComparison.FirstOrDefault(x => x.Scenario == s && x.CarbonPrice != null)?.CarbonPrice ?? 0, 0)).ToArray();

                await ngfsEcharts.SetOptionAsync(new
                {
                    tooltip = new { trigger = "axis", axisPointer = new { type = "shadow" } },
                    legend = new { data = new[] { "GSYİH Değişimi (%)", "Karbon Fiyatı ($/tCO₂)" }, textStyle = new { color = "#94a3b8", fontSize = 10 }, top = 0 },
                    grid = new { left = "8%", right = "8%", bottom = "5%", top = "18%", containLabel = true },
                    xAxis = new { type = "category", data = categories },
                    yAxis = new[]
                    {
                        new { type = "value", name = "GSYİH %", position = "left", axisLabel = new { color = "#94a3b8", fontSize = 9 } },
                        new { type = "value", name = "$/tCO₂", position = "right", axisLabel = new { color = "#94a3b8", fontSize = 9 } }
                    },
                    series = new object[]
                    {
                        new { name = "GSYİH Değişimi (%)", type = "bar", yAxisIndex = 0, data = gdpData, itemStyle = new { color = "#22c55e" } },
                        new { name = "Karbon Fiyatı ($/tCO₂)", type = "bar", yAxisIndex = 1, data = carbonData, itemStyle = new { color = "#60a5fa" } }
                    }
                });
            }
        }

        // Climate VaR chart
        if (_varResult != null && _varResult.LossDistribution.Count > 0)
        {
            var varEcharts = new EChartsInterop(JSRuntime, "climate-var-bar");
            if (await varEcharts.InitAsync())
            {
                var names = _varResult.LossDistribution.Select(l => (object)l.ScenarioName).ToArray();
                var losses = _varResult.LossDistribution.Select(l => (object)Math.Round(l.Loss, 0)).ToArray();
                var colors = _varResult.LossDistribution.Select(l => (object)(l.Loss >= 0 ? "#22c55e" : "#ef4444")).ToArray();

                await varEcharts.SetOptionAsync(new
                {
                    tooltip = new { trigger = "axis", axisPointer = new { type = "shadow" } },
                    grid = new { left = "10%", right = "5%", bottom = "15%", top = "5%", containLabel = true },
                    xAxis = new { type = "category", data = names, axisLabel = new { rotate = 30, fontSize = 9, color = "#94a3b8" } },
                    yAxis = new { type = "value", name = "Kayıp (TL)", axisLabel = new { color = "#94a3b8", fontSize = 9 } },
                    series = new[] { new { type = "bar", data = losses.Select((l, i) => new { value = l, itemStyle = new { color = colors[i] } }).ToArray() } }
                });
            }
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
                    detail = new { valueAnimation = true, formatter = "{value}", offsetCenter = new[] { "0%", "65%" }, fontSize = 22, fontWeight = "bold" },
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

        // IPCC risk matrix heatmap'leri: Tehlike × Varlık ve Tehlike × Senaryo
        if (_assessment.HazardExposureMatrix != null)
        {
            _hazardExposureEcharts ??= new EChartsInterop(JSRuntime, "climate-hazard-exposure");
            if (await _hazardExposureEcharts.InitAsync())
            {
                var m = _assessment.HazardExposureMatrix;
                var xIdx = m.Categories.Select((c, i) => i).ToArray();
                var data = m.Cells.Select(cell => new object[]
                {
                    (object)m.Categories.IndexOf(cell.Category),
                    (object)m.Hazards.IndexOf(cell.Hazard),
                    (object)Math.Round(cell.Value, 2)
                }).ToArray();
                await _hazardExposureEcharts.SetHeatmapOptionAsync(new
                {
                    tooltip = new { trigger = "item" },
                    grid = new { left = "2%", right = "2%", bottom = "2%", top = "2%", containLabel = true },
                    xAxis = new { type = "category", data = m.Categories, splitArea = new { show = true }, splitLine = new { show = false }, axisLabel = new { fontSize = 10 } },
                    yAxis = new { type = "category", data = m.Hazards, splitArea = new { show = true }, splitLine = new { show = false }, inverse = true, axisLabel = new { fontSize = 10 } },
                    visualMap = new { min = 0, max = 1, show = false, inRange = new { color = new[] { "#22c55e", "#84cc16", "#f59e0b", "#ef4444", "#991b1b" } } },
                    series = new[] { new { type = "heatmap", data = data, label = new { show = true, fontSize = 10, color = "#e5e7eb" }, itemStyle = new { borderColor = "#0d1117", borderWidth = 1 }, emphasis = new { itemStyle = new { borderColor = "#fff", borderWidth = 2 } } } }
                });
            }
        }

        if (_assessment.ScenarioMatrix != null)
        {
            _scenarioHeatEcharts ??= new EChartsInterop(JSRuntime, "climate-scenario-heat");
            if (await _scenarioHeatEcharts.InitAsync())
            {
                var m = _assessment.ScenarioMatrix;
                var data = m.Cells.Select(cell => new object[]
                {
                    (object)m.Scenarios.IndexOf(cell.Category),
                    (object)m.Hazards.IndexOf(cell.Hazard),
                    (object)Math.Round(cell.Value, 2)
                }).ToArray();
                await _scenarioHeatEcharts.SetHeatmapOptionAsync(new
                {
                    tooltip = new { trigger = "item" },
                    grid = new { left = "2%", right = "2%", bottom = "2%", top = "2%", containLabel = true },
                    xAxis = new { type = "category", data = m.Scenarios, splitArea = new { show = true }, splitLine = new { show = false }, axisLabel = new { fontSize = 10 } },
                    yAxis = new { type = "category", data = m.Hazards, splitArea = new { show = true }, splitLine = new { show = false }, inverse = true, axisLabel = new { fontSize = 10 } },
                    visualMap = new { min = 0, max = 1, show = false, inRange = new { color = new[] { "#22c55e", "#84cc16", "#f59e0b", "#ef4444", "#991b1b" } } },
                    series = new[] { new { type = "heatmap", data = data, label = new { show = true, fontSize = 10, color = "#e5e7eb" }, itemStyle = new { borderColor = "#0d1117", borderWidth = 1 }, emphasis = new { itemStyle = new { borderColor = "#fff", borderWidth = 2 } } } }
                });
            }
        }

        // Grid içinde boyutlar oturunca chart'ları yeniden boyutlandır.
        try
        {
            if (_pieEcharts != null) await _pieEcharts.ResizeAsync();
            if (_barEcharts != null) await _barEcharts.ResizeAsync();
            if (_lineEcharts != null) await _lineEcharts.ResizeAsync();
            if (_gaugeEcharts != null) await _gaugeEcharts.ResizeAsync();
            if (_hazardExposureEcharts != null) await _hazardExposureEcharts.ResizeAsync();
            if (_scenarioHeatEcharts != null) await _scenarioHeatEcharts.ResizeAsync();
        }
        catch { }
    }

    private async Task UpdateWorldMap()
    {
        var allRisks = await WriAqueduct.GetAllCountryRisksAsync();
        // Build name→risk mapping for JS (match by country name + iso3)
        var riskData = allRisks.Select(r => new
        {
            name = r.Name,
            value = Math.Round(r.WaterStress, 1),
            iso3 = r.Iso3
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
        _currentProvinceName = "";
        await JSRuntime.InvokeAsync<bool>("resetMapToWorld");
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
        _importMessage = "";
        _importError = false;

        var content = await JSRuntime.InvokeAsync<string>("eval", "(() => { const f = document.querySelector('#climate-file-input').files[0]; return f ? f.text() : ''; })()");
        if (string.IsNullOrEmpty(content))
        {
            _importMessage = "Dosya okunamadı.";
            _importError = true;
            StateHasChanged();
            return;
        }

        var result = CompanyProfileImporter.Parse(content);
        if (!result.IsSuccess || result.Profiles == null || result.Profiles.Count == 0)
        {
            _importMessage = $"İçe aktarma başarısız: {result.Error}";
            _importError = true;
            StateHasChanged();
            return;
        }

        foreach (var profile in result.Profiles)
        {
            ProfileService.Add(profile);
        }

        _profiles = ProfileService.GetAll().ToList();
        var first = result.Profiles[0];
        SelectProfile(first.Id);
        _importMessage = $"✅ {result.Profiles.Count} şirket eklendi: {string.Join(", ", result.Profiles.Select(p => p.Name))}";
        _importError = false;
        StateHasChanged();
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

    // 0-5 ölçekli su stresi riski için etiket ve renk
    private static string GetRiskColor(double score) => score switch
    {
        > 4 => "#991b1b",
        > 3 => "#ef4444",
        > 2 => "#f59e0b",
        _ => "#22c55e"
    };

    private static string GetRiskLevel5(double score) => score switch
    {
        > 4 => "Çok Yüksek",
        > 3 => "Yüksek",
        > 2 => "Orta",
        _ => "Düşük"
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

    private string GetDataConfidenceColor()
    {
        if (_assessment == null) return "#666";
        return _assessment.DataConfidence switch
        {
            >= 0.9 => "#22c55e",
            >= 0.75 => "#84cc16",
            >= 0.5 => "#eab308",
            >= 0.25 => "#f97316",
            _ => "#ef4444"
        };
    }

    private void SelectProfile(string profileId)
    {
        _input.CompanyProfileId = profileId;
        _selectedProfile = _profiles.FirstOrDefault(p => p.Id == profileId);
        if (_selectedProfile != null)
        {
            _input.LocationName = _selectedProfile.LocationName;
            _input.Latitude = _selectedProfile.Latitude;
            _input.Longitude = _selectedProfile.Longitude;
            _input.Sector = _selectedProfile.Sector;
        }
    }

    private void HandleProfileSave(CompanyProfile profile)
    {
        ProfileService.Update(profile);
        _profiles = ProfileService.GetAll().ToList();
        _selectedProfile = _profiles.FirstOrDefault(p => p.Id == profile.Id);
    }

    private void OpenDrawer()
    {
        _editingProfile = _selectedProfile;
        _isNewProfile = false;
        _drawerOpen = true;
    }

    private void OpenNewProfileDrawer()
    {
        _editingProfile = null;
        _isNewProfile = true;
        _drawerOpen = true;
    }

    private string GetDecisionClass(string decision) => decision switch
    {
        "REJECT" => "intent-badge-reject",
        "REVIEW" => "intent-badge-review",
        _ => "intent-badge-allow"
    };

    private string GetDecisionIcon(string decision) => decision switch
    {
        "REJECT" => "🔴",
        "REVIEW" => "🟡",
        _ => "🟢"
    };

    private string GetCategoryLabel(FinancialCategoryType type) => type switch
    {
        FinancialCategoryType.Revenue => "Gelir",
        FinancialCategoryType.Opex => "Operasyonel Giderler",
        FinancialCategoryType.Capex => "Kısa Vadeli Yatırımlar",
        FinancialCategoryType.CashFlow => "Uzun Vadeli Nakit Akışı",
        _ => type.ToString()
    };

    private string GetBarWidth(double value, FinancialImpact impact)
    {
        // Kategori bazlı görünür genişlik: mutlak değerin en büyük etkiye oranı.
        var maxCat = impact.CategoryImpacts
            .Select(c => Math.Max(Math.Abs(c.PhysicalImpact), Math.Abs(c.TransitionImpact)))
            .DefaultIfEmpty(1)
            .Max();
        var maxAbs = Math.Max(maxCat, 1);
        var pct = Math.Abs(value) / maxAbs * 100;
        // Çok küçük etkilerde bile bar görünür olsun diye min %8 taban kullan.
        return pct <= 0.5 ? "0%" : $"{Math.Max(pct, 8):F1}%";
    }

    private string FormatCurrency(double amount) => amount switch
    {
        < -1_000_000 => $"-{Math.Abs(amount / 1_000_000):F1}M ₺",
        < 0 => $"-{Math.Abs(amount / 1_000):F0}K ₺",
        > 1_000_000 => $"+{amount / 1_000_000:F1}M ₺",
        > 0 => $"+{amount / 1_000:F0}K ₺",
        _ => "0 ₺"
    };

    private string GetSignalCategoryColor(string source) => source switch
    {
        _ when source.StartsWith("physical") => "#38bdf8",
        _ when source.StartsWith("transition") => "#fb923c",
        _ when source.StartsWith("economic") => "#a78bfa",
        _ => "#94a3b8"
    };

    private string GetSignalCategoryLabel(string source) => source switch
    {
        _ when source.StartsWith("physical") => "Fiziksel Risk",
        _ when source.StartsWith("transition") => "Geçiş Riski",
        _ when source.StartsWith("economic") => "Finansal Risk",
        _ => "Diğer"
    };

    private static string GetSignalTooltip(string source, double weight) => source switch
    {
        "physical:water_stress" => $"Su Stresi: WRI Aqueduct verisine göre bölgenin su talebinin su kaynaklarına oranı ({weight:F2}). Yüksek değer → su kıtlığı riski artar.",
        "physical:flood" => $"Sel Riski: WRI Aqueduct nehir taşkını verisi ({weight:F2}). Yüksek değer → taşkın hasar potansiyeli yüksek.",
        "physical:drought" => $"Kuraklık Riski: WRI Aqueduct kuraklık endeksi ({weight:F2}). Yüksek değer → uzun süreli kuraklık beklentisi.",
        "physical:heatwave" => $"Sıcak Dalgası: Open-Meteo iklim projeksiyonundan sıcaklık maksimumları ({weight:F2}). Yüksek değer → aşırı sıcaklık günleri artar.",
        "physical:storm" => $"Fırtına: Open-Meteo rüzgar hızı maksimumları ({weight:F2}). Yüksek değer → fırtına hasar riski yükselir.",
        "physical:sea_level" => $"Deniz Seviyesi: GeoRiskHelper'dan hesapanan efektif deniz seviyesi yüksekliği ({weight:F2}). Kıyı tesisleri için doğrudan tehdit.",
        "transition:market" => $"Piyasa Riski: Karbon fiyatı, emtia fiyatları ve talep değişimleri ({weight:F2}). Geçiş sürecinde pazar payı kaybı riski.",
        "transition:policy" => $"Politika/Regülasyon: Karbon fiyatına duyarlılık + regülasyon değişiklikleri ({weight:F2}). Yeni düzenlemelere uyum maliyeti.",
        "transition:technology" => $"Teknoloji Dönüşümü: Düşük karbon teknolojilerine geçiş hızı ({weight:F2}). Mevcut teknolojinin eskimesi riski.",
        "transition:reputation" => $"İtibar Riski: Paydaş baskısı ve kamuoyu algısı ({weight:F2}). İklim eylemlerinden itibar kaybı riski.",
        "economic:revenue_at_risk" => $"Gelir Kaybı: İklim riskinin şirkete gelir etkisi / toplam ciro ({weight:F2}). Yüksek değer → gelir kaybı cironun büyük kısmını etkiler.",
        "economic:operational_expenses" => $"Operasyonel Giderler: İklim riskinin operasyonel maliyetlere etkisi / ciro ({weight:F2}). Yüksek değer → bakım, onarım, iş gücü maliyetleri artar.",
        "economic:cost_of_goods" => $"Maliyet Riski: Hammaddedarı ve lojistik maliyetleri üzerindeki iklim etkisi / ciro ({weight:F2}). Yüksek değer → tedarik zinciri kesintileri.",
        "economic:capital_expenditure" => $"Yatırım Riski: Yeni yatırım gereksinimleri ve mevcut varlıkların değer kaybı / ciro ({weight:F2}). Yüksek değer → sermaye yoğun yatırımlar gerekir.",
        "economic:impact" => $"Ekonomik Etki: Fiziksel ve geçiş riskinin birleşik Parasal etkisi ({weight:F2}). Tüm sektörler için genel ekonomik yansıma.",
        "signal:missing_data" => $"Eksik Veri: Kullanılamayan veri kaynaklarının sayısı ({weight:F2}). Yüksek değer → analiz güvenilirliği düşer, REVIEW kararı tetiklenir.",
        "signal:regional_estimate" => $"Bölgesel Tahmin: Yerel veri olmadığı için ülke profili kullanıldı ({weight:F2}). Yüksek değer → analiz tahminsel, doğruluk payı düşük.",
        _ => $"{source}: Ağırlık {weight:F2}"
    };

    public async ValueTask DisposeAsync()
    {
        if (_pieEcharts != null) await _pieEcharts.DisposeAsync();
        if (_barEcharts != null) await _barEcharts.DisposeAsync();
        if (_lineEcharts != null) await _lineEcharts.DisposeAsync();
        if (_gaugeEcharts != null) await _gaugeEcharts.DisposeAsync();
        if (_hazardExposureEcharts != null) await _hazardExposureEcharts.DisposeAsync();
        if (_scenarioHeatEcharts != null) await _scenarioHeatEcharts.DisposeAsync();
        try { await JSRuntime.InvokeAsync<object?>("IntentumECharts.dispose", "climate-geo-map"); } catch { }
        try { await JSRuntime.InvokeVoidAsync("unregisterDotNetClimateMap"); } catch { }
    }
}
