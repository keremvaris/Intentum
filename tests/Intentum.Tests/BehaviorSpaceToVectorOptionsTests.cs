using Intentum.Core;
using Intentum.Core.Behavior;

namespace Intentum.Tests;

/// <summary>
/// Tests for BehaviorSpace.ToVector with ToVectorOptions (normalization).
/// </summary>
public class BehaviorSpaceToVectorOptionsTests
{
    [Fact]
    public void ToVector_WithNullOptions_ReturnsRawCounts()
    {
        var space = new BehaviorSpace()
            .Observe("user", "login.failed")
            .Observe("user", "login.failed")
            .Observe("user", "login.failed")
            .Observe("user", "password.reset");

        var vector = space.ToVector(null);

        Assert.Equal(3, vector.Dimensions["user:login.failed"]);
        Assert.Equal(1, vector.Dimensions["user:password.reset"]);
    }

    [Fact]
    public void ToVector_WithCap_CapsPerDimension()
    {
        var space = new BehaviorSpace();
        for (int i = 0; i < 5; i++)
            space.Observe("user", "login.failed");
        space.Observe("user", "password.reset");

        var options = new ToVectorOptions(VectorNormalization.Cap, CapPerDimension: 3);
        var vector = space.ToVector(options);

        Assert.Equal(3, vector.Dimensions["user:login.failed"]);
        Assert.Equal(1, vector.Dimensions["user:password.reset"]);
    }

    [Fact]
    public void ToVector_WithL1_SumEqualsOne()
    {
        var space = new BehaviorSpace()
            .Observe("user", "a")
            .Observe("user", "a")
            .Observe("user", "b");

        var options = new ToVectorOptions(VectorNormalization.L1);
        var vector = space.ToVector(options);

        var sum = vector.Dimensions.Values.Sum();
        Assert.Equal(1.0, sum, 6);
        Assert.Equal(2.0 / 3.0, vector.Dimensions["user:a"], 6);
        Assert.Equal(1.0 / 3.0, vector.Dimensions["user:b"], 6);
    }

    [Fact]
    public void ToVector_WithSoftCap_ScalesByCap()
    {
        var space = new BehaviorSpace();
        for (int i = 0; i < 6; i++)
            space.Observe("user", "x");
        space.Observe("user", "y");

        var options = new ToVectorOptions(VectorNormalization.SoftCap, CapPerDimension: 3);
        var vector = space.ToVector(options);

        Assert.Equal(1.0, vector.Dimensions["user:x"]); // min(1, 6/3)
        Assert.Equal(1.0 / 3.0, vector.Dimensions["user:y"], 6);
    }

    [Fact]
    public void ToVector_WithTimeWindowAndOptions_AppliesNormalizationToWindowOnly()
    {
        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("user", "a", DateTimeOffset.UtcNow.AddHours(-2)));
        space.Observe(new BehaviorEvent("user", "a", DateTimeOffset.UtcNow.AddHours(-1)));
        space.Observe(new BehaviorEvent("user", "b", DateTimeOffset.UtcNow));

        var start = DateTimeOffset.UtcNow.AddHours(-1.5);
        var end = DateTimeOffset.UtcNow;
        var options = new ToVectorOptions(VectorNormalization.L1);
        var vector = space.ToVector(start, end, options);

        var sum = vector.Dimensions.Values.Sum();
        Assert.Equal(1.0, sum, 6);
    }
}
