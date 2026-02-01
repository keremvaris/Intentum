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

    [Fact]
    public void FromScore_BorderlineLowMedium_AssignsCorrectLevel()
    {
        Assert.Equal("Low", IntentConfidence.FromScore(0.29).Level);
        Assert.Equal("Medium", IntentConfidence.FromScore(0.3).Level);
    }

    [Fact]
    public void FromScore_BorderlineMediumHigh_AssignsCorrectLevel()
    {
        Assert.Equal("Medium", IntentConfidence.FromScore(0.59).Level);
        Assert.Equal("High", IntentConfidence.FromScore(0.6).Level);
    }

    [Fact]
    public void FromScore_BorderlineHighCertain_AssignsCorrectLevel()
    {
        Assert.Equal("High", IntentConfidence.FromScore(0.84).Level);
        Assert.Equal("Certain", IntentConfidence.FromScore(0.85).Level);
    }

    [Fact]
    public void FromScore_EdgeZeroAndOne_AssignsCorrectLevels()
    {
        var atZero = IntentConfidence.FromScore(0);
        Assert.Equal(0, atZero.Score);
        Assert.Equal("Low", atZero.Level);

        var atOne = IntentConfidence.FromScore(1.0);
        Assert.Equal(1.0, atOne.Score);
        Assert.Equal("Certain", atOne.Level);
    }

    [Fact]
    public void FromScore_Deterministic_SameScoreAlwaysSameLevel()
    {
        for (var i = 0; i < 10; i++)
        {
            Assert.Equal("Low", IntentConfidence.FromScore(0.1).Level);
            Assert.Equal("Medium", IntentConfidence.FromScore(0.5).Level);
            Assert.Equal("High", IntentConfidence.FromScore(0.7).Level);
            Assert.Equal("Certain", IntentConfidence.FromScore(0.99).Level);
        }
    }

    [Fact]
    public void FromScore_ScorePreserved_LevelDerivedFromScore()
    {
        var c = IntentConfidence.FromScore(0.42);
        Assert.Equal(0.42, c.Score);
        Assert.Equal("Medium", c.Level);
    }
}
