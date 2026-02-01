using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;
using Intentum.Core.Models;

namespace Intentum.Tests;

/// <summary>
/// Tests for SlidingWindowIntentModel (configurable window, deterministic for testing).
/// </summary>
public sealed class SlidingWindowIntentModelTests
{
    [Fact]
    public void SlidingWindow_WithReferenceTime_IsDeterministic()
    {
        var refTime = new DateTimeOffset(2025, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var window = TimeSpan.FromMinutes(10);
        var inner = new StubIntentModel(space => new Intent(
            "Stub",
            [],
            new IntentConfidence(0.8, "High"),
            $"events={space.Events.Count}"));

        var model = new SlidingWindowIntentModel(inner, window, refTime);

        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("user", "click", refTime.AddMinutes(-5)));
        space.Observe(new BehaviorEvent("user", "submit", refTime.AddMinutes(-2)));

        var intent1 = model.Infer(space);
        var intent2 = model.Infer(space);

        Assert.Equal("Stub", intent1.Name);
        Assert.Equal("events=2", intent1.Reasoning);
        Assert.Equal(intent1.Name, intent2.Name);
        Assert.Equal(intent1.Reasoning, intent2.Reasoning);
    }

    [Fact]
    public void SlidingWindow_ExcludesEventsOutsideWindow()
    {
        var refTime = new DateTimeOffset(2025, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var window = TimeSpan.FromMinutes(10); // 11:50 - 12:00
        var inner = new StubIntentModel(space =>
        {
            var actions = string.Join(",", space.Events.Select(e => e.Action).OrderBy(x => x));
            return new Intent("Stub", [], new IntentConfidence(0.7, "High"), actions);
        });

        var model = new SlidingWindowIntentModel(inner, window, refTime);

        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("user", "old", refTime.AddMinutes(-20))); // outside
        space.Observe(new BehaviorEvent("user", "recent", refTime.AddMinutes(-3))); // inside
        space.Observe(new BehaviorEvent("user", "now", refTime)); // inside

        var intent = model.Infer(space);

        Assert.Equal("Stub", intent.Name);
        Assert.Equal("now,recent", intent.Reasoning);
    }

    [Fact]
    public void SlidingWindow_WhenNoEventsInWindow_DelegatesWithFullSpace()
    {
        var refTime = new DateTimeOffset(2025, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var window = TimeSpan.FromMinutes(5); // 11:55 - 12:00
        var inner = new StubIntentModel(space =>
        {
            var count = space.Events.Count;
            return new Intent("Stub", [], new IntentConfidence(0.5, "Low"), $"count={count}");
        });

        var model = new SlidingWindowIntentModel(inner, window, refTime);

        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("user", "old", refTime.AddMinutes(-30)));

        var intent = model.Infer(space);

        Assert.Equal("Stub", intent.Name);
        Assert.Equal("count=1", intent.Reasoning);
    }

    [Fact]
    public void SlidingWindow_WhenEmptySpace_DelegatesToInner()
    {
        var refTime = new DateTimeOffset(2025, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var inner = new StubIntentModel(_ => new Intent("Empty", [], new IntentConfidence(0.0, "None"), "empty"));
        var model = new SlidingWindowIntentModel(inner, TimeSpan.FromMinutes(10), refTime);

        var space = new BehaviorSpace();
        var intent = model.Infer(space);

        Assert.Equal("Empty", intent.Name);
        Assert.Equal("empty", intent.Reasoning);
    }

    [Fact]
    public void SlidingWindow_PreservesMetadataOnWindowedSpace()
    {
        var refTime = new DateTimeOffset(2025, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var inner = new StubIntentModel(space =>
        {
            var sector = space.GetMetadata<string>("sector") ?? "none";
            return new Intent("Stub", [], new IntentConfidence(0.8, "High"), sector);
        });
        var model = new SlidingWindowIntentModel(inner, TimeSpan.FromMinutes(10), refTime);

        var space = new BehaviorSpace();
        space.SetMetadata("sector", "retail");
        space.Observe(new BehaviorEvent("user", "browse", refTime.AddMinutes(-2)));

        var intent = model.Infer(space);

        Assert.Equal("Stub", intent.Name);
        Assert.Equal("retail", intent.Reasoning);
    }

    private sealed class StubIntentModel : IIntentModel
    {
        private readonly Func<BehaviorSpace, Intent> _infer;

        public StubIntentModel(Func<BehaviorSpace, Intent> infer) => _infer = infer;

        public Intent Infer(BehaviorSpace behaviorSpace, BehaviorVector? precomputedVector = null)
            => _infer(behaviorSpace);
    }
}
