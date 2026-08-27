namespace Intentum.Sample.Blazor.Components.Services.ClimateData;

public enum FinancialCategoryType
{
    Opex,
    Capex,
    Revenue,
    CashFlow
}

public sealed class CompanyProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string Name { get; set; } = "";
    public string Sector { get; set; } = "Sanayi";
    public string LocationName { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public List<FinancialCategory> Categories { get; set; } = [];

    public double TotalRevenue => Categories
        .Where(c => c.Type == FinancialCategoryType.Revenue)
        .Sum(c => c.Total);

    public double TotalOpex => Categories
        .Where(c => c.Type == FinancialCategoryType.Opex)
        .Sum(c => c.Total);

    public double TotalCapex => Categories
        .Where(c => c.Type == FinancialCategoryType.Capex)
        .Sum(c => c.Total);

    public double TotalCashFlow => Categories
        .Where(c => c.Type == FinancialCategoryType.CashFlow)
        .Sum(c => c.Total);
}

public sealed class FinancialCategory
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public FinancialCategoryType Type { get; set; }
    public string Name { get; set; } = "";
    public List<FinancialLineItem> LineItems { get; set; } = [];
    public double Total => LineItems.Sum(i => i.Value);
}

public sealed class FinancialLineItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string Name { get; set; } = "";
    public double Value { get; set; }
    public double PhysicalSensitivity { get; set; }
    public double TransitionSensitivity { get; set; }
    // IPCC risk çerçevesi: Hassasiyet (Sensitivity) ve Uyum Kapasitesi (Adaptive Capacity) 0-1 arası.
    // Kırılganlık = f(Sensitivity / AdaptiveCapacity). AdaptiveCapacity 0 ise kırılganlık maksimum olur.
    public double Sensitivity { get; set; }
    public double AdaptiveCapacity { get; set; } = 1.0;
    public List<string> MappedRiskSignals { get; set; } = [];
}

public sealed class FinancialImpact
{
    public List<LineItemImpact> LineItemImpacts { get; set; } = [];
    public List<CategoryImpact> CategoryImpacts { get; set; } = [];

    public double TotalRevenueImpact =>
        CategoryImpacts.Where(c => c.Type == FinancialCategoryType.Revenue).Sum(c => c.TotalImpact);

    public double TotalOpexImpact =>
        CategoryImpacts.Where(c => c.Type == FinancialCategoryType.Opex).Sum(c => c.TotalImpact);

    public double TotalCapexImpact =>
        CategoryImpacts.Where(c => c.Type == FinancialCategoryType.Capex).Sum(c => c.TotalImpact);

    public double TotalCashFlowImpact =>
        CategoryImpacts.Where(c => c.Type == FinancialCategoryType.CashFlow).Sum(c => c.TotalImpact);

    public double NetCashFlowImpact =>
        TotalRevenueImpact - TotalOpexImpact - TotalCapexImpact + TotalCashFlowImpact;
}

public sealed class CategoryImpact
{
    public string CategoryId { get; set; } = "";
    public string Name { get; set; } = "";
    public FinancialCategoryType Type { get; set; }
    public double PhysicalImpact { get; set; }
    public double TransitionImpact { get; set; }
    public double TotalImpact => PhysicalImpact + TransitionImpact;
}

public sealed class LineItemImpact
{
    public string CategoryId { get; set; } = "";
    public string LineItemId { get; set; } = "";
    public string Name { get; set; } = "";
    public double PhysicalImpact { get; set; }
    public double TransitionImpact { get; set; }
    public double TotalImpact => PhysicalImpact + TransitionImpact;
}

public sealed class ScenarioComparisonResult
{
    public string Scenario { get; set; } = "";
    public RiskAssessment Assessment { get; set; } = new();
    public FinancialImpact Impact { get; set; } = new();
}

public static class CompanyProfileExtensions
{
    public static CompanyProfile DeepClone(this CompanyProfile source)
    {
        return new CompanyProfile
        {
            Id = source.Id,
            Name = source.Name,
            Sector = source.Sector,
            LocationName = source.LocationName,
            Latitude = source.Latitude,
            Longitude = source.Longitude,
            Categories = source.Categories.Select(c => new FinancialCategory
            {
                Id = c.Id,
                Type = c.Type,
                Name = c.Name,
                LineItems = c.LineItems.Select(li => new FinancialLineItem
                {
                    Id = li.Id,
                    Name = li.Name,
                    Value = li.Value,
                    PhysicalSensitivity = li.PhysicalSensitivity,
                    TransitionSensitivity = li.TransitionSensitivity,
                    Sensitivity = li.Sensitivity,
                    AdaptiveCapacity = li.AdaptiveCapacity,
                    MappedRiskSignals = new List<string>(li.MappedRiskSignals)
                }).ToList()
            }).ToList()
        };
    }
}
