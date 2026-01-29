using Intentum.Core;
using Intentum.Core.Behavior;

namespace Intentum.Tests;

public sealed class IntentumCoreExtensionsTests
{
    [Fact]
    public void BehaviorSpace_Observe_AddsEventAndReturnsSpace()
    {
        var space = new BehaviorSpace();
        var returned = space.Observe("user", "login");

        Assert.Same(space, returned);
        Assert.Single(space.Events);
        Assert.Equal("user", space.Events.First().Actor);
        Assert.Equal("login", space.Events.First().Action);
    }

    [Fact]
    public void BehaviorSpace_Observe_Chained_AddsMultipleEvents()
    {
        var space = new BehaviorSpace();
        space.Observe("user", "login").Observe("user", "submit");

        Assert.Equal(2, space.Events.Count);
        Assert.Equal("login", space.Events.First().Action);
        Assert.Equal("submit", space.Events.Skip(1).First().Action);
    }

    [Fact]
    public void BehaviorSpace_EvaluateIntent_ReturnsIntentEvaluationResult()
    {
        var space = new BehaviorSpace();
        space.Observe("user", "login").Observe("user", "login");

        var result = space.EvaluateIntent("login_intent");

        Assert.NotNull(result);
        Assert.NotNull(result.Intent);
        Assert.Equal("login_intent", result.Intent.Name);
        Assert.NotNull(result.BehaviorVector);
    }

    [Fact]
    public void BehaviorSpace_EvaluateIntent_WithManyEvents_NormalizesWeight()
    {
        var space = new BehaviorSpace();
        for (var i = 0; i < 15; i++)
            space.Observe("user", "repeat");
        var result = space.EvaluateIntent("repeat_intent");
        Assert.NotNull(result);
        Assert.Single(result.Intent.Signals);
        Assert.InRange(result.Intent.Signals.First().Weight, 0.9, 1.0);
    }
}
