using System.Diagnostics;
using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;
using Intentum.Observability;
using Moq;

namespace Intentum.Tests.Observability;

/// <summary>
/// Tests for ObservableIntentModel with ActivityListener verification for spans and tags.
/// </summary>
[Collection("Observability")]
public sealed class ObservableIntentModelTests : IDisposable
{
    private readonly ActivityListener _listener;
    private readonly List<Activity> _capturedActivities = [];

    public ObservableIntentModelTests()
    {
        _listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Intentum",
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity => _capturedActivities.Add(activity)
        };
        ActivitySource.AddActivityListener(_listener);
        _capturedActivities.Clear();
    }

    public void Dispose()
    {
        _listener.Dispose();
    }

    private List<Activity> GetTestActivities() =>
        _capturedActivities.Where(a => a.ParentId == _currentTestActivity?.Id).ToList();

    private Activity? _currentTestActivity;

    private Activity StartTestScope([System.Runtime.CompilerServices.CallerMemberName] string testName = "")
    {
        _capturedActivities.Clear();
        _currentTestActivity = new Activity(testName).Start();
        return _currentTestActivity;
    }

    [Fact]
    public void Infer_CreatesActivityWithCorrectName()
    {
        using var scope = StartTestScope();
        var inner = new Mock<IIntentModel>();
        inner.Setup(m => m.Infer(It.IsAny<BehaviorSpace>(), null))
            .Returns(new Intent("TestIntent", [], new IntentConfidence(0.8, "High")));

        var observable = new ObservableIntentModel(inner.Object);
        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("user", "login", DateTimeOffset.UtcNow));

        var result = observable.Infer(space);

        var testActivities = GetTestActivities();
        Assert.Single(testActivities);
        Assert.Equal(IntentumActivitySource.InferSpanName, testActivities[0].DisplayName);
    }

    [Fact]
    public void Infer_SetsIntentNameTag()
    {
        using var scope = StartTestScope();
        var inner = new Mock<IIntentModel>();
        inner.Setup(m => m.Infer(It.IsAny<BehaviorSpace>(), null))
            .Returns(new Intent("MyIntent", [], new IntentConfidence(0.9, "High")));

        var observable = new ObservableIntentModel(inner.Object);
        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("user", "action", DateTimeOffset.UtcNow));

        var result = observable.Infer(space);

        var testActivities = GetTestActivities();
        Assert.Single(testActivities);
        Assert.Equal("MyIntent", testActivities[0].GetTagItem("intentum.intent.name"));
    }

    [Fact]
    public void Infer_SetsConfidenceTags()
    {
        using var scope = StartTestScope();
        var inner = new Mock<IIntentModel>();
        inner.Setup(m => m.Infer(It.IsAny<BehaviorSpace>(), null))
            .Returns(new Intent("Test", [], new IntentConfidence(0.75, "Medium")));

        var observable = new ObservableIntentModel(inner.Object);
        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("user", "action", DateTimeOffset.UtcNow));

        var result = observable.Infer(space);

        var testActivities = GetTestActivities();
        Assert.Single(testActivities);
        Assert.Equal("Medium", testActivities[0].GetTagItem("intentum.intent.confidence.level"));
        Assert.Equal(0.75, testActivities[0].GetTagItem("intentum.intent.confidence.score"));
    }

    [Fact]
    public void Infer_SetsBehaviorEventCountTag()
    {
        using var scope = StartTestScope();
        var inner = new Mock<IIntentModel>();
        inner.Setup(m => m.Infer(It.IsAny<BehaviorSpace>(), null))
            .Returns(new Intent("Test", [], new IntentConfidence(0.5, "Low")));

        var observable = new ObservableIntentModel(inner.Object);
        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("user", "action1", DateTimeOffset.UtcNow));
        space.Observe(new BehaviorEvent("user", "action2", DateTimeOffset.UtcNow));
        space.Observe(new BehaviorEvent("user", "action3", DateTimeOffset.UtcNow));

        var result = observable.Infer(space);

        var testActivities = GetTestActivities();
        Assert.Single(testActivities);
        Assert.Equal(3, testActivities[0].GetTagItem("intentum.behavior.event.count"));
    }

    [Fact]
    public void Infer_WhenInnerThrows_SetsErrorStatus()
    {
        using var scope = StartTestScope();
        var inner = new Mock<IIntentModel>();
        inner.Setup(m => m.Infer(It.IsAny<BehaviorSpace>(), null))
            .Throws(new InvalidOperationException("Specific error message"));

        var observable = new ObservableIntentModel(inner.Object);
        var space = new BehaviorSpace();

        var ex = Assert.Throws<InvalidOperationException>(() => observable.Infer(space));
        Assert.Equal("Specific error message", ex.Message);

        var testActivities = GetTestActivities();
        Assert.Single(testActivities);
        Assert.Equal(ActivityStatusCode.Error, testActivities[0].Status);
        Assert.Contains("Specific error message", testActivities[0].StatusDescription);
    }

}