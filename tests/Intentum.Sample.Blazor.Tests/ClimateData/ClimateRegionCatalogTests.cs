using Intentum.Sample.Blazor.Components.Services.ClimateData;

namespace Intentum.Sample.Blazor.Tests.ClimateData;

public class ClimateRegionCatalogTests
{
    [Fact]
    public void Get_TurkeyIso3_ReturnsTurkeyProfile()
    {
        var profile = ClimateRegionCatalog.Get("TUR");
        Assert.Equal("Türkiye", profile.Name);
        Assert.Contains("Kuraklık Riski", profile.DominantHazards);
        Assert.True(profile.BaselineWaterStress > 3);
    }

    [Fact]
    public void Get_UnknownIso3_ReturnsDefault()
    {
        var profile = ClimateRegionCatalog.Get("ZZZ");
        Assert.Equal("UNK", profile.Code);
        Assert.NotEmpty(profile.AdaptiveRecommendations);
    }

    [Fact]
    public void DetectIso3_Izmir_ReturnsTurkey()
    {
        Assert.Equal("TUR", ClimateRegionCatalog.DetectIso3(38.42, 27.13));
    }

    [Fact]
    public void DetectIso3_Spain_ReturnsSpain()
    {
        Assert.Equal("ESP", ClimateRegionCatalog.DetectIso3(40.42, -3.70));
    }

    [Fact]
    public void FromCoordinates_ReturnsProfileForLocation()
    {
        var profile = ClimateRegionCatalog.FromCoordinates(39.93, 32.86);
        Assert.Equal("TUR", profile.Code);
    }
}
