using Intentum.Runtime.Policy;
using Intentum.Versioning;

namespace Intentum.Tests;

public class PolicyVersionTrackerTests
{
    [Fact]
    public void Add_And_Current_ReturnsLatest()
    {
        var tracker = new PolicyVersionTracker();
        var policy1 = new IntentPolicy().AddRule(new PolicyRule("r1", _ => true, PolicyDecision.Allow));
        var policy2 = new IntentPolicy().AddRule(new PolicyRule("r2", _ => true, PolicyDecision.Block));
        tracker.Add(new VersionedPolicy("1.0", policy1));
        tracker.Add(new VersionedPolicy("2.0", policy2));
        Assert.NotNull(tracker.Current);
        Assert.Equal("2.0", tracker.Current!.Version);
        Assert.Equal(2, tracker.Versions.Count);
    }

    [Fact]
    public void Rollback_MovesToPrevious()
    {
        var tracker = new PolicyVersionTracker();
        var p1 = new VersionedPolicy("1.0", new IntentPolicy());
        var p2 = new VersionedPolicy("2.0", new IntentPolicy());
        tracker.Add(p1).Add(p2);
        Assert.Equal("2.0", tracker.Current!.Version);
        var rolled = tracker.Rollback();
        Assert.True(rolled);
        Assert.Equal("1.0", tracker.Current!.Version);
    }

    [Fact]
    public void Rollforward_AfterRollback_MovesToNext()
    {
        var tracker = new PolicyVersionTracker();
        tracker.Add(new VersionedPolicy("1.0", new IntentPolicy()));
        tracker.Add(new VersionedPolicy("2.0", new IntentPolicy()));
        tracker.Rollback();
        var ok = tracker.Rollforward();
        Assert.True(ok);
        Assert.Equal("2.0", tracker.Current!.Version);
    }

    [Fact]
    public void Add_WhenNull_Throws()
    {
        var tracker = new PolicyVersionTracker();
        Assert.Throws<ArgumentNullException>(() => tracker.Add(null!));
    }

    [Fact]
    public void Current_WhenEmpty_IsNull()
    {
        var tracker = new PolicyVersionTracker();
        Assert.Null(tracker.Current);
    }

    [Fact]
    public void Rollback_WhenAtStart_ReturnsFalse()
    {
        var tracker = new PolicyVersionTracker();
        tracker.Add(new VersionedPolicy("1.0", new IntentPolicy()));
        Assert.False(tracker.Rollback());
        Assert.Equal("1.0", tracker.Current!.Version);
    }

    [Fact]
    public void Rollforward_WhenAtEnd_ReturnsFalse()
    {
        var tracker = new PolicyVersionTracker();
        tracker.Add(new VersionedPolicy("1.0", new IntentPolicy()));
        Assert.False(tracker.Rollforward());
    }

    [Fact]
    public void SetCurrent_ValidIndex_SetsCurrent()
    {
        var tracker = new PolicyVersionTracker();
        tracker.Add(new VersionedPolicy("1.0", new IntentPolicy()));
        tracker.Add(new VersionedPolicy("2.0", new IntentPolicy()));
        tracker.SetCurrent(0);
        Assert.Equal("1.0", tracker.Current!.Version);
    }

    [Fact]
    public void SetCurrent_OutOfRange_Throws()
    {
        var tracker = new PolicyVersionTracker();
        tracker.Add(new VersionedPolicy("1.0", new IntentPolicy()));
        Assert.Throws<ArgumentOutOfRangeException>(() => tracker.SetCurrent(1));
        Assert.Throws<ArgumentOutOfRangeException>(() => tracker.SetCurrent(-1));
    }

    [Fact]
    public void VersionedPolicy_IVersionedPolicy_Policy_ReturnsPolicy()
    {
        var policy = new IntentPolicy().AddRule(new PolicyRule("r1", _ => true, PolicyDecision.Allow));
        IVersionedPolicy versioned = new VersionedPolicy("1.0", policy);
        Assert.Same(policy, versioned.Policy);
    }

    [Fact]
    public void CompareVersions_ReturnsComparison()
    {
        Assert.True(PolicyVersionTracker.CompareVersions("a", "b") < 0);
        Assert.True(PolicyVersionTracker.CompareVersions("b", "a") > 0);
        Assert.Equal(0, PolicyVersionTracker.CompareVersions("x", "x"));
    }
}
