using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;
using Intentum.Observability;

namespace Intentum.Tests;

/// <summary>
/// Tests for ObservableIntentModel: Infer success/exception paths, inner model delegation.
/// </summary>
public sealed class ObservableIntentModelTests
{
    [Fact]
    public void Constructor_WhenInnerNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ObservableIntentModel(null!));
    }

    [Fact]
    public void Infer_WhenInnerReturnsIntent_ReturnsSameIntent()
    {
        var expected = new Intent("X", [], new IntentConfidence(0.9, "High"), "rule");
        var inner = new StubIntentModel(expected);
        var observable = new ObservableIntentModel(inner);
        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("u", "a", DateTimeOffset.UtcNow));

        var result = observable.Infer(space);

        Assert.Equal(expected.Name, result.Name);
        Assert.Equal(expected.Confidence.Score, result.Confidence.Score);
        Assert.Equal(expected.Confidence.Level, result.Confidence.Level);
    }

    [Fact]
    public void Infer_WhenInnerThrows_PropagatesException()
    {
        var inner = new ThrowingIntentModel();
        var observable = new ObservableIntentModel(inner);
        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("u", "a", DateTimeOffset.UtcNow));

        var ex = Assert.Throws<InvalidOperationException>(() => observable.Infer(space));
        Assert.Contains("inner", ex.Message);
    }

    [Fact]
    public void Infer_WithEmptyBehaviorSpace_CallsInner()
    {
        var expected = new Intent("Empty", [], new IntentConfidence(0, "None"), "rule");
        var inner = new StubIntentModel(expected);
        var observable = new ObservableIntentModel(inner);
        var space = new BehaviorSpace();

        var result = observable.Infer(space);

        Assert.Equal("Empty", result.Name);
    }

    [Fact]
    public void Infer_WithPrecomputedVector_PassesToInner()
    {
        var expected = new Intent("V", [], new IntentConfidence(0.5, "Medium"), "rule");
        var inner = new StubIntentModel(expected);
        var observable = new ObservableIntentModel(inner);
        var space = new BehaviorSpace();
        var vector = new BehaviorVector(new Dictionary<string, double> { ["x"] = 1.0 });

        var result = observable.Infer(space, vector);

        Assert.Equal("V", result.Name);
    }

    private sealed class StubIntentModel : IIntentModel
    {
        private readonly Intent _intent;

        public StubIntentModel(Intent intent) => _intent = intent;

        public Intent Infer(BehaviorSpace behaviorSpace, BehaviorVector? precomputedVector = null) => _intent;
    }

    private sealed class ThrowingIntentModel : IIntentModel
    {
        public Intent Infer(BehaviorSpace behaviorSpace, BehaviorVector? precomputedVector = null)
            => throw new InvalidOperationException("inner");
    }
}
