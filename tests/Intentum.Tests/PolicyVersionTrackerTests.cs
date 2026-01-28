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
}
