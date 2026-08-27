using Intentum.Sample.Blazor.Components.Services.ClimateData;

namespace Intentum.Sample.Blazor.Tests.ClimateData;

public class CompanyProfileImporterTests
{
    private const string SampleCsv = """
        SHIRKET,Ornek Gida A.S.,Tarim,Konya,37.87,32.48
        Kategori,KategoriAdi,KalemAdi,Value,PhysSens,TransSens,Sensitivity,AdaptiveCapacity,Signals
        Revenue,Ciro,Bugday Satisi,60000000,0.3,0.2,0.5,0.4,
        Revenue,Ciro,Hayvancilik,25000000,0.2,0.1,0.3,0.6,
        Opex,OPEX,Sulama Enerjisi,8000000,0.7,0.3,0.7,0.3,drought
        Capex,CAPEX,Sulama Altyapisi,7000000,0.6,0.2,0.5,0.4,drought,water_stress
        """;

    [Fact]
    public void Parse_MinimalCsv_ReturnsCompanyProfile()
    {
        var result = CompanyProfileImporter.Parse(SampleCsv);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Profile);
        Assert.Equal("Ornek Gida A.S.", result.Profile!.Name);
        Assert.Equal("Tarim", result.Profile.Sector);
        Assert.Equal("Konya", result.Profile.LocationName);
        Assert.Equal(37.87, result.Profile.Latitude);
        Assert.Equal(32.48, result.Profile.Longitude);
    }

    [Fact]
    public void Parse_ReadsCategoriesAndLineItems()
    {
        var result = CompanyProfileImporter.Parse(SampleCsv);

        var revenue = result.Profile!.Categories.FirstOrDefault(c => c.Type == FinancialCategoryType.Revenue);
        Assert.NotNull(revenue);
        Assert.Equal("Ciro", revenue!.Name);
        Assert.Equal(2, revenue.LineItems.Count);

        var opex = result.Profile.Categories.FirstOrDefault(c => c.Type == FinancialCategoryType.Opex);
        Assert.NotNull(opex);
        Assert.Single(opex!.LineItems);
    }

    [Fact]
    public void Parse_ReadsValuesAndSensitivities()
    {
        var result = CompanyProfileImporter.Parse(SampleCsv);

        var item = result.Profile!.Categories
            .SelectMany(c => c.LineItems)
            .First(i => i.Name == "Sulama Enerjisi");

        Assert.Equal(8_000_000, item.Value);
        Assert.Equal(0.7, item.PhysicalSensitivity);
        Assert.Equal(0.3, item.TransitionSensitivity);
        Assert.Equal(0.7, item.Sensitivity);
        Assert.Equal(0.3, item.AdaptiveCapacity);
        Assert.Contains("drought", item.MappedRiskSignals);
    }

    [Fact]
    public void Parse_MissingShirket_ReturnsError()
    {
        var result = CompanyProfileImporter.Parse("Kategori,KategoriAdi,KalemAdi,Value\nRevenue,Ciro,X,1,0,0,0,0,");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Parse_EmptyInput_ReturnsError()
    {
        var result = CompanyProfileImporter.Parse("");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Parse_MultipleCompanies_ReturnsAll()
    {
        var csv = SampleCsv + "\n" + """
        SHIRKET,Ikinci Sirket A.S.,Sanayi,Ankara,39.93,32.86
        Kategori,KategoriAdi,KalemAdi,Value,PhysSens,TransSens,Sensitivity,AdaptiveCapacity,Signals
        Opex,OPEX,Enerji,2000000,0.8,0.5,0.7,0.2,carbon_price
        """;

        var result = CompanyProfileImporter.Parse(csv);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Profiles);
        Assert.Equal(2, result.Profiles!.Count);
        Assert.Equal("Ikinci Sirket A.S.", result.Profiles[1].Name);
    }
}
