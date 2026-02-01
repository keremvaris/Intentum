using System.Diagnostics;
using Intentum.Core.Intents;
using Intentum.Runtime.Engine;
using Intentum.Runtime.Policy;

namespace Intentum.Observability;

/// <summary>
/// Extension methods for adding observability to policy decisions (metrics, OpenTelemetry spans, execution log).
/// </summary>
public static class ObservablePolicyEngine
{
    /// <summary>
    /// Decides on an intent with policy, records metrics, and creates a policy.evaluate span.
    /// </summary>
    public static PolicyDecision DecideWithMetrics(
        this Intent intent,
        IntentPolicy policy)
    {
        var (decision, _) = DecideWithExecutionLog(intent, policy);
        return decision;
    }

    /// <summary>
    /// Decides on an intent with policy, records metrics and span, and returns an execution record for logging (matched rule, intent name, decision, duration). On exception, the record contains Success = false and ExceptionMessage; log the record and exception trace.
    /// </summary>
    public static (PolicyDecision Decision, PolicyExecutionRecord Record) DecideWithExecutionLog(
        this Intent intent,
        IntentPolicy policy)
    {
        var sw = Stopwatch.StartNew();
        PolicyDecision decision;
        PolicyRule? matchedRule = null;
        var success = true;
        string? exceptionMessage = null;

        try
        {
            using var activity = IntentumActivitySource.Source.StartActivity();

            (decision, matchedRule) = IntentPolicyEngine.EvaluateWithRule(intent, policy);

            if (activity != null)
            {
                activity.DisplayName = IntentumActivitySource.PolicyEvaluateSpanName;
                activity.SetTag("intentum.policy.decision", decision.ToString());
                activity.SetTag("intentum.intent.name", intent.Name);
                activity.SetTag("intentum.intent.confidence.level", intent.Confidence.Level);
                if (matchedRule != null)
                    activity.SetTag("intentum.policy.matched_rule", matchedRule.Name);
            }

            IntentumMetrics.RecordPolicyDecision(decision);
        }
        catch (Exception ex)
        {
            success = false;
            exceptionMessage = ex.Message;
            decision = PolicyDecision.Observe;
            sw.Stop();
            var record = new PolicyExecutionRecord(
                intent.Name,
                matchedRule?.Name,
                decision,
                sw.Elapsed.TotalMilliseconds,
                success,
                exceptionMessage,
                ExceptionTrace: ex.StackTrace);
            return (decision, record);
        }

        sw.Stop();
        var successRecord = new PolicyExecutionRecord(
            intent.Name,
            matchedRule?.Name,
            decision,
            sw.Elapsed.TotalMilliseconds,
            success,
            exceptionMessage);

        return (decision, successRecord);
    }
}
