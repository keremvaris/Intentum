namespace Intentum.Sample.Blazor.Components.Services.ClimateData;

public sealed class CompanyProfileService
{
    private readonly List<CompanyProfile> _profiles = [];

    public CompanyProfileService()
    {
        _profiles.Add(CreateManufacturingAnkara());
        _profiles.Add(CreateEnergyIzmir());
        _profiles.Add(CreateTourismAntalya());
    }

    public IReadOnlyList<CompanyProfile> GetAll() => _profiles.ToList();

    public CompanyProfile? GetById(string id) =>
        _profiles.FirstOrDefault(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    public CompanyProfile Clone(CompanyProfile source)
    {
        var clone = new CompanyProfile
        {
            Name = source.Name,
            Sector = source.Sector,
            LocationName = source.LocationName,
            Latitude = source.Latitude,
            Longitude = source.Longitude,
            Categories = source.Categories.Select(c => new FinancialCategory
            {
                Type = c.Type,
                Name = c.Name,
                LineItems = c.LineItems.Select(i => new FinancialLineItem
                {
                    Name = i.Name,
                    Value = i.Value,
                    PhysicalSensitivity = i.PhysicalSensitivity,
                    TransitionSensitivity = i.TransitionSensitivity,
                    Sensitivity = i.Sensitivity,
                    AdaptiveCapacity = i.AdaptiveCapacity,
                    MappedRiskSignals = [.. i.MappedRiskSignals]
                }).ToList()
            }).ToList()
        };
        return clone;
    }

    public void Add(CompanyProfile profile) => _profiles.Add(profile);

    public bool Update(CompanyProfile profile)
    {
        var idx = _profiles.FindIndex(p => p.Id.Equals(profile.Id, StringComparison.OrdinalIgnoreCase));
        if (idx < 0) return false;
        _profiles[idx] = profile;
        return true;
    }

    public bool Delete(string id)
    {
        var idx = _profiles.FindIndex(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (idx < 0) return false;
        _profiles.RemoveAt(idx);
        return true;
    }

    public static CompanyProfile CreateManufacturingAnkara() => new()
    {
        Id = "manf-ank",
        Name = "Demo Sanayi A.S.",
        Sector = "Sanayi",
        LocationName = "Ankara",
        Latitude = 39.93,
        Longitude = 32.86,
        Categories =
        [
            new FinancialCategory
            {
                Type = FinancialCategoryType.Revenue,
                Name = "Ciro",
                LineItems =
                [
                    new FinancialLineItem { Name = "Urun A", Value = 85_000_000, Sensitivity = 0.5, AdaptiveCapacity = 0.4 },
                    new FinancialLineItem { Name = "Urun B", Value = 45_000_000, Sensitivity = 0.3, AdaptiveCapacity = 0.5 }
                ]
            },
            new FinancialCategory
            {
                Type = FinancialCategoryType.Opex,
                Name = "OPEX",
                LineItems =
                [
                    new FinancialLineItem { Name = "Enerji", Value = 18_000_000, TransitionSensitivity = 0.8, Sensitivity = 0.7, AdaptiveCapacity = 0.3, MappedRiskSignals = ["carbon_price"] },
                    new FinancialLineItem { Name = "Su", Value = 6_000_000, PhysicalSensitivity = 0.9, Sensitivity = 0.8, AdaptiveCapacity = 0.2, MappedRiskSignals = ["drought", "water_stress"] },
                    new FinancialLineItem { Name = "Bakim", Value = 12_000_000, PhysicalSensitivity = 0.3, Sensitivity = 0.4, AdaptiveCapacity = 0.6 }
                ]
            },
            new FinancialCategory
            {
                Type = FinancialCategoryType.Capex,
                Name = "CAPEX",
                LineItems =
                [
                    new FinancialLineItem { Name = "Iklim Dayanikliligi", Value = 8_000_000, PhysicalSensitivity = 0.7, Sensitivity = 0.5, AdaptiveCapacity = 0.5, MappedRiskSignals = ["heatwave", "flood"] },
                    new FinancialLineItem { Name = "Yeni Tesis", Value = 22_000_000, PhysicalSensitivity = 0.2, Sensitivity = 0.3, AdaptiveCapacity = 0.7 }
                ]
            }
        ]
    };

    public static CompanyProfile CreateEnergyIzmir() => new()
    {
        Id = "ener-izm",
        Name = "Ege Enerji A.S.",
        Sector = "Enerji",
        LocationName = "Izmir",
        Latitude = 38.42,
        Longitude = 27.13,
        Categories =
        [
            new FinancialCategory
            {
                Type = FinancialCategoryType.Revenue,
                Name = "Ciro",
                LineItems =
                [
                    new FinancialLineItem { Name = "Elektrik Satisi", Value = 120_000_000, Sensitivity = 0.6, AdaptiveCapacity = 0.4 },
                    new FinancialLineItem { Name = "Kapasite Odemeleri", Value = 45_000_000, Sensitivity = 0.2, AdaptiveCapacity = 0.6 }
                ]
            },
            new FinancialCategory
            {
                Type = FinancialCategoryType.Opex,
                Name = "OPEX",
                LineItems =
                [
                    new FinancialLineItem { Name = "Yakit", Value = 35_000_000, TransitionSensitivity = 0.95, Sensitivity = 0.9, AdaptiveCapacity = 0.2, MappedRiskSignals = ["carbon_price"] },
                    new FinancialLineItem { Name = "Bakim", Value = 18_000_000, PhysicalSensitivity = 0.7, Sensitivity = 0.6, AdaptiveCapacity = 0.4, MappedRiskSignals = ["storm", "flood"] },
                    new FinancialLineItem { Name = "Karbon Maliyeti", Value = 9_000_000, TransitionSensitivity = 1.0, Sensitivity = 0.8, AdaptiveCapacity = 0.3, MappedRiskSignals = ["carbon_price"] }
                ]
            },
            new FinancialCategory
            {
                Type = FinancialCategoryType.Capex,
                Name = "CAPEX",
                LineItems =
                [
                    new FinancialLineItem { Name = "Grid Altyapisi", Value = 28_000_000, PhysicalSensitivity = 0.6, Sensitivity = 0.6, AdaptiveCapacity = 0.4, MappedRiskSignals = ["storm"] },
                    new FinancialLineItem { Name = "Yenilenebilir", Value = 18_000_000, TransitionSensitivity = 0.5, Sensitivity = 0.4, AdaptiveCapacity = 0.6 }
                ]
            }
        ]
    };

    public static CompanyProfile CreateTourismAntalya() => new()
    {
        Id = "tour-ant",
        Name = "Akdeniz Turizm A.S.",
        Sector = "Turizm",
        LocationName = "Antalya",
        Latitude = 36.90,
        Longitude = 30.71,
        Categories =
        [
            new FinancialCategory
            {
                Type = FinancialCategoryType.Revenue,
                Name = "Ciro",
                LineItems =
                [
                    new FinancialLineItem { Name = "Yaz Sezonu", Value = 55_000_000, PhysicalSensitivity = 0.8, Sensitivity = 0.8, AdaptiveCapacity = 0.3, MappedRiskSignals = ["heatwave"] },
                    new FinancialLineItem { Name = "Kis Sezonu", Value = 20_000_000, Sensitivity = 0.3, AdaptiveCapacity = 0.6 }
                ]
            },
            new FinancialCategory
            {
                Type = FinancialCategoryType.Opex,
                Name = "OPEX",
                LineItems =
                [
                    new FinancialLineItem { Name = "Enerji (Sogutma)", Value = 12_000_000, PhysicalSensitivity = 0.9, Sensitivity = 0.9, AdaptiveCapacity = 0.2, MappedRiskSignals = ["heatwave"] },
                    new FinancialLineItem { Name = "Su", Value = 6_000_000, PhysicalSensitivity = 0.7, Sensitivity = 0.7, AdaptiveCapacity = 0.3, MappedRiskSignals = ["drought"] },
                    new FinancialLineItem { Name = "Personel", Value = 9_000_000, Sensitivity = 0.4, AdaptiveCapacity = 0.6 }
                ]
            },
            new FinancialCategory
            {
                Type = FinancialCategoryType.Capex,
                Name = "CAPEX",
                LineItems =
                [
                    new FinancialLineItem { Name = "Tesis Modernizasyonu", Value = 8_000_000, PhysicalSensitivity = 0.4, Sensitivity = 0.5, AdaptiveCapacity = 0.5 },
                    new FinancialLineItem { Name = "Izolasyon", Value = 5_000_000, PhysicalSensitivity = 0.6, Sensitivity = 0.6, AdaptiveCapacity = 0.4, MappedRiskSignals = ["heatwave"] }
                ]
            }
        ]
    };
}
