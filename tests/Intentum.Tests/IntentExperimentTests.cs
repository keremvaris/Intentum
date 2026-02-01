using Intentum.Experiments;
using Intentum.Testing;

namespace Intentum.Tests;

public class IntentExperimentTests
{
    [Fact]
    public async Task RunAsync_TwoVariants_SplitsResults()
    {
        var model = TestHelpers.CreateDefaultModel();
        var policy = TestHelpers.CreateDefaultPolicy();
        var space = TestHelpers.CreateSimpleSpace();
        var experiment = new IntentExperiment()
            .AddVariant("control", model, policy)
            .AddVariant("test", model, policy)
            .SplitTraffic(50, 50);
        var spaces = new[] { space, TestHelpers.CreateSpaceWithRetries(1) };
        var results = await experiment.RunAsync(spaces);
        Assert.Equal(2, results.Count);
        Assert.True(results.All(r => r.VariantName is "control" or "test"));
    }

    [Fact]
    public async Task Run_Sync_ReturnsSameAsRunAsync()
    {
        var model = TestHelpers.CreateDefaultModel();
        var policy = TestHelpers.CreateDefaultPolicy();
        var space = TestHelpers.CreateSimpleSpace();
        var experiment = new IntentExperiment().AddVariant("a", model, policy);
        var runSync = experiment.Run([space]);
        var runAsync = await experiment.RunAsync([space]);
        Assert.Single(runSync);
        Assert.Single(runAsync);
        Assert.Equal(runSync[0].Decision, runAsync[0].Decision);
    }
}
