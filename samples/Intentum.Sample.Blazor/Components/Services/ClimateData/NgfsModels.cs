namespace Intentum.Sample.Blazor.Components.Services.ClimateData;

/// <summary>
/// NGFS Phase 5 senaryo verileri — makroekonomik ve iklim değişkenleri.
/// Veri: IIASA Scenario Explorer CSV formatı (IAMC: model, scenario, region, variable, unit, years).
/// </summary>
public sealed class NgfsScenarioData
{
    public string Model { get; set; } = "";
    public string Scenario { get; set; } = "";
    public string Region { get; set; } = "";
    public string Variable { get; set; } = "";
    public string Unit { get; set; } = "";
    public Dictionary<int, double> YearlyValues { get; set; } = [];
}

/// <summary>NGFS senaryo tanımı — dashboard'da seçim için.</summary>
public sealed class NgfsScenario
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = ""; // Orderly, Disorderly, HotHouse
    public string Description { get; set; } = "";
    public double WarmingLevel { get; set; } // °C
    public string TransitionRisk { get; set; } = ""; // Düşük/Orta/Yüksek/Çok Yüksek
    public string PhysicalRisk { get; set; } = ""; // Düşük/Orta/Yüksek/Çok Yüksek
}

/// <summary>NGFS'den hesaplanan bir ülke/region için makroekonomik özet.</summary>
public sealed class NgfsMacroSnapshot
{
    public string Region { get; set; } = "";
    public string Scenario { get; set; } = "";
    public int Year { get; set; }
    public double? GdpChange { get; set; } // % change from baseline
    public double? CarbonPrice { get; set; } // $/tCO2
    public double? TemperatureChange { get; set; } // °C above pre-industrial
    public double? EnergyInvestment { get; set; } // relative change
    public double? EmploymentChange { get; set; } // percentage points
}

/// <summary>NGFS Phase 5 resmi senaryo listesi.</summary>
public static class NgfsScenarios
{
    public static readonly List<NgfsScenario> All =
    [
        new()
        {
            Id = "n nz2050", Name = "Net Zero 2050", Category = "Orderly",
            Description = "1.5°C hedefi için hemen harekete geçilir, net sıfır emisyon 2050'de ulaşılır.",
            WarmingLevel = 1.5, TransitionRisk = "Yüksek", PhysicalRisk = "Düşük"
        },
        new()
        {
            Id = "n below2c", Name = "Below 2°C", Category = "Orderly",
            Description = "1.7°C altında sınırlama, erken ve kademeli geçiş.",
            WarmingLevel = 1.7, TransitionRisk = "Yüksek", PhysicalRisk = "Düşük-Orta"
        },
        new()
        {
            Id = "n delayed", Name = "Delayed Transition", Category = "Disorderly",
            Description = "2030'a kadar gecikme, sonra ani ve maliyetli geçiş.",
            WarmingLevel = 1.5, TransitionRisk = "Çok Yüksek", PhysicalRisk = "Orta"
        },
        new()
        {
            Id = "n ndcs", Name = "NDCs", Category = "Disorderly",
            Description = "Mevcut ulusal katkı beyanları sürdürülür.",
            WarmingLevel = 1.8, TransitionRisk = "Orta", PhysicalRisk = "Orta-Yüksek"
        },
        new()
        {
            Id = "n fragmented", Name = "Fragmented World", Category = "Disorderly",
            Description = "Bölgesel dağılma, bazı ülkeler gecikir, diğerleri erken harekete geçer.",
            WarmingLevel = 2.3, TransitionRisk = "Bölgesel", PhysicalRisk = "Yüksek"
        },
        new()
        {
            Id = "n currpol", Name = "Current Policies", Category = "HotHouse",
            Description = "Sadece mevcut politikalar sürdürülür, ciddi fiziksel riskler.",
            WarmingLevel = 3.0, TransitionRisk = "Düşük", PhysicalRisk = "Çok Yüksek"
        },
        new()
        {
            Id = "n lowdemand", Name = "Low Demand", Category = "Orderly",
            Description = "Talep tarafı önlemlerle enerji tüketimi azaltılır.",
            WarmingLevel = 1.5, TransitionRisk = "Orta", PhysicalRisk = "Düşük"
        }
    ];

    public static NgfsScenario? GetById(string id) =>
        All.FirstOrDefault(s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    public static NgfsScenario? GetByName(string name) =>
        All.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
}
