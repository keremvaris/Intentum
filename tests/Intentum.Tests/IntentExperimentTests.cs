using Intentum.Experiments;
using Intentum.Runtime.Policy;
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

    [Fact]
    public void AddVariant_WithNullModel_ThrowsArgumentNullException()
    {
        var policy = TestHelpers.CreateDefaultPolicy();
        var experiment = new IntentExperiment();

        Assert.Throws<ArgumentNullException>(() =>
            experiment.AddVariant("a", null!, policy));
    }

    [Fact]
    public async Task RunAsync_WithNoVariants_ThrowsInvalidOperationException()
    {
        var experiment = new IntentExperiment();
        var space = TestHelpers.CreateSimpleSpace();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            experiment.RunAsync([space]));
    }

    [Fact]
    public void ComputeSignificance_WithTwoVariants_ReturnsSignificance()
    {
        var model = TestHelpers.CreateDefaultModel();
        var policyAllow = new IntentPolicyBuilder().Allow("A", _ => true).Build();
        var policyBlock = new IntentPolicyBuilder().Block("B", _ => true).Build();
        var experiment = new IntentExperiment()
            .AddVariant("control", model, policyAllow)
            .AddVariant("test", model, policyBlock)
            .SplitTraffic(50, 50);
        var spaces = Enumerable.Range(0, 20)
            .Select(_ => TestHelpers.CreateSimpleSpace())
            .ToList();
        var results = experiment.Run(spaces);

        var sig = IntentExperiment.ComputeSignificance(results, "control", "test");

        Assert.NotNull(sig);
        Assert.InRange(sig.PValue, 0, 1);
    }

    [Fact]
    public void ComputeSignificance_WithEmptyGroup_ReturnsPValueOne()
    {
        var results = new List<ExperimentResult>();
        if (results == null) throw new ArgumentNullException(nameof(results));

        var sig = IntentExperiment.ComputeSignificance(results, "a", "b");

        Assert.Equal(1.0, sig.PValue);
        Assert.False(sig.IsSignificant);
    }
}
