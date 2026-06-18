using Intentum.AI.Ensemble;
using Intentum.Core.Intents;

namespace Intentum.Tests.AI;

public sealed class EnsembleTests
{
    [Fact]
    public void WeightedEnsemble_Average_ReturnsWeightedScore()
    {
        var ensemble = new WeightedEnsemble();
        var results = new[]
        {
            new ModelResult("A", 0.9, 1.0),
            new ModelResult("B", 0.5, 1.0)
        };

        var intent = ensemble.Combine(results);

        Assert.Equal(0.7, intent.Confidence.Score, 2);
    }

    [Fact]
    public void WeightedEnsemble_WithWeights_AppliesWeights()
    {
        var ensemble = new WeightedEnsemble();
        var results = new[]
        {
            new ModelResult("A", 1.0, 3.0),
            new ModelResult("B", 0.0, 1.0)
        };

        var intent = ensemble.Combine(results);

        Assert.Equal(0.75, intent.Confidence.Score, 2);
    }

    [Fact]
    public void WeightedEnsemble_TakesMajorityName()
    {
        var ensemble = new WeightedEnsemble();
        var results = new[]
        {
            new ModelResult("Purchase", 0.8, 1.0),
            new ModelResult("Purchase", 0.7, 1.0),
            new ModelResult("Browse", 0.6, 1.0)
        };

        var intent = ensemble.Combine(results);

        Assert.Equal("Purchase", intent.Name);
    }

    [Fact]
    public void MajorityVotingEnsemble_MajorityWins()
    {
        var ensemble = new MajorityVotingEnsemble();
        var results = new[]
        {
            new ModelResult("Purchase", 0.8, 1.0),
            new ModelResult("Purchase", 0.7, 1.0),
            new ModelResult("Support", 0.9, 1.0)
        };

        var intent = ensemble.Combine(results);

        Assert.Equal("Purchase", intent.Name);
    }

    [Fact]
    public void MajorityVotingEnsemble_Tie_UsesHighestConfidence()
    {
        var ensemble = new MajorityVotingEnsemble();
        var results = new[]
        {
            new ModelResult("Purchase", 0.6, 1.0),
            new ModelResult("Support", 0.9, 1.0)
        };

        var intent = ensemble.Combine(results);

        Assert.Equal("Support", intent.Name);
    }

    [Fact]
    public void WeightedEnsemble_SingleResult_ReturnsThatResult()
    {
        var ensemble = new WeightedEnsemble();
        var results = new[]
        {
            new ModelResult("Only", 0.75, 1.0)
        };

        var intent = ensemble.Combine(results);

        Assert.Equal("Only", intent.Name);
        Assert.Equal(0.75, intent.Confidence.Score);
    }

    [Fact]
    public void EnsembleIntentModel_EmptyResults_ReturnsUnknown()
    {
        var ensemble = new WeightedEnsemble();
        var results = Array.Empty<ModelResult>();

        var intent = ensemble.Combine(results);

        Assert.Equal("Unknown", intent.Name);
    }
}
