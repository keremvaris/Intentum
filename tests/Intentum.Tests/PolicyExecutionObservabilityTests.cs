using Intentum.Core.Intents;
using Intentum.Observability;
using Intentum.Runtime.Policy;

namespace Intentum.Tests;

/// <summary>
/// Tests for policy execution observability (execution log, intent correlation, failure trace).
/// </summary>
public sealed class PolicyExecutionObservabilityTests
{
    [Fact]
    public void DecideWithExecutionLog_ReturnsDecisionAndRecord_WithMatchedRuleAndDuration()
    {
        var intent = new Intent("TestIntent", [], new IntentConfidence(0.8, "High"), "rule");
        var policy = new IntentPolicy([
            new PolicyRule("AllowHigh", i => i.Confidence.Level == "High", PolicyDecision.Allow)
        ]);

        var (decision, record) = intent.DecideWithExecutionLog(policy);

        Assert.Equal(PolicyDecision.Allow, decision);
        Assert.True(record.Success);
        Assert.Equal("TestIntent", record.IntentName);
        Assert.Equal("AllowHigh", record.MatchedRuleName);
        Assert.Equal(PolicyDecision.Allow, record.Decision);
        Assert.True(record.DurationMs >= 0);
        Assert.Null(record.ExceptionMessage);
        Assert.Null(record.ExceptionTrace);
    }

    [Fact]
    public void DecideWithExecutionLog_WhenNoRuleMatches_RecordHasNullMatchedRule()
    {
        var intent = new Intent("Unknown", [], new IntentConfidence(0.2, "Low"));
        var policy = new IntentPolicy([
            new PolicyRule("BlockLow", i => i.Confidence.Level == "High", PolicyDecision.Block)
        ]);

        var (decision, record) = intent.DecideWithExecutionLog(policy);

        Assert.Equal(PolicyDecision.Observe, decision);
        Assert.True(record.Success);
        Assert.Null(record.MatchedRuleName);
        Assert.Equal(PolicyDecision.Observe, record.Decision);
    }

    [Fact]
    public void DecideWithMetrics_ReturnsSameDecisionAsDecideWithExecutionLog()
    {
        var intent = new Intent("X", [], new IntentConfidence(0.7, "High"));
        var policy = new IntentPolicy([
            new PolicyRule("Allow", _ => true, PolicyDecision.Allow)
        ]);

        var decisionFromMetrics = intent.DecideWithMetrics(policy);
        var (decisionFromLog, _) = intent.DecideWithExecutionLog(policy);

        Assert.Equal(decisionFromLog, decisionFromMetrics);
        Assert.Equal(PolicyDecision.Allow, decisionFromMetrics);
    }

    [Fact]
    public void DecideWithExecutionLog_WhenEvaluationThrows_ReturnsObserveAndRecordWithException()
    {
        var intent = new Intent("X", [], new IntentConfidence(0.7, "High"));
        var policy = new IntentPolicy([
            new PolicyRule("Throw", _ => throw new InvalidOperationException("policy error"), PolicyDecision.Block)
        ]);

        var (decision, record) = intent.DecideWithExecutionLog(policy);

        Assert.Equal(PolicyDecision.Observe, decision);
        Assert.False(record.Success);
        Assert.Equal("X", record.IntentName);
        Assert.NotNull(record.ExceptionMessage);
        Assert.Contains("policy error", record.ExceptionMessage);
        Assert.NotNull(record.ExceptionTrace);
    }
}
