using Intentum.Simulation;

namespace Intentum.Tests;

public class BehaviorSpaceSimulatorTests
{
    private static readonly string[] DefaultActors = ["user", "system"];
    private static readonly string[] DefaultActions = ["a", "b"];
    private static readonly string[] SingleActor = ["user"];
    private static readonly string[] SingleAction = ["a"];

    [Fact]
    public void FromSequence_CreatesSpaceWithEvents()
    {
        var simulator = new BehaviorSpaceSimulator();
        var sequence = new List<(string, string)> { ("user", "login"), ("user", "submit") };
        var space = simulator.FromSequence(sequence);
        Assert.Equal(2, space.Events.Count);
        Assert.Equal("user", space.Events.ElementAt(0).Actor);
        Assert.Equal("login", space.Events.ElementAt(0).Action);
        Assert.Equal("submit", space.Events.ElementAt(1).Action);
    }

    [Fact]
    public void GenerateRandom_WithSeed_Reproducible()
    {
        var simulator = new BehaviorSpaceSimulator();
        var space1 = simulator.GenerateRandom(DefaultActors, DefaultActions, eventCount: 5, randomSeed: 42);
        var space2 = simulator.GenerateRandom(DefaultActors, DefaultActions, eventCount: 5, randomSeed: 42);
        Assert.Equal(5, space1.Events.Count);
        Assert.Equal(5, space2.Events.Count);
        var keys1 = space1.Events.Select(e => $"{e.Actor}:{e.Action}").ToList();
        var keys2 = space2.Events.Select(e => $"{e.Actor}:{e.Action}").ToList();
        Assert.Equal(keys1, keys2);
    }

    [Fact]
    public void GenerateRandom_ZeroEvents_ReturnsEmptySpace()
    {
        var simulator = new BehaviorSpaceSimulator();
        var space = simulator.GenerateRandom(SingleActor, SingleAction, 0);
        Assert.Empty(space.Events);
    }
}
