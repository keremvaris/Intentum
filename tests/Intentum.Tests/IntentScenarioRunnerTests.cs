using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Runtime.Policy;
using Intentum.Simulation;

namespace Intentum.Tests;

public sealed class IntentScenarioRunnerTests
{
    [Fact]
    public async Task RunAsync_WithSequenceScenario_ReturnsOneResultPerScenario()
    {
        var simulator = new BehaviorSpaceSimulator();
        var runner = new IntentScenarioRunner(simulator);
        var model = new LlmIntentModel(new MockEmbeddingProvider(), new SimpleAverageSimilarityEngine());
        var policy = new IntentPolicyBuilder().Allow("A", _ => true).Build();
        var scenarios = new List<BehaviorScenario>
        {
            new("s1", "Login", Sequence: new List<(string, string)> { ("user", "login") }),
            new("s2", "Checkout", Sequence: new List<(string, string)> { ("customer", "checkout") })
        };

        var results = await runner.RunAsync(scenarios, model, policy);

        Assert.Equal(2, results.Count);
        Assert.Equal("s1", results[0].ScenarioId);
        Assert.Equal("s2", results[1].ScenarioId);
        Assert.True(results[0].DurationMs >= 0);
        Assert.Equal(PolicyDecision.Allow, results[0].Decision);
    }

    [Fact]
    public async Task RunAsync_WithRandomScenario_ReturnsResult()
    {
        var simulator = new BehaviorSpaceSimulator();
        var runner = new IntentScenarioRunner(simulator);
        var model = new LlmIntentModel(new MockEmbeddingProvider(), new SimpleAverageSimilarityEngine());
        var policy = new IntentPolicyBuilder().Allow("A", _ => true).Build();
        var scenarios = new List<BehaviorScenario>
        {
            new("r1", "Random", Actors: ["user"], Actions: ["login"], EventCount: 3, RandomSeed: 42)
        };

        var results = await runner.RunAsync(scenarios, model, policy);

        Assert.Single(results);
        Assert.Equal("r1", results[0].ScenarioId);
        Assert.NotNull(results[0].Intent);
    }

    [Fact]
    public async Task RunAsync_WithNullScenarios_ReturnsEmptyList()
    {
        var simulator = new BehaviorSpaceSimulator();
        var runner = new IntentScenarioRunner(simulator);
        var model = new LlmIntentModel(new MockEmbeddingProvider(), new SimpleAverageSimilarityEngine());
        var policy = new IntentPolicyBuilder().Build();

        var results = await runner.RunAsync(null, model, policy);

        Assert.Empty(results);
    }

    [Fact]
    public async Task RunAsync_WithInvalidScenario_NoSequenceNoActors_ThrowsArgumentException()
    {
        var simulator = new BehaviorSpaceSimulator();
        var runner = new IntentScenarioRunner(simulator);
        var model = new LlmIntentModel(new MockEmbeddingProvider(), new SimpleAverageSimilarityEngine());
        var policy = new IntentPolicyBuilder().Build();
        var scenarios = new List<BehaviorScenario>
        {
            new("bad", "Bad", Sequence: null, Actors: null, Actions: null) // no Sequence, no Actors+Actions
        };

        await Assert.ThrowsAsync<ArgumentException>(() => runner.RunAsync(scenarios, model, policy));
    }
}
