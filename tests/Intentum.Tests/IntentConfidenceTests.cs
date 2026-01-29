using Intentum.Core.Intents;

namespace Intentum.Tests;

public sealed class IntentConfidenceTests
{
    [Fact]
    public void FromScore_MapsToLevels()
    {
        Assert.Equal("Low", IntentConfidence.FromScore(0.2).Level);
        Assert.Equal("Medium", IntentConfidence.FromScore(0.4).Level);
        Assert.Equal("High", IntentConfidence.FromScore(0.7).Level);
        Assert.Equal("Certain", IntentConfidence.FromScore(0.9).Level);
    }
}
