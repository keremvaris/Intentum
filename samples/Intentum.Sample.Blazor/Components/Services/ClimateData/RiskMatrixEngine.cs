namespace Intentum.Sample.Blazor.Components.Services.ClimateData;

/// <summary>
/// IPCC risk çerçevesi: Risk = Tehlike (Hazard) × Maruziyet (Exposure) × Kırılganlık (Vulnerability).
/// - Tehlike: iklim modeli/ölçümlerinden (RiskCalculationEngine'in ürettiği risk skorları).
/// - Maruziyet: şirketin varlığının finansal değeri (Revenue/Opex/Capex/CashFlow → normalize 0-1).
/// - Kırılganlık: f(Hassasiyet / Uyum Kapasitesi) her satır kalemi için.
/// İki matrix üretir: Tehlike×Varlık ve Tehlike×Senaryo.
/// </summary>
public sealed class RiskMatrixEngine
{
    public static readonly string[] HazardNames =
    [
        "Sıcaklık Artışı",
        "Yağış Değişimi",
        "Maks. Rüzgar",
        "Deniz Seviyesi",
        "Su Stresi",
        "Sel Riski",
        "Kuraklık Riski"
    ];

    /// <summary>Kırılganlık = clamp(Sensitivity / AdaptiveCapacity, 0, 1). Uyum kapasitesi 0 ise maksimum olur.</summary>
    public double ComputeVulnerability(double sensitivity, double adaptiveCapacity)
    {
        if (adaptiveCapacity <= 0) return 1.0;
        return Math.Clamp(sensitivity / adaptiveCapacity, 0, 1);
    }

    /// <summary>Kırılganlık × Maruziyet × Tehlike. Sonuç 0-1 arasına sıkıştırılır.</summary>
    public double ComputeRiskScore(double hazard, double exposure, double vulnerability)
        => Math.Clamp(hazard * exposure * vulnerability, 0, 1);

    /// <summary>Tehlike × Varlık matrix'i. Her hücre, o tehlike + o varlık kategorisi için toplam risk skoru.</summary>
    public HazardExposureMatrix ComputeHazardExposureMatrix(CompanyProfile profile, Func<string, double> hazardProvider)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(hazardProvider);

        var categories = profile.Categories
            .Where(c => c.Total > 0)
            .Select(c => c.Name)
            .Distinct()
            .ToList();
        if (categories.Count == 0) categories.Add("Genel");

        var cells = new List<MatrixCell>();
        foreach (var hazard in HazardNames)
        {
            var h = Math.Clamp(hazardProvider(hazard), 0, 1);
            foreach (var category in categories)
            {
                var exposure = ComputeExposure(profile, category);
                var vulnerability = ComputeCategoryVulnerability(profile, category);
                cells.Add(new MatrixCell
                {
                    Hazard = hazard,
                    Category = category,
                    Value = ComputeRiskScore(h, exposure, vulnerability)
                });
            }
        }

        return new HazardExposureMatrix
        {
            Hazards = HazardNames.ToList(),
            Categories = categories,
            Cells = cells
        };
    }

    /// <summary>Tehlike × Senaryo matrix'i. Her hücre, o tehlike + o senaryo için toplam risk skoru.</summary>
    public ScenarioMatrix ComputeScenarioMatrix(
        CompanyProfile profile,
        IReadOnlyDictionary<string, double> risksByScenario,
        Func<string, double> hazardProvider)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(risksByScenario);
        ArgumentNullException.ThrowIfNull(hazardProvider);

        var scenarios = risksByScenario.Keys.ToList();
        var cells = new List<MatrixCell>();
        foreach (var hazard in HazardNames)
        {
            var h = Math.Clamp(hazardProvider(hazard), 0, 1);
            foreach (var scenario in scenarios)
            {
                var scenarioRisk = Math.Clamp(risksByScenario[scenario], 0, 1);
                var exposure = ComputeTotalExposure(profile);
                var vulnerability = ComputeTotalVulnerability(profile);
                cells.Add(new MatrixCell
                {
                    Hazard = hazard,
                    Category = scenario,
                    Value = ComputeRiskScore(h, exposure, vulnerability * scenarioRisk)
                });
            }
        }

        return new ScenarioMatrix
        {
            Hazards = HazardNames.ToList(),
            Scenarios = scenarios,
            Cells = cells
        };
    }

    /// <summary>Belirli bir varlık kategorisinin normalize maruziyeti (0-1).</summary>
    public double ComputeExposure(CompanyProfile profile, string categoryName)
    {
        var category = profile.Categories.FirstOrDefault(c => c.Name == categoryName);
        if (category == null) return 0;
        // En büyük kategori değeri 200M kabul edilir, üstü 1'e sıkışır.
        return Math.Clamp(category.Total / 200_000_000.0, 0, 1);
    }

    /// <summary>Şirketin toplam maruziyeti (tüm kategoriler, normalize).</summary>
    public double ComputeTotalExposure(CompanyProfile profile)
        => Math.Clamp(profile.Categories.Sum(c => c.Total) / 400_000_000.0, 0, 1);

    private double ComputeCategoryVulnerability(CompanyProfile profile, string categoryName)
    {
        var items = profile.Categories
            .FirstOrDefault(c => c.Name == categoryName)?
            .LineItems ?? [];
        if (items.Count == 0) return 0;
        // Kategorinin kırılganlığı = satır kalemlerinin ortalama kırılganlığı.
        return items.Select(i => ComputeVulnerability(i.Sensitivity, i.AdaptiveCapacity)).Average();
    }

    private double ComputeTotalVulnerability(CompanyProfile profile)
    {
        var items = profile.Categories.SelectMany(c => c.LineItems).ToList();
        if (items.Count == 0) return 0;
        return items.Select(i => ComputeVulnerability(i.Sensitivity, i.AdaptiveCapacity)).Average();
    }
}

public sealed class MatrixCell
{
    public string Hazard { get; set; } = "";
    public string Category { get; set; } = "";
    public double Value { get; set; }
}

public sealed class HazardExposureMatrix
{
    public List<string> Hazards { get; set; } = [];
    public List<string> Categories { get; set; } = [];
    public List<MatrixCell> Cells { get; set; } = [];
}

public sealed class ScenarioMatrix
{
    public List<string> Hazards { get; set; } = [];
    public List<string> Scenarios { get; set; } = [];
    public List<MatrixCell> Cells { get; set; } = [];
}
