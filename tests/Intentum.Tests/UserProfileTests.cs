using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Core.UBA;

namespace Intentum.Tests;

/// <summary>
/// Tests for UserProfile: Learn, CalculateDeviation.
/// </summary>
public sealed class UserProfileTests
{
    [Fact]
    public void Constructor_WithNullUserId_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new UserProfile(null!));
    }

    [Fact]
    public void Learn_UpdatesTotalSessionsAndActionFrequency()
    {
        var profile = new UserProfile("user1");
        var space = new BehaviorSpace()
            .Observe("user", "login")
            .Observe("user", "login")
            .Observe("user", "submit");

        profile.Learn(space);

        Assert.Equal(1, profile.TotalSessions);
        Assert.Equal(2, profile.ActionFrequency["login"]);
        Assert.Equal(1, profile.ActionFrequency["submit"]);
    }

    [Fact]
    public void CalculateDeviation_WithNoLearning_ReturnsZero()
    {
        var profile = new UserProfile("user1");
        var space = new BehaviorSpace().Observe("user", "login");

        var deviation = profile.CalculateDeviation(space);

        Assert.Equal(0, deviation);
    }

    [Fact]
    public void CalculateDeviation_WithMatchingBehavior_ReturnsLowDeviation()
    {
        var profile = new UserProfile("user1");
        var learnSpace = new BehaviorSpace()
            .Observe("user", "login")
            .Observe("user", "login")
            .Observe("user", "submit");
        profile.Learn(learnSpace);

        var currentSpace = new BehaviorSpace()
            .Observe("user", "login")
            .Observe("user", "submit");

        var deviation = profile.CalculateDeviation(currentSpace);

        Assert.InRange(deviation, 0, 0.5);
    }

    [Fact]
    public void CalculateDeviation_WithUnknownActions_ReturnsHigherDeviation()
    {
        var profile = new UserProfile("user1");
        var learnSpace = new BehaviorSpace()
            .Observe("user", "login")
            .Observe("user", "submit");
        profile.Learn(learnSpace);

        var currentSpace = new BehaviorSpace()
            .Observe("user", "unknown_action")
            .Observe("user", "another_unknown");

        var deviation = profile.CalculateDeviation(currentSpace);

        Assert.True(deviation > 0.5);
    }
}
