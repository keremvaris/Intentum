using System.Diagnostics;
using Intentum.Core.Intents;
using Intentum.Observability;
using Intentum.Runtime.Policy;

namespace Intentum.Tests.Observability;

/// <summary>
/// Tests for ObservablePolicyEngine with ActivityListener verification for spans and tags.
/// </summary>
[Collection("Observability")]
public sealed class ObservablePolicyEngineTests : IDisposable
{
    private readonly ActivityListener _listener;
    private readonly List<Activity> _capturedActivities = [];
    private Activity? _currentTestActivity;

    public ObservablePolicyEngineTests()
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

    private Activity StartTestScope([System.Runtime.CompilerServices.CallerMemberName] string testName = "")
    {
        _capturedActivities.Clear();
        _currentTestActivity = new Activity(testName).Start();
        return _currentTestActivity;
    }

    [Fact]
    public void DecideWithExecutionLog_CreatesActivityWithCorrectName()
    {
        using var scope = StartTestScope();
        var intent = new Intent("TestIntent", [], new IntentConfidence(0.8, "High"), "rule");
        var policy = new IntentPolicy([
            new PolicyRule("AllowHigh", i => i.Confidence.Level == "High", PolicyDecision.Allow)
        ]);

        var (decision, record) = intent.DecideWithExecutionLog(policy);

        var testActivities = GetTestActivities();
        Assert.Single(testActivities);
        Assert.Equal(IntentumActivitySource.PolicyEvaluateSpanName, testActivities[0].DisplayName);
    }

    [Fact]
    public void DecideWithExecutionLog_SetsDecisionTag()
    {
        using var scope = StartTestScope();
        var intent = new Intent("TestIntent", [], new IntentConfidence(0.8, "High"), "rule");
        var policy = new IntentPolicy([
            new PolicyRule("AllowHigh", i => i.Confidence.Level == "High", PolicyDecision.Allow)
        ]);

        var (decision, record) = intent.DecideWithExecutionLog(policy);

        var testActivities = GetTestActivities();
        Assert.Single(testActivities);
        Assert.Equal(PolicyDecision.Allow.ToString(), testActivities[0].GetTagItem("intentum.policy.decision"));
    }

    [Fact]
    public void DecideWithExecutionLog_SetsIntentNameTag()
    {
        using var scope = StartTestScope();
        var intent = new Intent("MyIntent", [], new IntentConfidence(0.7, "High"), "rule");
        var policy = new IntentPolicy([
            new PolicyRule("Allow", _ => true, PolicyDecision.Allow)
        ]);

        var (decision, record) = intent.DecideWithExecutionLog(policy);

        var testActivities = GetTestActivities();
        Assert.Single(testActivities);
        Assert.Equal("MyIntent", testActivities[0].GetTagItem("intentum.intent.name"));
    }

    [Fact]
    public void DecideWithExecutionLog_SetsConfidenceLevelTag()
    {
        using var scope = StartTestScope();
        var intent = new Intent("Test", [], new IntentConfidence(0.6, "Medium"), "rule");
        var policy = new IntentPolicy([
            new PolicyRule("Allow", _ => true, PolicyDecision.Allow)
        ]);

        var (decision, record) = intent.DecideWithExecutionLog(policy);

        var testActivities = GetTestActivities();
        Assert.Single(testActivities);
        Assert.Equal("Medium", testActivities[0].GetTagItem("intentum.intent.confidence.level"));
    }

    [Fact]
    public void DecideWithExecutionLog_WhenRuleMatches_SetsMatchedRuleTag()
    {
        using var scope = StartTestScope();
        var intent = new Intent("Test", [], new IntentConfidence(0.9, "High"), "rule");
        var policy = new IntentPolicy([
            new PolicyRule("SpecificRule", i => i.Confidence.Level == "High", PolicyDecision.Allow)
        ]);

        var (decision, record) = intent.DecideWithExecutionLog(policy);

        var testActivities = GetTestActivities();
        Assert.Single(testActivities);
        Assert.Equal("SpecificRule", testActivities[0].GetTagItem("intentum.policy.matched_rule"));
    }

    [Fact]
    public void DecideWithExecutionLog_WhenNoRuleMatches_NoMatchedRuleTag()
    {
        using var scope = StartTestScope();
        var intent = new Intent("Test", [], new IntentConfidence(0.3, "Low"), "rule");
        var policy = new IntentPolicy([
            new PolicyRule("HighOnly", i => i.Confidence.Level == "High", PolicyDecision.Allow)
        ]);

        var (decision, record) = intent.DecideWithExecutionLog(policy);

        var testActivities = GetTestActivities();
        Assert.Single(testActivities);
        Assert.Null(testActivities[0].GetTagItem("intentum.policy.matched_rule"));
    }

    [Fact]
    public void DecideWithMetrics_CreatesActivity()
    {
        using var scope = StartTestScope();
        var intent = new Intent("Test", [], new IntentConfidence(0.8, "High"), "rule");
        var policy = new IntentPolicy([
            new PolicyRule("Allow", _ => true, PolicyDecision.Allow)
        ]);

        var decision = intent.DecideWithMetrics(policy);

        var testActivities = GetTestActivities();
        Assert.Single(testActivities);
        Assert.Equal(IntentumActivitySource.PolicyEvaluateSpanName, testActivities[0].DisplayName);
    }

    [Fact]
    public void DecideWithExecutionLog_WhenExceptionThrown_SetsErrorStatus()
    {
        using var scope = StartTestScope();
        var intent = new Intent("Test", [], new IntentConfidence(0.8, "High"), "rule");
        var policy = new IntentPolicy([
            new PolicyRule("Throw", _ => throw new InvalidOperationException("Test error"), PolicyDecision.Block)
        ]);

        var (decision, record) = intent.DecideWithExecutionLog(policy);

        Assert.Equal(PolicyDecision.Observe, decision);
        Assert.False(record.Success);
        Assert.Equal("Test error", record.ExceptionMessage);
        Assert.NotNull(record.ExceptionTrace);

        var testActivities = GetTestActivities();
        Assert.Single(testActivities);
        Assert.Equal(ActivityStatusCode.Error, testActivities[0].Status);
        Assert.Contains("Test error", testActivities[0].StatusDescription);
    }

}