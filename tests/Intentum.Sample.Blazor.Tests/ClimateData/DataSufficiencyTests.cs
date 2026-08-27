using Intentum.Sample.Blazor.Components.Services.ClimateData;

namespace Intentum.Sample.Blazor.Tests.ClimateData;

public class DataSufficiencyTests
{
    [Fact]
    public void Evaluate_AllDataPresent_ReturnsFullConfidence()
    {
        var r = DataSufficiency.Evaluate(hasProjection: true, hasWri: true, hasCompanyProfile: true);
        Assert.Equal(1.0, r.Score);
        Assert.Empty(r.Missing);
        Assert.False(r.IsRegionalEstimate);
    }

    [Fact]
    public void Evaluate_MissingProjection_ReturnsLowerConfidenceAndMarksMissing()
    {
        var r = DataSufficiency.Evaluate(hasProjection: false, hasWri: true, hasCompanyProfile: true);
        Assert.Equal(2.0 / 3.0, r.Score, 2);
        Assert.Contains("Open-Meteo projeksiyon", r.Missing);
        Assert.True(r.IsRegionalEstimate);
    }

    [Fact]
    public void Evaluate_MissingTwoSources_ReturnsLowConfidence()
    {
        var r = DataSufficiency.Evaluate(hasProjection: false, hasWri: false, hasCompanyProfile: true);
        Assert.Equal(1.0 / 3.0, r.Score, 2);
        Assert.Equal(2, r.Missing.Count);
    }

    [Fact]
    public void Evaluate_NoProfile_IndicatesMissingProfile()
    {
        var r = DataSufficiency.Evaluate(hasProjection: true, hasWri: true, hasCompanyProfile: false);
        Assert.Contains("Şirket finansal profili", r.Missing);
    }
}
