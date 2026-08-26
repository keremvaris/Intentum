using Intentum.Sample.Blazor.Components.Services.ClimateData;
using Moq;

namespace Intentum.Sample.Blazor.Tests.ClimateData;

public class ScenarioComparisonEngineTests
{
    [Fact]
    public async Task CompareAllAsync_WithSeedProfile_ReturnsFourResults()
    {
        var profile = CompanyProfileService.CreateManufacturingAnkara();
        var input = new RiskInput
        {
            Latitude = profile.Latitude,
            Longitude = profile.Longitude,
            LocationName = profile.LocationName,
            Sector = profile.Sector,
            Horizon = 2050
        };

        var engineMock = new Mock<RiskCalculationEngine>(
            null!, null!, null!);

        engineMock
            .Setup(e => e.AssessAsync(It.IsAny<RiskInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RiskInput ri, CancellationToken _) => new RiskAssessment
            {
                Input = ri,
                PhysicalRisk = 0.4,
                TransitionRisk = 0.3,
                Decision = "ALLOW"
            });

        var comparison = new ScenarioComparisonEngine(engineMock.Object);
        var results = await comparison.CompareAllAsync(profile, input, CancellationToken.None);

        Assert.Equal(4, results.Count);
        Assert.Contains(results, r => r.Scenario == "SSP1-2.6");
        Assert.Contains(results, r => r.Scenario == "SSP5-8.5");
    }
}
